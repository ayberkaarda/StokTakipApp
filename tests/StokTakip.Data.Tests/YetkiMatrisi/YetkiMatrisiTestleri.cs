using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StokTakip.Application;
using StokTakip.Application.Denetim;
using StokTakip.Application.DisaAktarim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Kimlik;
using StokTakip.Application.Raporlama;
using StokTakip.Application.Services;
using StokTakip.Data.Tests.Altyapi;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Data.Tests.YetkiMatrisi;

/// <summary>
/// Üretim bileşimi (AddApplication + AddData) bellek içi SQLite ile. Her test kendi veritabanını ve kendi
/// tekil oturum bağlamını kurar; her işlem uygulamadaki gibi ayrı bir DI kapsamında çalışır.
/// </summary>
internal sealed class MatrisOrtami : IAsyncDisposable
{
    public const string Parola = "Matris-Parola-01";

    private readonly SqliteConnection _baglanti;

    private MatrisOrtami()
    {
        _baglanti = new SqliteConnection("DataSource=:memory:");
        _baglanti.Open();
        var services = new ServiceCollection();
        services.AddSingleton<IParolaHasher>(TestParolaHasher.Instance);
        services.AddApplication();
        services.AddData("Server=(localdb)\\yok;Database=Yok;Integrated Security=true");
        services.RemoveAll<IDbContextOptionsConfiguration<StokTakipDbContext>>();
        services.AddDbContext<StokTakipDbContext>(o => o
            .UseSqlite(_baglanti)
            .ReplaceService<IModelCustomizer, KimlikSqliteModelCustomizer>()
            .AddInterceptors(new RowVersionInterceptor()));
        Servisler = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        Oturum = Servisler.GetRequiredService<IOturumBaglami>();
    }

    public ServiceProvider Servisler { get; }

    public IOturumBaglami Oturum { get; }

    public int AdminId { get; private set; }

    public int OperatorId { get; private set; }

    public int HedefId { get; private set; }

    public int UrunId { get; private set; }

    public int SilinecekUrunId { get; private set; }

    public int SiparisId { get; private set; }

    public int KategoriId { get; private set; }

    /// <param name="tohumla">false ise kullanıcısız boş veritabanı (ilk kurulum akışı için).</param>
    public static async Task<MatrisOrtami> KurAsync(bool tohumla = true)
    {
        var ortam = new MatrisOrtami();
        await using var scope = ortam.Servisler.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<StokTakipDbContext>();
        await context.Database.EnsureCreatedAsync();
        if (!tohumla)
        {
            return ortam;
        }

        var ozet = TestParolaHasher.Instance.Hashle(Parola);
        var admin = new Kullanici("yonetici", ozet.Hash, ozet.Tuz, ozet.Iterasyon, KullaniciRolu.Admin);
        var operatorKullanici = new Kullanici("ali", ozet.Hash, ozet.Tuz, ozet.Iterasyon, KullaniciRolu.Operator);
        var hedef = new Kullanici("hedef", ozet.Hash, ozet.Tuz, ozet.Iterasyon, KullaniciRolu.Operator);
        var kategori = new Kategori("Ofis");
        context.AddRange(admin, operatorKullanici, hedef, kategori);
        await context.SaveChangesAsync();

        var kalem = new Urun("Kalem", 51);
        kalem.KategoriyeAta(kategori);
        var silinecek = new Urun("Silinecek", 5);
        context.AddRange(kalem, silinecek);
        await context.SaveChangesAsync();
        var siparis = Siparis.Olustur(kalem, 1, new DateTime(2026, 5, 10, 9, 0, 0));
        context.Siparisler.Add(siparis);
        await context.SaveChangesAsync();

        (ortam.AdminId, ortam.OperatorId, ortam.HedefId) = (admin.KullaniciId, operatorKullanici.KullaniciId, hedef.KullaniciId);
        (ortam.UrunId, ortam.SilinecekUrunId, ortam.SiparisId, ortam.KategoriId) = (kalem.UrunId, silinecek.UrunId, siparis.SiparisId, kategori.KategoriId);
        return ortam;
    }

    public void OturumAc(string rol)
    {
        switch (rol)
        {
            case "Oturumsuz":
                Oturum.OturumuKapat();
                break;
            case "Operator":
                Oturum.OturumAc(new OturumKullanicisi(OperatorId, "ali", KullaniciRolu.Operator));
                break;
            case "Admin":
                Oturum.OturumAc(new OturumKullanicisi(AdminId, "yonetici", KullaniciRolu.Admin));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(rol), rol, null);
        }
    }

    public async Task CalistirAsync(Func<IServiceProvider, Task> islem)
    {
        await using var scope = Servisler.CreateAsyncScope();
        await islem(scope.ServiceProvider);
    }

    public async Task SqlAsync(FormattableString sql)
    {
        await using var scope = Servisler.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<StokTakipDbContext>().Database.ExecuteSqlInterpolatedAsync(sql);
    }

    public async Task<List<DenetimKaydi>> DenetimKayitlariAsync()
    {
        await using var scope = Servisler.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<StokTakipDbContext>()
            .DenetimKayitlari.AsNoTracking().OrderBy(x => x.DenetimId).ToListAsync();
    }

    public async Task<List<Kullanici>> KullanicilarAsync()
    {
        await using var scope = Servisler.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<StokTakipDbContext>()
            .Kullanicilar.AsNoTracking().OrderBy(x => x.KullaniciId).ToListAsync();
    }

    /// <summary>Tüm tabloların karşılaştırılabilir metin görüntüsü: reddedilen işlemin hiçbir iz bırakmadığını doğrulamak için.</summary>
    public async Task<string> AnlikGoruntuAsync()
    {
        await using var scope = Servisler.CreateAsyncScope();
        var c = scope.ServiceProvider.GetRequiredService<StokTakipDbContext>();
        var sb = new StringBuilder();
        foreach (var u in await c.Urunler.AsNoTracking().OrderBy(x => x.UrunId).ToListAsync())
            sb.AppendLine($"U{u.UrunId}|{u.UrunAdi}|{u.Stok}|{u.DusukStokEsigi}|{u.KategoriId}");
        foreach (var s in await c.Siparisler.IgnoreQueryFilters().AsNoTracking().OrderBy(x => x.SiparisId).ToListAsync())
            sb.AppendLine($"S{s.SiparisId}|{s.UrunId}|{s.Adet}|{s.IsDeleted}");
        foreach (var k in await c.Kategoriler.AsNoTracking().OrderBy(x => x.KategoriId).ToListAsync())
            sb.AppendLine($"K{k.KategoriId}|{k.Ad}");
        foreach (var k in await c.Kullanicilar.AsNoTracking().OrderBy(x => x.KullaniciId).ToListAsync())
            sb.AppendLine($"Ku{k.KullaniciId}|{k.KullaniciAdi}|{k.Rol}|{k.Aktif}|{k.BasarisizDenemeSayisi}|{k.KilitBitis}|{k.SonGirisTarihi}|{Convert.ToBase64String(k.ParolaHash)}");
        sb.AppendLine($"D{await c.DenetimKayitlari.CountAsync()}");
        return sb.ToString();
    }

    public async ValueTask DisposeAsync()
    {
        await Servisler.DisposeAsync();
        await _baglanti.DisposeAsync();
    }
}

/// <summary>
/// Savunma regresyonu: rol × işlem matrisi. Her servis işlemi için Operator, Admin ve oturumsuz çağrının beklenen
/// sonucu sabittir; reddedilen çağrı veritabanında (denetim kaydı dahil) hiçbir değişiklik bırakmaz.
/// </summary>
public sealed class YetkiMatrisiTestleri
{
    private const string YeniParola = "Yeni-Parola-2024x";

    private sealed record Islem(KullaniciRolu GerekliRol, bool Yazar, Func<IServiceProvider, MatrisOrtami, Task> Calistir);

    private static T S<T>(IServiceProvider s) where T : notnull => s.GetRequiredService<T>();

    private static readonly IReadOnlyDictionary<string, Islem> Islemler = new Dictionary<string, Islem>
    {
        ["UrunListele"] = new(KullaniciRolu.Operator, false, (s, o) => S<IStokService>(s).UrunleriListeleAsync()),
        ["UrunGetir"] = new(KullaniciRolu.Operator, false, (s, o) => S<IStokService>(s).UrunGetirAsync(o.UrunId)),
        ["UrunEkle"] = new(KullaniciRolu.Operator, true, (s, o) => S<IStokService>(s).UrunEkleAsync(new UrunEkleIstegi("Yeni Ürün", 3))),
        ["UrunGuncelle"] = new(KullaniciRolu.Operator, true, (s, o) => S<IStokService>(s).UrunGuncelleAsync(new UrunGuncelleIstegi(o.UrunId, "Kalem X", 40))),
        ["StokGuncelle"] = new(KullaniciRolu.Operator, true, (s, o) => S<IStokService>(s).StokGuncelleAsync(o.UrunId, 33)),
        ["UrunSil"] = new(KullaniciRolu.Admin, true, (s, o) => S<IStokService>(s).UrunSilAsync(o.SilinecekUrunId)),
        ["EsikDegistir"] = new(KullaniciRolu.Admin, true, (s, o) => S<IStokService>(s).UrunGuncelleAsync(new UrunGuncelleIstegi(o.UrunId, "Kalem", 50, DusukStokEsigi: 3))),
        ["SiparisOlustur"] = new(KullaniciRolu.Operator, true, (s, o) => S<ISiparisService>(s).SiparisOlusturAsync(new SiparisOlusturIstegi(o.UrunId, 2))),
        ["SiparisGecmisi"] = new(KullaniciRolu.Operator, false, (s, o) => S<ISiparisService>(s).SiparisGecmisiniGetirAsync()),
        ["SiparisGecmisiSayfa"] = new(KullaniciRolu.Operator, false, (s, o) => S<ISiparisService>(s).SiparisGecmisiSayfaGetirAsync(new SiparisGecmisiSorgusu())),
        ["SiparisOzeti"] = new(KullaniciRolu.Operator, false, (s, o) => S<ISiparisService>(s).SiparisOzetiGetirAsync()),

        // Geçmiş ekranıyla aynı veri; dosyaya aktarım yetkisi ISiparisDisaAktarimService'tedir.
        ["SiparisCsvSayfasi"] = new(KullaniciRolu.Operator, false, (s, o) => S<ISiparisService>(s).SiparisCsvSayfasiGetirAsync(new SiparisGecmisiSorgusu(), null, 10)),
        ["SiparisSil"] = new(KullaniciRolu.Admin, true, (s, o) => S<ISiparisService>(s).SiparisSilAsync(o.SiparisId)),
        ["KategoriListele"] = new(KullaniciRolu.Operator, false, (s, o) => S<IKategoriService>(s).KategorileriListeleAsync()),
        ["KategoriEkle"] = new(KullaniciRolu.Admin, true, (s, o) => S<IKategoriService>(s).KategoriEkleAsync("Kırtasiye")),
        ["KategoriYenidenAdlandir"] = new(KullaniciRolu.Admin, true, (s, o) => S<IKategoriService>(s).KategoriYenidenAdlandirAsync(o.KategoriId, "Büro")),
        ["KategoriSil"] = new(KullaniciRolu.Admin, true, (s, o) => S<IKategoriService>(s).KategoriSilAsync(o.KategoriId)),
        ["RaporEnCokSatan"] = new(KullaniciRolu.Operator, false, (s, o) => S<IRaporService>(s).EnCokSatanlariGetirAsync(null, null, 10)),
        ["RaporGunluk"] = new(KullaniciRolu.Operator, false, (s, o) => S<IRaporService>(s).GunlukOzetAsync(new DateTime(2026, 5, 1), new DateTime(2026, 5, 31))),
        ["RaporAylik"] = new(KullaniciRolu.Operator, false, (s, o) => S<IRaporService>(s).AylikOzetAsync(new DateTime(2026, 1, 1), new DateTime(2026, 12, 31))),
        ["SiparisCsvYetkiKontrol"] = new(KullaniciRolu.Admin, false, (s, o) => S<ISiparisDisaAktarimService>(s).YetkiKontrolEtAsync()),
        ["SiparisCsvAktar"] = new(KullaniciRolu.Admin, true, async (s, o) =>
        {
            var servis = S<ISiparisDisaAktarimService>(s);
            var sayi = await servis.SiparisleriCsvAktarAsync(new SiparisGecmisiSorgusu(), new MemoryStream());
            await servis.AktarimTamamlandiAsync(sayi);
        }),
        ["DenetimSorgula"] = new(KullaniciRolu.Admin, false, (s, o) => S<IDenetimService>(s).DenetimKayitlariniGetirAsync(new DenetimSorgusu())),
        ["DenetimYetkiKontrol"] = new(KullaniciRolu.Admin, false, (s, o) => S<IDenetimService>(s).YetkiKontrolEtAsync()),
        ["DenetimCsvAktar"] = new(KullaniciRolu.Admin, true, async (s, o) =>
        {
            var servis = S<IDenetimService>(s);
            var sayi = await servis.DenetimKayitlariniCsvAktarAsync(new DenetimSorgusu(), new MemoryStream());
            await servis.AktarimTamamlandiAsync(sayi);
        }),
        ["KullaniciListele"] = new(KullaniciRolu.Admin, false, (s, o) => S<IKimlikService>(s).KullaniciListeleAsync()),
        ["KullaniciOlustur"] = new(KullaniciRolu.Admin, true, (s, o) => S<IKimlikService>(s).KullaniciOlusturAsync("yeni.kullanici", "Gecerli-Parola-77", KullaniciRolu.Operator)),
        ["RolDegistir"] = new(KullaniciRolu.Admin, true, (s, o) => S<IKimlikService>(s).RolDegistirAsync(o.HedefId, KullaniciRolu.Admin)),
        ["AktiflikDegistir"] = new(KullaniciRolu.Admin, true, (s, o) => S<IKimlikService>(s).AktiflikDegistirAsync(o.HedefId, false)),
        ["ParolaDegistir"] = new(KullaniciRolu.Operator, true, (s, o) => S<IKimlikService>(s).ParolaDegistirAsync(MatrisOrtami.Parola, YeniParola)),
    };

    public static TheoryData<string, string> Matris()
    {
        var veri = new TheoryData<string, string>();
        foreach (var rol in new[] { "Oturumsuz", "Operator", "Admin" })
        foreach (var islem in Islemler.Keys)
            veri.Add(rol, islem);
        return veri;
    }

    public static TheoryData<string> TumIslemler() => new(Islemler.Keys);

    public static TheoryData<string> AdminIslemleri() => new(Islemler.Where(x => x.Value.GerekliRol == KullaniciRolu.Admin).Select(x => x.Key));

    public static TheoryData<string> OperatorIslemleri() => new(Islemler.Where(x => x.Value.GerekliRol == KullaniciRolu.Operator).Select(x => x.Key));

    private static async Task ReddedilirVeIzBirakmazAsync(MatrisOrtami ortam, string islemAdi, string beklenenMesaj)
    {
        var once = await ortam.AnlikGoruntuAsync();

        var hata = await Assert.ThrowsAsync<YetkisizIslemException>(() => ortam.CalistirAsync(s => Islemler[islemAdi].Calistir(s, ortam)));

        Assert.Equal(beklenenMesaj, hata.Message);
        Assert.Equal(once, await ortam.AnlikGoruntuAsync());
    }

    [Theory]
    [MemberData(nameof(Matris))]
    public async Task Rol_islem_matrisi(string rol, string islemAdi)
    {
        await using var ortam = await MatrisOrtami.KurAsync();
        var islem = Islemler[islemAdi];
        ortam.OturumAc(rol);
        var izinli = rol != "Oturumsuz" && Enum.Parse<KullaniciRolu>(rol) >= islem.GerekliRol;

        if (!izinli)
        {
            var mesaj = rol == "Oturumsuz" ? YetkisizIslemException.OturumGerekliMesaji
                : islemAdi == "EsikDegistir" ? YetkiDenetimliStokService.EsikYetkisiMesaji
                : YetkisizIslemException.YetkiYokMesaji;
            await ReddedilirVeIzBirakmazAsync(ortam, islemAdi, mesaj);

            // Rolü yetmeyen geçerli oturum kapanmaz; yalnızca o işlem reddedilir.
            Assert.Equal(rol != "Oturumsuz", ortam.Oturum.OturumAcikMi);
            return;
        }

        var onceki = await ortam.AnlikGoruntuAsync();
        var oncekiKayitSayisi = (await ortam.DenetimKayitlariAsync()).Count;

        await ortam.CalistirAsync(s => islem.Calistir(s, ortam));

        Assert.True(ortam.Oturum.OturumAcikMi);
        if (!islem.Yazar)
        {
            Assert.Equal(onceki, await ortam.AnlikGoruntuAsync());
            return;
        }

        Assert.NotEqual(onceki, await ortam.AnlikGoruntuAsync());
        var yeniKayitlar = (await ortam.DenetimKayitlariAsync()).Skip(oncekiKayitSayisi).ToList();
        Assert.NotEmpty(yeniKayitlar);
        var beklenenAktor = rol == "Admin" ? (ortam.AdminId, "yonetici") : (ortam.OperatorId, "ali");
        Assert.All(yeniKayitlar, k => Assert.Equal(beklenenAktor, (k.KullaniciId!.Value, k.KullaniciAdi)));
    }

    [Theory]
    [MemberData(nameof(TumIslemler))]
    public async Task Pasif_operator_oturumu_acik_olsa_da_her_islemden_reddedilir(string islemAdi)
    {
        await using var ortam = await MatrisOrtami.KurAsync();
        ortam.OturumAc("Operator");
        await ortam.SqlAsync($"UPDATE Kullanicilar SET Aktif = 0 WHERE KullaniciID = {ortam.OperatorId}");

        await ReddedilirVeIzBirakmazAsync(ortam, islemAdi, YetkisizIslemException.YetkiYokMesaji);

        // Operator düzeyindeki işlemde güncel kayıt okunur ve oturum kapatılır; Admin işlemi bellekteki rolle önceden reddedilir.
        if (Islemler[islemAdi].GerekliRol == KullaniciRolu.Operator)
        {
            Assert.False(ortam.Oturum.OturumAcikMi);
        }
    }

    [Theory]
    [MemberData(nameof(TumIslemler))]
    public async Task Pasif_admin_her_islemden_reddedilir_ve_oturumu_kapanir(string islemAdi)
    {
        await using var ortam = await MatrisOrtami.KurAsync();
        ortam.OturumAc("Admin");
        await ortam.SqlAsync($"UPDATE Kullanicilar SET Aktif = 0 WHERE KullaniciID = {ortam.AdminId}");

        await ReddedilirVeIzBirakmazAsync(ortam, islemAdi, YetkisizIslemException.YetkiYokMesaji);
        Assert.False(ortam.Oturum.OturumAcikMi);
    }

    [Theory]
    [MemberData(nameof(AdminIslemleri))]
    public async Task Rolu_dusurulmus_admin_admin_islemlerinden_reddedilir_ve_oturumu_kapanir(string islemAdi)
    {
        await using var ortam = await MatrisOrtami.KurAsync();
        ortam.OturumAc("Admin");
        await ortam.SqlAsync($"UPDATE Kullanicilar SET Rol = 0 WHERE KullaniciID = {ortam.AdminId}");

        await ReddedilirVeIzBirakmazAsync(ortam, islemAdi, YetkisizIslemException.YetkiYokMesaji);
        Assert.False(ortam.Oturum.OturumAcikMi);
    }

    [Theory]
    [MemberData(nameof(OperatorIslemleri))]
    public async Task Rolu_dusurulmus_admin_operator_islemlerini_yapabilir(string islemAdi)
    {
        await using var ortam = await MatrisOrtami.KurAsync();
        ortam.OturumAc("Admin");
        await ortam.SqlAsync($"UPDATE Kullanicilar SET Rol = 0 WHERE KullaniciID = {ortam.AdminId}");

        await ortam.CalistirAsync(s => Islemler[islemAdi].Calistir(s, ortam));
    }

    [Theory]
    [InlineData("UrunSil")]
    [InlineData("KullaniciOlustur")]
    [InlineData("DenetimSorgula")]
    public async Task Veritabaninda_admin_yapilmis_operator_oturumu_yeniden_giris_yapmadan_admin_islemi_yapamaz(string islemAdi)
    {
        await using var ortam = await MatrisOrtami.KurAsync();
        ortam.OturumAc("Operator");
        await ortam.SqlAsync($"UPDATE Kullanicilar SET Rol = 1 WHERE KullaniciID = {ortam.OperatorId}");

        await ReddedilirVeIzBirakmazAsync(ortam, islemAdi, YetkisizIslemException.YetkiYokMesaji);
        Assert.True(ortam.Oturum.OturumAcikMi);
    }

    [Theory]
    [InlineData("UrunListele")]
    [InlineData("SiparisOlustur")]
    [InlineData("ParolaDegistir")]
    public async Task Veritabanindan_silinmis_kullanicinin_oturumu_reddedilir_ve_kapanir(string islemAdi)
    {
        await using var ortam = await MatrisOrtami.KurAsync();
        ortam.OturumAc("Operator");
        await ortam.SqlAsync($"DELETE FROM Kullanicilar WHERE KullaniciID = {ortam.OperatorId}");

        await ReddedilirVeIzBirakmazAsync(ortam, islemAdi, YetkisizIslemException.YetkiYokMesaji);
        Assert.False(ortam.Oturum.OturumAcikMi);
    }

    [Fact]
    public async Task Operator_yeni_urunde_esik_veremez_varsayilan_kullanilir_ret_degil()
    {
        await using var ortam = await MatrisOrtami.KurAsync();
        ortam.OturumAc("Operator");

        UrunDto urun = null!;
        await ortam.CalistirAsync(async s => urun = await S<IStokService>(s).UrunEkleAsync(new UrunEkleIstegi("Defter", 4, DusukStokEsigi: 0)));

        Assert.Equal(Urun.VarsayilanDusukStokEsigi, urun.DusukStokEsigi);
    }

    // ---- Kullanıcı adı ve parola politikası sınırları ----

    public static TheoryData<string?> GecersizKullaniciAdlari() => new(
        null, "", "   ", "ab", "  ab  ", new string('a', Kullanici.KullaniciAdiMaksimumUzunluk + 1),
        "ali veli", "ayşe", "ılgaz", "İlker", "çağrı", "ali@firma", "ali/veli", "ali\tveli", "ali'veli");

    [Theory]
    [MemberData(nameof(GecersizKullaniciAdlari))]
    public void Gecersiz_kullanici_adi_dogrulamada_ve_giris_normallestirmesinde_reddedilir(string? ad)
    {
        var hata = Assert.Throws<GecersizGirdiException>(() => Kullanici.KullaniciAdiDogrula(ad));
        Assert.Equal(nameof(Kullanici.KullaniciAdi), hata.AlanAdi);
        Assert.False(Kullanici.KullaniciAdiNormallestir(ad, out var normal));
        Assert.Equal(string.Empty, normal);
    }

    [Theory]
    [InlineData("abc", "abc")]
    [InlineData("  Ali.Veli_01-x  ", "ali.veli_01-x")]
    [InlineData("ADMIN", "admin")]
    [InlineData("123", "123")]
    [InlineData("a-b", "a-b")]
    public void Gecerli_kullanici_adi_kirpilir_ve_kucuk_harfe_cevrilir(string ad, string beklenen)
    {
        Assert.Equal(beklenen, Kullanici.KullaniciAdiDogrula(ad));
        Assert.True(Kullanici.KullaniciAdiNormallestir(ad, out var normal));
        Assert.Equal(beklenen, normal);
    }

    [Fact]
    public void Kullanici_adi_uzunluk_sinirlari_tam_degerlerde_kabul_edilir()
    {
        Assert.Equal(Kullanici.KullaniciAdiMinimumUzunluk, Kullanici.KullaniciAdiDogrula(new string('x', 3)).Length);
        Assert.Equal(Kullanici.KullaniciAdiMaksimumUzunluk, Kullanici.KullaniciAdiDogrula(new string('X', 50)).Length);
        Assert.Throws<GecersizGirdiException>(() => Kullanici.KullaniciAdiDogrula(new string('x', 2)));
        Assert.Throws<GecersizGirdiException>(() => Kullanici.KullaniciAdiDogrula(new string('x', 51)));
    }

    [Fact]
    public async Task Servisten_gecersiz_adla_kullanici_olusturulamaz_buyuk_harfli_ad_kucuk_harfle_saklanir_ve_tekrar_edilemez()
    {
        await using var ortam = await MatrisOrtami.KurAsync();
        ortam.OturumAc("Admin");
        var once = await ortam.AnlikGoruntuAsync();

        foreach (var ad in new[] { "ab", "ayşe", "ali veli", new string('a', 51) })
        {
            await Assert.ThrowsAsync<GecersizGirdiException>(
                () => ortam.CalistirAsync(s => S<IKimlikService>(s).KullaniciOlusturAsync(ad, "Gecerli-Parola-77", KullaniciRolu.Operator)));
        }

        Assert.Equal(once, await ortam.AnlikGoruntuAsync());

        await ortam.CalistirAsync(s => S<IKimlikService>(s).KullaniciOlusturAsync("Mehmet.Can", "Gecerli-Parola-77", KullaniciRolu.Operator));
        await Assert.ThrowsAsync<GecersizGirdiException>(
            () => ortam.CalistirAsync(s => S<IKimlikService>(s).KullaniciOlusturAsync("MEHMET.CAN", "Gecerli-Parola-77", KullaniciRolu.Operator)));
        Assert.Single(await ortam.KullanicilarAsync(), x => x.KullaniciAdi == "mehmet.can");

        ortam.OturumAc("Oturumsuz");
        await ortam.CalistirAsync(s => S<IKimlikService>(s).GirisYapAsync("  MEHMET.can ", "Gecerli-Parola-77"));
        Assert.Equal("mehmet.can", ortam.Oturum.MevcutKullanici!.KullaniciAdi);
    }

    public static TheoryData<string?> GecersizParolalar() => new(
        null, "", "abcdefg12", new string('a', 127) + "12", "abcdefghijk", "12345678901", "xx-ALI.VELI-99", "1ali.veli2-ab");

    [Theory]
    [MemberData(nameof(GecersizParolalar))]
    public void Parola_politikasi_sinir_disi_degerleri_reddeder(string? parola)
    {
        var hata = Assert.Throws<GecersizGirdiException>(() => ParolaPolitikasi.Dogrula(parola, "ali.veli"));
        Assert.Equal(ParolaPolitikasi.AlanAdi, hata.AlanAdi);
    }

    [Theory]
    [InlineData("abcdefgh12")]
    [InlineData("Ççşşğğüüö1")]
    public void Parola_politikasi_alt_sinirdaki_gecerli_degerleri_kabul_eder(string parola)
    {
        Assert.Equal(ParolaPolitikasi.MinimumUzunluk, parola.Length);
        ParolaPolitikasi.Dogrula(parola, "ali.veli");
    }

    [Fact]
    public void Parola_politikasi_ust_sinir_tam_degerde_kabul_eder()
    {
        ParolaPolitikasi.Dogrula(new string('a', ParolaPolitikasi.MaksimumUzunluk - 1) + "1", "ali.veli");
        Assert.Throws<GecersizGirdiException>(() => ParolaPolitikasi.Dogrula(new string('a', ParolaPolitikasi.MaksimumUzunluk) + "1", "ali.veli"));
    }

    [Fact]
    public async Task Parola_politikasi_ilk_kurulumda_kullanici_olusturmada_ve_parola_degisiminde_uygulanir()
    {
        await using (var bos = await MatrisOrtami.KurAsync(tohumla: false))
        {
            await Assert.ThrowsAsync<GecersizGirdiException>(() => bos.CalistirAsync(s => S<IKimlikService>(s).IlkAdminOlusturAsync("yonetici", "kisa1")));
            await Assert.ThrowsAsync<GecersizGirdiException>(() => bos.CalistirAsync(s => S<IKimlikService>(s).IlkAdminOlusturAsync("yonetici", "Yonetici-123456")));
            Assert.Empty(await bos.KullanicilarAsync());
        }

        await using var ortam = await MatrisOrtami.KurAsync();
        ortam.OturumAc("Admin");
        await Assert.ThrowsAsync<GecersizGirdiException>(
            () => ortam.CalistirAsync(s => S<IKimlikService>(s).KullaniciOlusturAsync("veli", "sadeceharfler", KullaniciRolu.Operator)));
        var once = await ortam.AnlikGoruntuAsync();
        await Assert.ThrowsAsync<GecersizGirdiException>(
            () => ortam.CalistirAsync(s => S<IKimlikService>(s).ParolaDegistirAsync(MatrisOrtami.Parola, "1234567890")));
        Assert.Equal(once, await ortam.AnlikGoruntuAsync());
    }

    // ---- DTO yapısı: istemci rol, kimlik, aktiflik gibi sunucu alanlarını belirleyemez ----

    private static readonly string[] SunucuAlanlari =
    [
        "Rol", "Aktif", "KullaniciId", "Zaman", "Tarih", "SiparisId", "IsDeleted", "RowVersion", "ParolaHash", "ParolaTuzu",
        "Iterasyon", "BasarisizDenemeSayisi", "KilitBitis", "SonGirisTarihi", "DenetimId",
    ];

    private static string[] Ozellikler(Type tur) =>
        tur.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetIndexParameters().Length == 0)
            .Select(p => p.Name).Where(x => x != "EqualityContract").Order(StringComparer.Ordinal).ToArray();

    [Theory]
    [InlineData(typeof(UrunEkleIstegi), "DusukStokEsigi,KategoriId,Stok,UrunAdi")]
    [InlineData(typeof(UrunGuncelleIstegi), "DusukStokEsigi,KategoriGuncellensin,KategoriId,Stok,UrunAdi,UrunId")]
    [InlineData(typeof(SiparisOlusturIstegi), "Adet,UrunId")]
    [InlineData(typeof(SiparisGecmisiSorgusu), "Baslangic,Bitis,Sayfa,SayfaBoyutu,UrunAdi,UrunId")]
    [InlineData(typeof(DenetimSorgusu), "Baslangic,Bitis,Islem,KullaniciAdi,Sayfa,SayfaBoyutu,VarlikAdi")]
    public void Istemci_istek_tipleri_yalnizca_beklenen_alanlari_tasir(Type tur, string beklenen)
    {
        var ozellikler = Ozellikler(tur);

        Assert.Equal(beklenen.Split(','), ozellikler);
        Assert.Empty(ozellikler.Intersect(SunucuAlanlari));
    }

    public static TheoryData<Type> ServisArayuzleri() => new(
        typeof(IStokService), typeof(ISiparisService), typeof(IKategoriService), typeof(IRaporService),
        typeof(IDenetimService), typeof(ISiparisDisaAktarimService), typeof(IKimlikService));

    [Theory]
    [MemberData(nameof(ServisArayuzleri))]
    public void Servis_arayuzleri_varlik_ya_da_kullanici_dto_su_almaz(Type arayuz)
    {
        var yasak = arayuz.GetMethods()
            .SelectMany(m => m.GetParameters().Select(p => (Metot: m.Name, Tur: p.ParameterType)))
            .Where(x => x.Tur.Namespace == typeof(Kullanici).Namespace || x.Tur == typeof(KullaniciDto) || x.Tur == typeof(OturumKullanicisi)
                || x.Tur == typeof(UrunDto) || x.Tur == typeof(SiparisDto) || x.Tur == typeof(DenetimKaydiDto))
            .Select(x => $"{x.Metot}({x.Tur.Name})");

        Assert.Empty(yasak);
    }

    [Fact]
    public void Rol_parametresi_yalnizca_admin_gerektiren_kullanici_olusturma_ve_rol_degistirmede_vardir()
    {
        var rolAlanlar = typeof(IKimlikService).GetMethods()
            .Where(m => m.GetParameters().Any(p => p.ParameterType == typeof(KullaniciRolu)))
            .Select(m => m.Name).Order(StringComparer.Ordinal);

        Assert.Equal(new[] { nameof(IKimlikService.KullaniciOlusturAsync), nameof(IKimlikService.RolDegistirAsync) }, rolAlanlar);
        Assert.DoesNotContain(typeof(IKimlikService).GetMethod(nameof(IKimlikService.AktiflikDegistirAsync))!.GetParameters(), p => p.ParameterType == typeof(KullaniciRolu));
    }

    [Fact]
    public void Kullanici_dto_su_parola_ozeti_tuz_ve_sayac_tasimaz()
    {
        var ozellikler = typeof(KullaniciDto).GetProperties();

        Assert.DoesNotContain(ozellikler, p => p.PropertyType == typeof(byte[]));
        Assert.Equal("Aktif,KilitBitis,KullaniciAdi,KullaniciId,Rol,SonGirisTarihi", string.Join(",", Ozellikler(typeof(KullaniciDto))));
    }

    public static TheoryData<Type> DisariVerilenDtolar() => new(
        typeof(UrunDto), typeof(SiparisDto), typeof(KategoriDto), typeof(SiparisOzetiDto), typeof(KullaniciDto),
        typeof(DenetimKaydiDto), typeof(UrunSatisOzetiDto), typeof(DonemOzetiDto), typeof(UrunEkleIstegi),
        typeof(UrunGuncelleIstegi), typeof(SiparisOlusturIstegi), typeof(SiparisGecmisiSorgusu), typeof(DenetimSorgusu));

    [Theory]
    [MemberData(nameof(DisariVerilenDtolar))]
    public void Dto_lar_degistirilemez_kayitlardir(Type tur)
    {
        Assert.True(tur.IsSealed);
        var degistirilebilir = tur.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.SetMethod is { IsPublic: true } set
                && !set.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(IsExternalInit)))
            .Select(p => p.Name);

        Assert.Empty(degistirilebilir);
    }

    // ---- Denetim kaydında hassas veri yok ----

    [Fact]
    public async Task Kimlik_akislarindan_sonra_denetim_kayitlarinda_parola_ozet_tuz_ve_bilinmeyen_kullanici_adi_bulunmaz()
    {
        const string ilkParola = "Ilk-Gizli-Parola-11";
        const string yanlisParola = "Yanlis-Gizli-Parola-22";
        const string aliParolasi = "Ikinci-Gizli-Sifre-33";
        const string yeniParola = "Yeni-Gizli-Parola-44";
        const string bilinmeyen = "hayalet.kullanici";
        await using var ortam = await MatrisOrtami.KurAsync(tohumla: false);

        await ortam.CalistirAsync(s => S<IKimlikService>(s).IlkAdminOlusturAsync("yonetici", ilkParola));
        await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => ortam.CalistirAsync(s => S<IKimlikService>(s).GirisYapAsync("yonetici", yanlisParola)));
        await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => ortam.CalistirAsync(s => S<IKimlikService>(s).GirisYapAsync(bilinmeyen, ilkParola)));
        await ortam.CalistirAsync(s => S<IKimlikService>(s).GirisYapAsync("yonetici", ilkParola));
        KullaniciDto ali = null!;
        await ortam.CalistirAsync(async s => ali = await S<IKimlikService>(s).KullaniciOlusturAsync("ali", aliParolasi, KullaniciRolu.Operator));
        await Assert.ThrowsAsync<GecersizGirdiException>(() => ortam.CalistirAsync(s => S<IKimlikService>(s).ParolaDegistirAsync(yanlisParola, yeniParola)));
        await ortam.CalistirAsync(s => S<IKimlikService>(s).ParolaDegistirAsync(ilkParola, yeniParola));
        await ortam.CalistirAsync(s => S<IKimlikService>(s).RolDegistirAsync(ali.KullaniciId, KullaniciRolu.Admin));
        await ortam.CalistirAsync(s => S<IKimlikService>(s).AktiflikDegistirAsync(ali.KullaniciId, false));
        await ortam.CalistirAsync(s => S<IKimlikService>(s).CikisYapAsync());

        var yasak = new List<string> { ilkParola, yanlisParola, aliParolasi, yeniParola, bilinmeyen };
        foreach (var k in await ortam.KullanicilarAsync())
        {
            yasak.AddRange([Convert.ToBase64String(k.ParolaHash), Convert.ToHexString(k.ParolaHash), Convert.ToBase64String(k.ParolaTuzu), Convert.ToHexString(k.ParolaTuzu)]);
        }

        var kayitlar = await ortam.DenetimKayitlariAsync();
        Assert.Equal(9, kayitlar.Count);
        foreach (var kayit in kayitlar)
        {
            var metin = string.Join("\n", kayit.KullaniciAdi, kayit.VarlikTuru, kayit.VarlikAdi, kayit.EskiDeger, kayit.YeniDeger, kayit.Ozet);
            Assert.All(yasak, y => Assert.DoesNotContain(y, metin, StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain("parola=", metin, StringComparison.OrdinalIgnoreCase);
        }

        var bilinmeyenKaydi = Assert.Single(kayitlar, x => x.Islem == DenetimIslemi.GirisBasarisiz && x.KullaniciId is null);
        Assert.Null(bilinmeyenKaydi.VarlikAdi);
    }
}
