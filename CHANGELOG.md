# Değişiklik Günlüğü

## 1.0.0 - 2026-10-19

- Proje .NET Framework 4.8'den .NET 10'a taşındı (`net10.0-windows` arayüz, `net10.0` kütüphaneler).
- Katmanlı mimariye geçildi: Domain, Application, Data ve UI projeleri; arayüz MVP (presenter + view arayüzleri) düzeninde.
- Veri erişimi EF Core 10'a taşındı; `DatabaseHelper` kaldırıldı, repository'ler ve `UnitOfWork` eklendi. Şema migration'larla oluşturuluyor (`.bak` dosyası yerine).
- Bağlantı dizesi ve log ayarları `appsettings.json`'a taşındı; Serilog ile günlük dosyalara loglama (14 gün).
- Sipariş geçmişine sayfalama, tarih aralığı filtresi ve ürün araması eklendi.
- Sipariş silme yumuşak silme oldu; silinen adet stoğa iade ediliyor.
- `RowVersion` ile eşzamanlılık koruması eklendi.
- Kategoriler ve ürün bazlı düşük stok eşiği eklendi.
- Raporlar eklendi: en çok satanlar, günlük ve aylık özet.
- Kullanıcı girişi, ilk kurulum ekranı ve Yönetici/Operatör rolleri eklendi; yetkiler her işlemde veritabanındaki güncel kayıtla doğrulanıyor.
- Parolalar PBKDF2-SHA256 ile saklanıyor; parola politikası ve 5 hatalı denemede 15 dakikalık giriş kilidi eklendi.
- Kullanıcı yönetimi ve parola değiştirme ekranları eklendi.
- Denetim kaydı eklendi: kim, ne zaman, ne yaptı; önceki/sonraki değer ve fark. Yalnızca yönetici görür, kayıtlar uygulama üzerinden değiştirilemez.
- Sipariş geçmişi ve denetim kayıtları için CSV dışa aktarma eklendi (Excel uyumlu, formül enjeksiyonuna karşı korumalı).
- Tehdit modeli yazıldı (`docs/THREAT_MODEL.md`).
- Testler eklendi: birim (Application), SQLite üzerinde entegrasyon ve yetki matrisi (Data), arayüz (UI), isteğe bağlı gerçek SQL Server testleri.
- GitHub Actions iş akışı eklendi: derleme, test, zafiyetli paket ve migration kontrolü, `v*` etiketinde win-x64 tek dosyalı yayın paketi.
