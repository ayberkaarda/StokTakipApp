# StokTakip Tehdit Modeli

Sürüm: 1.0 (kod durumu: `3964a72`)
Kapsam: `src/StokTakip.Domain`, `Application`, `Data`, `UI`; `tests/**`; `appsettings.json`; log dosyaları; CSV dışa aktarım; SQL Server şeması.

Bu belge mantık ve zincir odaklıdır: tek tek kod hatalarından çok, "hangi adımlar arka arkaya gelirse ne olur" sorusuna cevap verir. Önceki kod denetiminde kayda geçen bulgular (çağıran kimliğinin parametreyle verilmesi, oturumun uzaktan iptal edilmemesi, süreç içinden dekoratör atlanması, çıkış/giriş yarışı, kilitleme ile hesap kapatma, kilitli hesapla kullanıcı numaralandırma, `TrustServerCertificate`, CSV dosyasının yetki denetiminden önce sıfırlanması, log/ hata mesajı ifşası, `.gitignore` eksikleri) burada yeniden anlatılmaz; yalnızca bir zincirin halkası olduğunda adıyla anılır.

---

## 1. Yönetici özeti

1. **Uygulamadaki rol modeli, veritabanı tarafında karşılığı olmayan bir "süreç içi" kontroldür.** Bağlantı `Trusted_Connection=True` ile kurulur; yani SQL Server'a bağlanan kimlik, uygulamayı çalıştıran Windows hesabıdır. Operator rolündeki bir çalışanın Windows hesabı SQL Server'da yazma hakkına sahipse (SQLEXPRESS kurulumlarında varsayılan budur), `Kullanicilar` tablosunu düzenleyerek kendini Admin yapabilir, kilidini açabilir, parolasını değiştirebilir. `RowVersion` ve CHECK kısıtları bunu engellemez; CHECK `Iterasyon > 0` ile 1 iterasyonlu bir özet bile kabul edilir ve ilk başarılı girişte uygulama bu özeti sessizce güçlü özete çevirir (iz silinir). Bu, belgedeki en yüksek riskli zincirdir (Z1).
2. **Stok hareketlerinin kaydı yoktur.** Stok "mutlak değer" olarak yazılır (`StokGuncelleAsync(urunId, yeniStok)`), bir hareket defteri (giriş/çıkış/düzeltme, kim, ne zaman, neden) tutulmaz. Loglarda işlemi yapan kullanıcı yoktur. Operator'un fiziksel ürünü alıp stoğu düşürmesi tespit edilemez (Z2).
3. **Sipariş silme, araya giren sayım düzeltmelerini hesaba katmaz.** Silme, siparişin adedini *bugünkü* stoğa ekler. Araya `StokGuncelle` ile bir sayım girmişse stok fiziksel miktarın üstüne çıkar. Silinen siparişler raporlardan ve geçmişten tamamen kaybolur; kimin, ne zaman sildiği kaydedilmez (Z3).
4. **Sipariş tarihi istemcinin saatinden alınır** (`TimeProvider.GetLocalNow()`); veritabanındaki `GETDATE()` yalnızca varsayılandır. Saati geri alan bir istemci kapanmış döneme sipariş yazabilir; günlük/aylık raporlar ve CSV filtresi bu tarihe güvenir (Z4).
5. **Düşük stok eşiği Operator yetkisiyle değiştirilebilir.** Eşik 0 yapılan ürün, stoğu 1 bile olsa "Yeterli" görünür; uyarı sayacı ve "Düşük/Kritik" filtresi onu göstermez (Z5).
6. **Giriş ve ilk kurulum ekranı henüz arayüzde yok.** Üretim DI'ı tüm servisleri dekoratörle sarar ve oturum olmadığı için her çağrı `YetkisizIslemException` verir; uygulama bugün "kapalı" durumda açılır. Bu güvenli bir varsayılan, ancak ilk Admin'in arayüzden değil elle SQL ile ekleneceği ve geçici olarak `YetkiDenetimiYok`'un üretime sızabileceği anlamına gelir (Z6).

Öncelik sırası: §9'da.

---

## 2. Varlıklar ve güven sınırları

| # | Varlık | Nerede | Kim erişir | Güven sınırı / not |
|---|---|---|---|---|
| V1 | Parola özetleri, tuz, iterasyon | `Kullanicilar` tablosu | Uygulama; DB'ye bağlanabilen her Windows hesabı | Özet güçlü (PBKDF2-SHA256, 600k). Tablo satırını yazabilen herkes özeti *değiştirebilir*; kırmasına gerek yok. |
| V2 | Rol ve aktiflik (`Rol`, `Aktif`, `KilitBitis`, `BasarisizDenemeSayisi`) | `Kullanicilar` | Aynı | CHECK yalnızca aralık doğrular (`Rol IN (0,1)`), kaynağı doğrulamaz. |
| V3 | Stok miktarı ve eşik | `Urunler.Stok`, `DusukStokEsigi` | Operator (uygulama), DB yazıcısı | `Stok >= 0` CHECK var; hareket kaydı yok. |
| V4 | Sipariş kayıtları (tarih, adet, IsDeleted) | `Siparisler` | Operator oluşturur, Admin siler | Yumuşak silme; silinen kayıt arayüzden ve raporlardan görünmez, kim sildiği yok. |
| V5 | Raporlar, CSV çıktısı | Bellek / kullanıcının seçtiği dosya | Operator rapor görür, Admin CSV alır | CSV, Excel'de açılır; hücre kaçışı var. Dosya kullanıcı diskinde korumasız. |
| V6 | Bağlantı dizesi, Serilog ayarları | `appsettings.json` (uygulama dizini) | Dizine yazabilen herkes | Parola içermez; ama sunucu adı ve log yolu buradan değiştirilebilir. |
| V7 | Log dosyaları | `%LocalAppData%\StokTakip\logs` (ayarla başka yere taşınabilir) | O Windows kullanıcısı | Yalnız KullaniciId ve iş verisi (UrunId, Adet); işlemi yapan kullanıcı yok. 14 gün. |
| V8 | Oturum (`OturumBaglami`) | Süreç belleği, singleton | Süreç | Zaman aşımı yok; kapanışa kadar sürer. |
| V9 | Uygulama ikilileri ve DI yapılandırması | Uygulama dizini | Yükleyen; dizine yazabilen | İmzasız; `YetkiDenetimiYok` sınıfı üretim derlemesinin içinde. |

Güven sınırları:

- **S1 — Masaüstü istemci ↔ SQL Server (ağ):** TDS üzerinden; SqlClient varsayılanı şifreli, `TrustServerCertificate=True` sertifika doğrulamasını kapatır. Ağ saldırganı sunucuyu taklit edebilir veya araya girebilir.
- **S2 — Uygulama kontrolleri ↔ veritabanı hakları:** Uygulama Operator/Admin ayrımı yapar; veritabanı yapmaz. Bu sınır **gerçekte yoktur**: DB'de yazma hakkı olan her kullanıcı için uygulama kontrolleri önerilerden ibarettir.
- **S3 — Kullanıcı ↔ uygulama dizini/dosya sistemi:** `appsettings.json`, log yolu, CSV hedefi kullanıcı denetimindedir.
- **S4 — Oturum açmış kullanıcı ↔ sunucu durumu:** Rol ve aktiflik girişte okunur, bellekte kalır; sunucudaki değişiklik diğer makinelere yansımaz.

---

## 3. STRIDE tablosu

Şiddet: Y (yüksek), O (orta), D (düşük). "Mevcut azaltım" kodda bugün bulunanı anlatır.

| Kategori | Tehdit | Senaryo | Etki | Mevcut azaltım | Kalan risk | Öneri |
|---|---|---|---|---|---|---|
| **S**poofing | Kullanicilar satırı üzerinden kimlik ele geçirme | DB yazma hakkı olan Operator, hedef Admin'in `ParolaHash/ParolaTuzu/Iterasyon` alanlarını kendi bildiği bir parolanın özetiyle değiştirir (Iterasyon=1 bile geçerlidir), sonra o hesapla girer | Tam yetki, kurbanın adıyla iz | CHECK `Iterasyon > 0`; RowVersion (yalnız iyimser eşzamanlılık) | Y | §9 madde 1: DB haklarını ayır; `Iterasyon >= 600000` CHECK'i; uygulama tarafında `Iterasyon < MinimumIterasyon` ise *yeniden hash yerine* uyarı + log; giriş sonrası "son parola değişimi" alanıyla tutarlılık denetimi |
| S | Ağdan sunucu taklidi | S1'de araya giren proxy, `TrustServerCertificate=True` sayesinde TLS'yi sonlandırır, `SELECT ... FROM Kullanicilar` sonucunda `Rol` ve `Aktif` sütunlarını değiştirip iletir | Yerel Admin oturumu; sonraki yazmalar gerçek DB'ye gider | Yok (Windows kimliği NTLM ile aktarılabilir) | O (yalnız uzak sunucu kurulumlarında) | `Encrypt=Strict`/`Mandatory` + geçerli sertifika; `TrustServerCertificate=False`; Kerberos zorunlu |
| S | İlk kurulumun yeniden açılması | `Kullanicilar` boşaltılırsa (eski yedekten dönüş, yanlış migration, kasıtlı DELETE) `IlkKurulumGerekliMiAsync` true döner; uygulamayı ilk açan kişi Admin olur | Yetkisiz Admin | İlk kurulum yalnız tablo boşken; Serializable işlem | O | Kurulum tamamlandı bayrağını ayrı bir `Ayarlar` tablosunda tut; tablo boş ama bayrak doluysa ilk kurulumu reddet ve logla |
| **T**ampering | Stokun izsiz değiştirilmesi | Operator `StokGuncelle` ile stoğu istediği değere çeker; hareket kaydı yok, logda kullanıcı yok | Hırsızlık gizleme, envanter güvenilmezliği | Stok ≥ 0; RowVersion (yalnız çakışma) | Y | Hareket defteri (`StokHareketleri`: ürün, delta, tür, kullanıcı, zaman, açıklama); mutlak set yerine "sayım düzeltmesi" işlemi ve zorunlu açıklama; Operator için günlük düzeltme limiti |
| T | Silme ile stok şişirme | Sipariş sonrası sayım düzeltmesi yapılmış ürünün siparişi silinir; adet tekrar eklenir | Fiziksel stoktan fazla görünen stok, hatalı sipariş kabulü | Çift silmeye karşı RowVersion | O | Silme sırasında siparişten sonra stok düzeltmesi varsa iadeyi yapma ya da onay iste; iadeyi hareket defterine yaz |
| T | Sipariş tarihinin istemciden gelmesi | İstemci saatini geriye alır, kapanmış döneme sipariş yazar; ya da ileriye alır | Rapor/dönem manipülasyonu, CSV filtresi aldanır | Yok (DB `GETDATE()` yalnız varsayılan) | O | `Tarih`'i uygulamadan gönderme, DB varsayılanına bırak (ya da `SYSUTCDATETIME()`); ileri tarihli kayıt için CHECK yok, uygulama tarafında doğrula |
| T | Tarihsel rapor etiketlerinin değişmesi | Operator ürünü yeniden adlandırır; rapor ve CSV *güncel adı* gösterir | Geçmiş raporların anlamı değişir | Yok | D | Sipariş satırına ürün adını kopyala (snapshot) ya da raporlarda UrunId'yi de göster (zaten var); ad değişikliğini logla |
| T | `appsettings.json` ile yönlendirme | Dizine yazabilen kullanıcı sunucu adını kendi denetimindeki SQL Server'a çevirir; orada boş DB → kendini Admin yapar, ya da log yolunu paylaşımlı klasöre çevirir | Yerel sahte ortam; log sızıntısı | Yok | D (gerçek veriye erişim sağlamaz) | Dosya için yalnızca yöneticilerin yazabildiği kurulum dizini; sunucu adını kayıt defteri/GPO'dan okuma seçeneği |
| T | Kategori silme ile sınıflandırma kaybı | Admin kategoriyi siler; ürünler sessizce "Kategorisiz" olur | Yanlış filtreleme | Arayüz varsayılanı "Tüm kategoriler"; uyarı sayacı kategori filtresinden bağımsız | D | Silmede ürün sayısını göster ve onay al (arayüz) |
| **R**epudiation | İşlemi yapanın kaydedilmemesi | Stok/sipariş/kategori loglarında kullanıcı yok; `IslemGunlugu` yalnız işlem adı ve mesaj yazar | Z2, Z3'te sorumlu bulunamaz | Kimlik işlemlerinde KullaniciId var | Y | Dekoratörlerde `IOturumBaglami.MevcutKullanici.KullaniciId`'yi log kapsamına (`BeginScope`) ekle; kritik işlemleri DB'de `Denetim` tablosuna yaz |
| R | Yumuşak silinen siparişin görünmez olması | Silme zamanı/kullanıcısı tutulmaz; arayüzde silinenleri gösteren ekran yok | İtiraz edilemez | `IsDeleted` korunur, fiziksel silinmez | O | `SilinmeZamani`, `SilenKullaniciId` sütunları; Admin'e "silinenleri göster" |
| R | Log dosyasının kullanıcı denetiminde olması | Loglar kullanıcının kendi profilinde; silinebilir, düzenlenebilir | Yerel iz kaybı | Yok | O | Önemli olayları DB'ye de yaz (yukarıdaki `Denetim`); log dizini için salt-ekleme ACL kurulum adımı |
| **I**nfo disclosure | CSV dosyasının korumasız olması | Admin tüm sipariş geçmişini masaüstüne/USB'ye yazar | Ticari veri sızıntısı | Yalnız Admin | D (kabul, §8) | Dışa aktarımı logla (dosya adı, satır sayısı, kullanıcı) |
| I | Operator'un tüm geçmişi görmesi | `GecmisVeRaporGoruntule` Operator'a açık; CSV kapalı ama ekran görüntüsü alınabilir | Sınırlı | — | D (kabul) | Gerekirse tarih aralığı sınırı |
| **D**enial of service | İlk kurulum yarışında kilitlenme (deadlock) | İki istemci aynı anda ilk Admin'i oluşturur: Serializable altında iki `SELECT` aralık kilidi alır, iki `INSERT` birbirini bekler; SQL Server birini kurban seçer (1205) | Kurban `SqlException` görür ("Beklenmeyen hata"); tek Admin garantisi korunur | Serializable işlem + ikinci kontrol | D | 1205'i yakalayıp "İlk kurulum zaten tamamlanmış" mesajına çevir; §7 T6 testi |
| D | İki Admin'in birbirini aynı anda düşürmesi | A, B'yi; B, A'yı Operator yapar; her ikisi `AktifAdminSayisi = 2` okur | Serializable aralık kilitleri işlemleri sıraya sokar; biri deadlock kurbanı olur, sistemde ≥1 Admin kalır | Serializable işlem | D | 1205'i yakalayıp yeniden dene; §7 T5 testi |
| D | Stok taşması | `StokEkle` `checked` toplama kullanır; stok `int.MaxValue` yakınındayken sipariş silme `OverflowException` fırlatır | Sipariş silinemez | — | D | Stok için üst sınır (ör. 1.000.000) domain kuralı + CHECK |
| D | Toplu veri üretimi | Operator sınırsız ürün/sipariş ekler | Rapor yavaşlar | Sayfa boyutu 200, rapor aralığı 5 yıl | D (kabul) | — |
| **E**levation of privilege | DB hakkıyla rol yükseltme | `UPDATE Kullanicilar SET Rol=1, Aktif=1, KilitBitis=NULL, BasarisizDenemeSayisi=0 WHERE KullaniciAdi='ali'`; ya da yeni Admin satırı ekleme | Tam yetki | CHECK aralıkları | Y | §9 madde 1 |
| E | `YetkiDenetimiYok`'un üretimde kullanılması | Arayüze giriş eklenirken geçici olarak DI'da `YetkiDenetimiYok` kaydedilir ve kalır | Tüm roller eşit | `YetkiUctanUcaTestleri` üretim DI'ının `OturumYetkiDenetcisi` ve dekoratörleri kaydettiğini doğrular | O | Testi "UI'ın `Program.ServisleriOlustur` çıktısı" üzerinde de çalıştır (bugün yalnız `AddApplication+AddData` test ediliyor) |
| E | Son Admin korumasının hesap kurtarma yolu olmaması | Tek Admin parolasını unutur / hesabı kilitlenir; kurtarma yolu yalnız DB'ye elle müdahale | Z1'deki yöntem "normal işlem" haline gelir | Kilit 15 dk'da kendiliğinden açılır | O | Admin'in başkasının parolasını sıfırlaması (zorunlu değişimle) ve kilit açma işlevi; çevrimdışı kurtarma için belgelenmiş, loglanan bir komut |

---

## 4. İş mantığı kötüye kullanımı (ayrıntı)

### 4.1 Stok manipülasyonu
- `StokGuncelleAsync` ve `UrunGuncelleAsync` stoğu mutlak değer olarak yazar; ikisi de Operator'a açık.
- Yalnız `UrunId` ve `YeniStok` loglanır; işlemi yapan kullanıcı, önceki değer ve neden kaydedilmez.
- Sonuç: Operator "sayım" bahanesiyle her gün stoğu düşürebilir; fark hiçbir raporda görünmez (raporlar sipariş tablosundan beslenir, stoktan değil).

### 4.2 Sipariş silme ile stok şişirme
- `Siparis.Sil(urun)` → `urun.StokEkle(Adet)`: iade, siparişin açıldığı andaki stoğa değil bugünkü stoğa yapılır.
- Araya giren `StokGuncelle` (sayım) iadeyi geçersizleştirir; sistem fiziksel miktarın üstüne çıkar.
- Silinen sipariş `HasQueryFilter(!IsDeleted)` ile tüm sorgulardan düşer; raporlar o satışı hiç yaşanmamış sayar. Silme izi yoktur.

### 4.3 Yarış koşulları ve RowVersion
- `Urun.RowVersion` ve `Siparis.RowVersion` iyimser eşzamanlılık belirtecidir; iki istemcinin son stoğu aynı anda satması ve aynı siparişin iki kez silinmesi engellenir (testleri var).
- Boşluk: Sipariş oluşturma `ReadCommitted` işlemde `GetByIdAsync` → `StoktanDus` → `SaveChanges` yapar; çakışma `EszamanlilikException` ile kullanıcıya düşer, otomatik yeniden deneme yok (kullanılabilirlik, güvenlik değil).
- Kimlik tarafında giriş döngüsü 10 kez yeniden okur; `Kullanici.RowVersion` sayesinde eşzamanlı yanlış denemeler kaybolmaz (testleri var).
- Testler SQLite üzerinde çalışır; SQL Server'a özgü aralık kilidi/deadlock davranışları (§3 D satırları) hiçbir testte yok.

### 4.4 Yumuşak silme ve yetki
- Ürün silme `HasSiparisAsync` ile silinen siparişleri de sayar; sipariş geçmişi olan ürün hiç silinemez (iyi).
- Boşluk: Silinen siparişi görmenin/geri almanın yolu yok; "kaza ile sildim" durumu da, "kasıtlı sildim" durumu da aynı görünür.

### 4.5 Kategori ve eşik ile uyarı baskılama
- `DusukStokEsigi` Operator yetkisiyle 0 yapılabilir; `StokDurumunuHesapla(1, 0)` → `Yeterli`. Ürün uyarı sayacından ve düşük stok filtresinden çıkar.
- Kategori silme (Admin) ürünleri kategorisiz yapar ama uyarı sayacı tüm ürünleri sayar; baskılama olmaz.
- Öneri: eşik değişikliğini `UrunEkleGuncelle`'den ayırıp Admin'e ver (`Yetki.EsikDuzenle`), değişikliği logla; "eşik 0" için arayüzde uyarı.

### 4.6 Rapor verisi manipülasyonu
- Raporlar `Siparisler` üzerinden `Tarih` ile gruplanır; `Tarih` istemci saatidir (4.7 Z4).
- Yeniden adlandırılan ürün, geçmiş raporlarda yeni adıyla çıkar.
- Silinen sipariş rapordan düşer. Admin "dönem kapanışından sonra sil" yaparak satış toplamını değiştirebilir; kayıt yok.

---

## 5. Yetki durum makinesi

Durumlar (sunucu tarafı, `Kullanicilar` satırı): **Yok** → **Aktif-Operator** / **Aktif-Admin**; her aktif durumun **Kilitli** (15 dk) alt durumu; **Pasif**.
Durumlar (istemci, `OturumBaglami`): **Oturum yok** → **Oturum(Operator)** / **Oturum(Admin)**.

| Geçiş | Kim tetikler | Kontrol | Boşluk |
|---|---|---|---|
| Yok → Aktif-Admin (ilk kurulum) | Herkes, tablo boşken | İki kez `Any`, Serializable | Tablo boşalınca yeniden açılır; iki istemci yarışında kurban SqlException görür (§3). Arayüzde bu ekran henüz yok → ilk Admin SQL ile elle eklenecek. |
| Yok → Aktif-X (kullanıcı oluşturma) | Aktif Admin | `cagiranKullaniciId` DB'den doğrulanır | Çağıran kimliği parametreyle gelir (önceki denetimde kayıtlı). |
| Aktif → Kilitli | 5 yanlış deneme | Domain kuralı, RowVersion ile sayaç kaybolmaz | Admin kilidi açamaz; yalnızca süre. |
| Kilitli → Aktif | 15 dk sonra ilk işlem | `KilitliMi(simdiUtc)` | Zaman istemciden (`TimeProvider.System`); saat ileri alınmış istemci kilitli hesabı erken açar (`KilitBitis` ile karşılaştırma istemci UTC'siyle). |
| Aktif-Operator ↔ Aktif-Admin | Aktif Admin | Son aktif Admin düşürülemez | Hedefin diğer makinedeki oturumu değişmez (önceki denetim). Kendini düşürme serbest (başka Admin varsa). |
| Aktif → Pasif | Aktif Admin | Son aktif Admin pasifleştirilemez | Pasif kullanıcının açık oturumu başka makinede sürer (önceki denetim). |
| Oturum yok → Oturum(X) | Doğru parola | PBKDF2, sahte doğrulama, kilit | Oturum zaman aşımı yok; makine başında bırakılan açık Admin oturumu süresizdir. |
| Oturum(X) → Oturum yok | `CikisYap`, pasifleştirme (yerel) | — | Parola değişimi oturumları düşürmez. |
| Parola değişimi | Hesap sahibi (eski parola ile) | Politika, kilit | Admin'in başkasının parolasını sıfırlaması yok → kurtarma için DB'ye elle müdahale (Z1 yöntemini meşrulaştırır). |

İstemci saatine bağımlılık: `KilitliMi` ve sipariş tarihi istemci saatini kullanır. Veritabanı saati alınıp (`SELECT SYSUTCDATETIME()`) karşılaştırma onunla yapılmalı ya da kilit süresi kontrolü SQL tarafında yapılmalı.

---

## 6. Saldırı zincirleri

### Z1 — Operator'dan Admin'e: veritabanı üzerinden (Y)
Ön koşul: Operator'un Windows hesabı SQL Server'da `db_datawriter` veya üstü (SQLEXPRESS/LocalDB kurulumlarında yaygın).
1. SSMS/`sqlcmd` ile bağlanır; `Trusted_Connection` olduğu için parola gerekmez.
2. `UPDATE Kullanicilar SET Rol = 1 WHERE KullaniciAdi = 'ali'`. CHECK geçer (1 geçerli).
3. Uygulamayı açar, kendi parolasıyla girer; `OturumAc(..., Admin)`.
4. İstediği siparişi siler, CSV alır, başka Admin oluşturur, gerçek Admin'i pasifleştirir (son Admin koruması kendisi Admin olduğu için engellemez).
5. İzleri kapatmak için `Rol = 0`'a döner. Logda yalnız `KullaniciId` ile "Sipariş silindi" satırı kalır; o da kendi makinesindeki dosyada.
Varyant: Admin'in `ParolaHash/ParolaTuzu/Iterasyon=1` alanlarını kendi seçtiği parolanın özetiyle değiştirir; girişte uygulama özeti 600k iterasyona yükseltir, Admin bir sonraki girişinde parolası değiştiği için şüphelenir ama satır zaten "normal" görünür.
Azaltım: Uygulama kullanıcılarının Windows hesaplarına yalnızca uygulamanın kullandığı nesnelere `EXECUTE`/DML ver, `Kullanicilar`'a doğrudan `UPDATE/INSERT/DELETE` **DENY**; kullanıcı yönetimi işlemlerini saklı yordamla yap ve yordam içinde değişikliği denetim tablosuna yaz. Alternatif, daha küçük adım: `CK_Kullanicilar_Iterasyon` → `>= 600000`; `Kullanicilar` üzerinde `UPDATE` tetikleyicisiyle `Rol/Aktif/ParolaHash` değişikliklerini `KullaniciDenetim` tablosuna kopyala; uygulama açılışta son 24 saatteki değişiklikleri Admin'e göster. SQL Server Audit varsa `Kullanicilar` için etkinleştir.

### Z2 — Hırsızlığın stok düzeltmesiyle gizlenmesi (Y)
1. Operator depodan 3 ürün alır.
2. Ana formda ürünü seçer, stok alanına mevcut−3 yazar, "Stok Güncelle". `StokGuncelle` yetkisi Operator'da.
3. Log: `Stok güncellendi. UrunId=12, YeniStok=47` — eski değer ve kullanıcı yok. Raporlar sipariş tablosundan beslendiği için fark hiçbir yerde çıkmaz.
4. Sayım günü fiziksel stok sistemle "tutar".
Azaltım: Hareket defteri + Operator'un stoğu yalnızca "giriş" (artış) yapabilmesi; azaltma ve sayım düzeltmesi Admin'e ya da açıklama zorunlu + günlük rapor. Dekoratörlerde kullanıcı kimliğini log kapsamına ekle.

### Z3 — Satışın silinmesi ve stokun şişmesi (O)
1. Operator 10 adetlik sipariş açar (stok 50→40). Ürün satılır, para alınır.
2. Admin (ya da Z1 ile Admin olmuş Operator) ay sonu sayımında stoğu 40 olarak onaylar (`StokGuncelle 40`).
3. Ertesi gün siparişi siler: stok 40→50. Fiziksel 40.
4. Aylık rapor 10 adet eksik gösterir; silinen sipariş hiçbir ekranda yok; silen kişi kaydı yok.
5. Sonraki siparişler 50 görünen stoktan kabul edilir; teslimat sırasında eksik çıkar.
Azaltım: Silme yerine "iptal" kaydı (yeni satır, negatif hareket) ve raporlarda "iptal edilen" sütunu; siparişten sonra stok düzeltmesi varsa silmeyi reddet veya iadesiz sil seçeneği; `SilinmeZamani/SilenKullaniciId`.

### Z4 — İstemci saatiyle dönem kaydırma (O)
1. Operator Windows saatini önceki aya alır (yerel yöneticiyse; değilse NTP kapalı bir makinede saat kayması da aynı etkiyi verir).
2. Sipariş oluşturur; `Tarih = GetLocalNow()` önceki ay yazılır.
3. Kapanmış ayın raporu ve CSV'si değişir; günlük hedef/prim hesabı bozulur.
4. Saati düzeltir. `Siparisler.Tarih` için ileri/geri sınır yok.
Ek etki: Aynı saat kayması `KilitliMi`'yi de etkiler; saati 15 dk ileri alan istemci kilitli hesabı hemen dener (sunucu `KilitBitis` değerini değiştirmez, karşılaştırma istemcide yapılır).
Azaltım: `Tarih`'i uygulamadan göndermeyip DB varsayılanına bırak ya da `SYSUTCDATETIME()` sorgusuyla al; kilit kontrolünü de DB saatiyle yap.

### Z5 — Eşikle uyarı baskılama + stok düşürme (O)
1. Operator ürünü düzenler, eşiği 0 yapar (`UrunEkleGuncelle`).
2. Z2'deki gibi stoğu azaltır. Ürün "Yeterli" görünür; düşük stok sayacı ve filtresi onu göstermez.
3. Yönetici uyarı almaz; ürün tükenince fark edilir, arada geçen süre izsizdir.
Azaltım: Eşik düzenlemeyi Admin'e ayır; eşik değişikliğini logla; "Eşik 0 olan ürünler" raporu.

### Z6 — İlk kurulum boşluğundan Admin (O)
Bugünkü durum: arayüzde giriş/ilk kurulum ekranı yok; uygulama her işlemde "Bu işlem için oturum açmanız gerekir" uyarısı verir. Zincir, giriş ekranı eklendiğinde geçerlidir.
1. Saldırgan `Kullanicilar` tablosunu boşaltır (DB yazma hakkı: Z1 ön koşulu) **ya da** yönetici yeni ortam kurar, migration'ları uygular ve ilk açılışı yapmadan önce makine paylaşımlıdır.
2. Uygulamayı ilk açan kişi "İlk yönetici oluştur" ekranını görür, kendini Admin yapar.
3. Gerçek yönetici geldiğinde "İlk kurulum zaten tamamlanmış" mesajını alır; mevcut Admin'i tanımaz, kurtarma için yine DB'ye elle gider.
Eşzamanlı varyant: İki kişi aynı anda ilk Admin'i oluşturur; biri `SqlException 1205` görür ve neden olduğunu anlamaz; diğerinin kim olduğu loglarda yalnız `KullaniciId=1` olarak yazılır.
Azaltım: Kurulum bayrağı (`Ayarlar.IlkKurulumTamamlandi`), ilk kurulum için kurulum sırasında üretilen tek kullanımlık anahtar; ilk Admin'in kullanıcı adını (yalnız bu olayda) logla; 1205'i yakalayıp anlamlı mesaj ver.

### Z7 — Ağdan rol enjeksiyonu (O; yalnız uzak SQL Server kurulumunda)
1. Aynı ağdaki saldırgan ARP/DNS ile istemciyi kendi TDS proxy'sine yönlendirir; `TrustServerCertificate=True` sertifika hatasını susturur.
2. Proxy NTLM kimliğini gerçek sunucuya aktarır (Kerberos zorunlu değilse).
3. Girişte dönen `Kullanicilar` satırında `Rol`'ü 1, `Aktif`'i 1 yapar; `ParolaHash` dokunulmaz (saldırganın hedefi kullanıcının kendisi ya da kullanıcının farkında olmadan Admin oturumu). Alternatif: kullanıcı yanlış parola girdiğinde proxy `BasarisizDenemeSayisi` güncellemesini düşürür (kilit baskılama) ve çevrimiçi deneme sınırını kaldırır.
4. Uygulama bellekte Admin oturumu açar; sonraki tüm yazmalar gerçek DB'ye gerçek kimlikle gider.
Azaltım: `Encrypt=Mandatory;TrustServerCertificate=False` + güvenilen sertifika; Kerberos (SPN) zorunlu; SQL Server'da `Extended Protection`.

---

## 7. En önemli 5 zincir için test önerileri

Test dosyası yazılmamıştır; aşağıdakiler eklenmesi önerilen senaryolardır. SQL Server'a özgü maddeler için (aralık kilidi, deadlock, CHECK) LocalDB üzerinde çalışan ayrı bir `[Trait("Kategori","SqlServer")]` test sınıfı gerekir; SQLite bu davranışları üretmez.

**T1 — Z1 (Kullanicilar satırını düzenleme)**
- SQLite: Ham SQL ile `UPDATE Kullanicilar SET Iterasyon = 1, ParolaHash = <1 iterasyonlu özet>, ParolaTuzu = <tuz>`; ardından `GirisYapAsync` **başarılı olur** ve özet 600k'ya yükseltilir. Bu test bugünkü davranışı belgeler; azaltım sonrası beklenen: `Iterasyon < MinimumIterasyon` ise giriş reddedilir ya da `Warning` loglanır ve Admin'e bildirilir.
- SQL Server: `CK_Kullanicilar_Iterasyon` güncellendikten sonra `Iterasyon = 1` ile `INSERT/UPDATE` reddedilir.
- SQL Server: Uygulama kullanıcısı rolüyle `UPDATE Kullanicilar SET Rol = 1` → izin hatası (229) beklenir.
- Tetikleyici/denetim eklenirse: `Rol` değişikliği `KullaniciDenetim`'e düşer (eski/yeni değer, `SUSER_SNAME()`, zaman).

**T2 — Z2 (izsiz stok düzeltmesi)**
- Operator oturumuyla `StokGuncelleAsync(urunId, stok - 3)`; log çıktısında (`FakeLogger` ile) `KullaniciId`, `EskiStok`, `YeniStok` alanlarının bulunduğunu doğrula (bugün başarısız olur; hedef davranış).
- Hareket defteri eklenirse: her `StokGuncelle` ve `SiparisOlustur/Sil` çağrısı tam bir hareket satırı üretir; `SUM(delta)` = `Urunler.Stok` değişmezi (property-based: rastgele işlem dizisi sonrası eşitlik).
- Yetki ayrımı eklenirse: Operator'un stoğu *azaltan* `StokGuncelle` çağrısı `YetkisizIslemException`, artıran çağrısı başarılı.

**T3 — Z3 (silme ile stok şişirme)**
- Sıra: sipariş (50→40) → `StokGuncelle(40)` → `SiparisSil`. Bugünkü sonuç 50; hedef davranış ya `GecersizIslemException("Siparişten sonra stok düzeltmesi yapılmış")` ya da stok 40 kalır ve hareket defterinde "iadesiz iptal" satırı oluşur.
- `SiparisSil` sonrası `Siparisler` satırında `SilinmeZamani` ve `SilenKullaniciId` dolu; `GetGecmisSayfaAsync(silinenlerDahil: true)` Admin için satırı döndürür, Operator için `YetkisizIslemException`.
- Rapor: silinen siparişin `AylikOzet`'te "İptal" sütununa düştüğü, toplamdan sessizce çıkmadığı.

**T4 — Z4 (istemci saati)**
- `SabitZamanSaglayici` ile saati 40 gün geriye al, sipariş oluştur; bugünkü davranışta `Tarih` geçmiş aya yazılır. Hedef: `Tarih` DB varsayılanından gelir (`SabitZamanSaglayici` değerinden bağımsız) ya da `|Tarih - DbNow| > tolerans` ise `GecersizIslemException`.
- Kilit: saat 15 dk ileri alınmış istemcide `GirisYapAsync` kilitli hesabı kabul etmez (kilit kontrolü DB saatiyle yapıldıktan sonra).
- SQL Server: `Siparisler.Tarih` için `CHECK (Tarih <= DATEADD(day, 1, GETDATE()))` (ileri tarih) kabul/ret testi; geçmiş tarih için uygulama seviyesinde dönem kilidi varsa testi.

**T5 — Z6 ve kimlik yarışları (ilk kurulum, karşılıklı düşürme)**
- SQL Server (LocalDB): iki ayrı `DbContext` + `KimlikService` ile `IlkAdminOlusturAsync` aynı anda (`Barrier` ile); beklenen: tam 1 satır, bir çağrı `KullaniciDto`, diğeri **`GecersizIslemException`** (bugün `SqlException 1205` ya da `DbUpdateException` döner; önce bu davranışı belgeleyen bir test, azaltımdan sonra hedef davranış).
- SQL Server: iki Admin'in birbirini aynı anda Operator yapması; sonuçta `AktifAdminSayisi >= 1`, en fazla biri başarılı, diğeri anlamlı `DomainException`.
- Kurulum bayrağı eklenirse: `Kullanicilar` boş + bayrak dolu → `IlkKurulumGerekliMiAsync == false`, `IlkAdminOlusturAsync` → `GecersizIslemException`, `Warning` log.
- Üretim DI: `Program.ServisleriOlustur` (ya da ondan çıkarılan paylaşılan kayıt metodu) ile kurulan sağlayıcıda `IYetkiDenetcisi` türü `OturumYetkiDenetcisi` ve `YetkiDenetimiYok` hiçbir yerde kayıtlı değil (bugün bu test yalnız `AddApplication+AddData` için var).

---

## 8. Kabul edilen riskler ve gerekçeleri

| Risk | Gerekçe | Koşul |
|---|---|---|
| Süreç içinden `OturumAc(...)` / somut servis çözme / `YetkiDenetimiYok` ile kontrol atlama | İki katmanlı masaüstü uygulamasında süreç kullanıcının denetimindedir; bu yolu kullanabilen kişi DB'ye de doğrudan ulaşabilir (Z1). Asıl savunma DB haklarıdır. | Z1 azaltımı uygulanırsa bu yollar pratik önemini yitirir. |
| CSV dosyasının korumasız olması, Operator'un geçmişi ekranda görmesi | İş gereği; dosya kullanıcının kendi diskinde. | Dışa aktarım loglanır. |
| `appsettings.json`'ın kullanıcı tarafından düzenlenebilmesi | Parola içermez; başka sunucuya yönlendirme gerçek veriye erişim sağlamaz. | Kurulum, uygulama dizinini yalnızca yöneticiye yazılabilir yapar. |
| Oturum zaman aşımının olmaması | Tezgâh başı kullanım; Windows ekran kilidi politikası var sayılır. | İleride "15 dk hareketsizlikte oturumu kapat" eklenebilir. |
| Toplu veri üretimiyle yavaşlatma | Operator sayısı az, DB yerel. | — |
| Kategori silmede ürünlerin kategorisiz kalması | Tasarım kararı (`ON DELETE SET NULL`); uyarı sayacı etkilenmez. | Arayüzde onay metni ürün sayısını gösterir. |
| Testlerin SQLite üzerinde koşması | Hız ve CI kolaylığı. | SQL Server'a özgü maddeler için ayrı, isteğe bağlı test sınıfı (§7). |
| Yerel log dosyasının silinebilmesi | Kritik olaylar DB denetim tablosuna da yazılacak. | Denetim tablosu eklenince kabul edilir. |

---

## 9. Öncelikli yapılacaklar (şiddet sırasıyla)

1. **Veritabanı haklarını uygulama rolleriyle hizala (Z1).** Uygulama kullanıcılarının Windows gruplarına yalnızca gereken tablo hakları; `Kullanicilar` için `DENY UPDATE/INSERT/DELETE`, kullanıcı yönetimi saklı yordamla ve denetim tablosuna yazarak. Kısa vadede: `CK_Kullanicilar_Iterasyon >= 600000`, `Kullanicilar` üzerinde değişiklik tetikleyicisi, SQL Server Audit.
2. **İşlemi yapan kullanıcıyı her log satırına ve DB denetim tablosuna yaz (R).** Dekoratörlerde `BeginScope(KullaniciId)`; `SiparisSil`, `StokGuncelle`, rol/aktiflik, kategori silme için `Denetim` tablosu.
3. **Stok hareket defteri (Z2, Z3).** Mutlak set yerine delta; sipariş/iptal/düzeltme türleri; Operator'a yalnızca artış; azaltma/sayım Admin ya da açıklama zorunlu. Silmede sayım sonrası iade kuralı.
4. **Sipariş tarihini ve kilit kontrolünü sunucu saatine bağla (Z4).** `Tarih` DB varsayılanı; ileri tarih CHECK; `KilitliMi` için DB saati.
5. **Eşik düzenlemeyi Admin yetkisine al ve logla (Z5).**
6. **İlk kurulumu sağlamlaştır (Z6).** Kurulum bayrağı, tek kullanımlık kurulum anahtarı, 1205 için anlamlı hata, ilk Admin adının loglanması. Giriş/ilk kurulum/kullanıcı yönetimi ekranları eklenirken `Program.ServisleriOlustur` üzerinde dekoratör testi.
7. **Bağlantı güvenliği (Z7).** Uzak SQL Server kullanılan kurulumlarda `Encrypt=Mandatory; TrustServerCertificate=False`, Kerberos.
8. **Kurtarma işlevleri.** Admin'in başkasının parolasını sıfırlaması (ilk girişte zorunlu değişim) ve kilit açma; böylece DB'ye elle müdahale "normal" olmaz.
9. **Silinen siparişleri görünür kıl.** `SilinmeZamani/SilenKullaniciId`; Admin'e "silinenleri göster"; raporlarda iptal sütunu.
10. **Küçük sağlamlaştırmalar.** Stok üst sınırı (taşma), ürün adı değişikliğini logla, dışa aktarımı logla (dosya adı, satır sayısı), oturum zaman aşımı seçeneği.
