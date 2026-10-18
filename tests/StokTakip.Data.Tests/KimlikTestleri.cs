using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StokTakip.Application.Kimlik;
using StokTakip.Data.Tests.Altyapi;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Data.Tests;

public class KimlikTestleri
{
    private const string Parola = "Dogru-Parola-42";
    private static readonly DateTime T0 = new(2026, 10, 18, 9, 0, 0, DateTimeKind.Utc);
    private readonly SabitZamanSaglayici _zaman = new(T0);

    private static async Task<Kullanici> OkuAsync(KimlikTestVeritabani db, string ad)
    {
        await using var context = db.ContextOlustur();
        return await context.Kullanicilar.AsNoTracking().SingleAsync(x => x.KullaniciAdi == ad);
    }

    private static OturumBaglami AdminOturumu(int adminId)
    {
        var oturum = new OturumBaglami();
        oturum.OturumAc(new OturumKullanicisi(adminId, "yonetici", KullaniciRolu.Admin));
        return oturum;
    }

    private async Task<int> AdminVeOperatorOlusturAsync(KimlikTestVeritabani db)
    {
        using var kapsam = new KimlikKapsami(db, _zaman);
        var admin = await kapsam.Kimlik.IlkAdminOlusturAsync("yonetici", Parola);
        kapsam.Oturum.OturumAc(new(admin.KullaniciId, admin.KullaniciAdi, admin.Rol));
        await kapsam.Kimlik.KullaniciOlusturAsync("ali", Parola, KullaniciRolu.Operator);
        return admin.KullaniciId;
    }

    private async Task<Exception> YanlisGirisAsync(KimlikTestVeritabani db, string ad = "ali")
    {
        using var kapsam = new KimlikKapsami(db, _zaman);
        return await Record.ExceptionAsync(() => kapsam.Kimlik.GirisYapAsync(ad, "yanlis-parola-1"));
    }

    [Fact]
    public async Task IlkKurulum_bos_veritabaninda_gerekli_ilk_admin_bir_kez_olusur_ve_giris_yapar()
    {
        using var db = new KimlikTestVeritabani();
        using (var kapsam = new KimlikKapsami(db, _zaman))
        {
            Assert.True(await kapsam.Kimlik.IlkKurulumGerekliMiAsync());
            await kapsam.Kimlik.IlkAdminOlusturAsync("Yonetici", Parola);
        }

        using (var kapsam = new KimlikKapsami(db, _zaman))
        {
            Assert.False(await kapsam.Kimlik.IlkKurulumGerekliMiAsync());
            await Assert.ThrowsAsync<GecersizIslemException>(() => kapsam.Kimlik.IlkAdminOlusturAsync("ikinci", Parola));
        }

        using (var kapsam = new KimlikKapsami(db, _zaman))
        {
            var dto = await kapsam.Kimlik.GirisYapAsync("YONETICI", Parola);
            Assert.Equal(KullaniciRolu.Admin, kapsam.Oturum.MevcutKullanici!.Rol);
            Assert.Equal(dto.KullaniciId, kapsam.Oturum.MevcutKullanici.KullaniciId);
        }

        var kayit = await OkuAsync(db, "yonetici");
        Assert.Equal(KullaniciRolu.Admin, kayit.Rol);
        Assert.Equal(32, kayit.ParolaHash.Length);
        Assert.Equal(16, kayit.ParolaTuzu.Length);
        Assert.Equal(TestParolaHasher.Iterasyon, kayit.Iterasyon);
        Assert.Equal(T0, kayit.SonGirisTarihi);
        Assert.NotNull(kayit.RowVersion);
        await using var context = db.ContextOlustur();
        Assert.Equal(1, await context.Kullanicilar.CountAsync());
    }

    [Fact]
    public async Task Kullanici_adi_benzersiz_buyuk_kucuk_harf_duyarsiz_servis_ve_indeks_engeller()
    {
        using var db = new KimlikTestVeritabani();
        var adminId = await AdminVeOperatorOlusturAsync(db);

        using (var kapsam = new KimlikKapsami(db, _zaman, AdminOturumu(adminId)))
        {
            var hata = await Assert.ThrowsAsync<GecersizGirdiException>(
                () => kapsam.Kimlik.KullaniciOlusturAsync(" ALI ", Parola, KullaniciRolu.Admin));
            Assert.Equal("'ali' kullanıcı adı zaten kullanılıyor.", hata.Message);
        }

        await using var context = db.ContextOlustur();
        var ex = await Assert.ThrowsAsync<SqliteException>(() => context.Database.ExecuteSqlAsync(
            $"INSERT INTO Kullanicilar (KullaniciAdi, ParolaHash, ParolaTuzu, Iterasyon, Rol, Aktif, BasarisizDenemeSayisi) VALUES ('ali', x'01', x'01', 1, 0, 1, 0)"));
        Assert.Contains("UNIQUE", ex.Message);
        Assert.Equal(2, await context.Kullanicilar.CountAsync());
    }

    [Theory]
    [InlineData("'x1'", "2", "1", "0", "CK_Kullanicilar_Rol")]
    [InlineData("'x2'", "0", "0", "0", "CK_Kullanicilar_Iterasyon")]
    [InlineData("'x3'", "0", "1", "-1", "CK_Kullanicilar_BasarisizDenemeSayisi")]
    [InlineData("''", "0", "1", "0", "CK_Kullanicilar_KullaniciAdi")]
    public async Task CHECK_kisitlari_gecersiz_satirlari_reddeder(string ad, string rol, string iterasyon, string sayac, string kisit)
    {
        using var db = new KimlikTestVeritabani();
        await using var context = db.ContextOlustur();

        var sql = "INSERT INTO Kullanicilar (KullaniciAdi, ParolaHash, ParolaTuzu, Iterasyon, Rol, Aktif, BasarisizDenemeSayisi) "
            + $"VALUES ({ad}, x'01', x'01', {iterasyon}, {rol}, 1, {sayac})";
        var ex = await Assert.ThrowsAsync<SqliteException>(() => context.Database.ExecuteSqlRawAsync(sql));

        Assert.Contains(kisit, ex.Message);
    }

    [Fact]
    public async Task Kilit_kapsamlar_arasinda_kalici_kilitliyken_dogru_parola_reddedilir_sure_dolunca_sifirlanir()
    {
        using var db = new KimlikTestVeritabani();
        await AdminVeOperatorOlusturAsync(db);

        for (var i = 0; i < 5; i++)
        {
            Assert.IsType<GecersizKimlikBilgisiException>(await YanlisGirisAsync(db));
        }

        var kilitli = await OkuAsync(db, "ali");
        Assert.Equal(5, kilitli.BasarisizDenemeSayisi);
        Assert.Equal(T0.AddMinutes(15), kilitli.KilitBitis);

        using (var kapsam = new KimlikKapsami(db, _zaman))
        {
            await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => kapsam.Kimlik.GirisYapAsync("ali", Parola));
        }

        Assert.Equal(5, (await OkuAsync(db, "ali")).BasarisizDenemeSayisi);

        _zaman.Simdi = T0.AddMinutes(15);
        using (var kapsam = new KimlikKapsami(db, _zaman))
        {
            await kapsam.Kimlik.GirisYapAsync("ali", Parola);
        }

        var acik = await OkuAsync(db, "ali");
        Assert.Equal(0, acik.BasarisizDenemeSayisi);
        Assert.Null(acik.KilitBitis);
        Assert.Equal(T0.AddMinutes(15), acik.SonGirisTarihi);
    }

    [Fact]
    public async Task Bayat_kullanici_kaydi_kaydedilirse_eszamanlilik_hatasi_verir()
    {
        using var db = new KimlikTestVeritabani();
        await AdminVeOperatorOlusturAsync(db);

        using var a = new KimlikKapsami(db, _zaman);
        var bayat = await a.Repository.GetByKullaniciAdiAsync("ali");
        Assert.IsType<GecersizKimlikBilgisiException>(await YanlisGirisAsync(db));

        bayat!.BasarisizGirisKaydet(T0);
        await Assert.ThrowsAsync<EszamanlilikException>(() => a.UnitOfWork.SaveChangesAsync());
        Assert.Equal(1, (await OkuAsync(db, "ali")).BasarisizDenemeSayisi);
    }

    [Fact]
    public async Task Araya_giren_yanlis_deneme_kaybolmaz_sayac_ikiye_cikar()
    {
        using var db = new KimlikTestVeritabani();
        await AdminVeOperatorOlusturAsync(db);

        using var a = new KimlikKapsami(db, _zaman, sarmala: ic => new AraGirenRepository(ic, async () =>
            Assert.IsType<GecersizKimlikBilgisiException>(await YanlisGirisAsync(db))));

        await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => a.Kimlik.GirisYapAsync("ali", "yanlis-parola-1"));

        Assert.Equal(2, (await OkuAsync(db, "ali")).BasarisizDenemeSayisi);
    }

    [Fact]
    public async Task Araya_giren_dorduncu_denemeden_sonra_besinci_deneme_hesabi_kilitler()
    {
        using var db = new KimlikTestVeritabani();
        await AdminVeOperatorOlusturAsync(db);
        for (var i = 0; i < 3; i++) await YanlisGirisAsync(db);

        using var a = new KimlikKapsami(db, _zaman, sarmala: ic => new AraGirenRepository(ic, () => YanlisGirisAsync(db)));
        await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => a.Kimlik.GirisYapAsync("ali", "yanlis-parola-1"));

        var kayit = await OkuAsync(db, "ali");
        Assert.Equal(5, kayit.BasarisizDenemeSayisi);
        Assert.Equal(T0.AddMinutes(15), kayit.KilitBitis);
    }

    [Fact]
    public async Task Paralel_yanlis_denemelerde_sayac_tam_bes_olur_ve_hesap_kilitlenir()
    {
        using var db = new KimlikTestVeritabani(dosyada: true);
        await AdminVeOperatorOlusturAsync(db);

        using var baslat = new ManualResetEventSlim();
        var gorevler = Enumerable.Range(0, 8).Select(_ => Task.Run(async () =>
        {
            baslat.Wait();
            return await YanlisGirisAsync(db);
        })).ToArray();
        baslat.Set();
        var hatalar = await Task.WhenAll(gorevler);

        Assert.All(hatalar, h => Assert.IsType<GecersizKimlikBilgisiException>(h));
        var kayit = await OkuAsync(db, "ali");
        Assert.Equal(5, kayit.BasarisizDenemeSayisi);
        Assert.Equal(T0.AddMinutes(15), kayit.KilitBitis);
    }

    [Fact]
    public async Task Son_aktif_admin_sqlite_uzerinde_de_korunur()
    {
        using var db = new KimlikTestVeritabani();
        var adminId = await AdminVeOperatorOlusturAsync(db);

        using (var kapsam = new KimlikKapsami(db, _zaman, AdminOturumu(adminId)))
        {
            await Assert.ThrowsAsync<GecersizIslemException>(
                () => kapsam.Kimlik.RolDegistirAsync(adminId, KullaniciRolu.Operator));
        }

        using (var kapsam = new KimlikKapsami(db, _zaman, AdminOturumu(adminId)))
        {
            await Assert.ThrowsAsync<GecersizIslemException>(() => kapsam.Kimlik.AktiflikDegistirAsync(adminId, false));
        }

        var ali = await OkuAsync(db, "ali");
        using (var kapsam = new KimlikKapsami(db, _zaman, AdminOturumu(adminId)))
        {
            await kapsam.Kimlik.RolDegistirAsync(ali.KullaniciId, KullaniciRolu.Admin);
        }

        using (var kapsam = new KimlikKapsami(db, _zaman, AdminOturumu(adminId)))
        {
            await kapsam.Kimlik.AktiflikDegistirAsync(adminId, false);
        }

        Assert.False((await OkuAsync(db, "yonetici")).Aktif);
        Assert.Equal(KullaniciRolu.Admin, (await OkuAsync(db, "ali")).Rol);
        using (var kapsam = new KimlikKapsami(db, _zaman))
        {
            var hata = await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => kapsam.Kimlik.GirisYapAsync("yonetici", Parola));
            Assert.Equal(GecersizKimlikBilgisiException.GenelMesaj, hata.Message);
        }
    }

    [Fact]
    public void Kullanici_RowVersion_uretim_modelinde_eszamanlilik_belirteci()
    {
        using var db = new TestVeritabani();
        using var context = db.ContextOlustur();
        var property = context.Model.FindEntityType(typeof(Kullanici))!.FindProperty(nameof(Kullanici.RowVersion))!;
        Assert.True(property.IsConcurrencyToken);
    }

    /// <summary>İlk kullanıcı okumasından hemen sonra, kayıt geri dönmeden önce verilen işlemi çalıştırır.</summary>
    private sealed class AraGirenRepository(IKullaniciRepository ic, Func<Task> arayaGiren) : IKullaniciRepository
    {
        private bool _calisti;

        public async Task<Kullanici?> GetByKullaniciAdiAsync(string kullaniciAdi, CancellationToken cancellationToken = default)
        {
            var kullanici = await ic.GetByKullaniciAdiAsync(kullaniciAdi, cancellationToken);
            if (!_calisti)
            {
                _calisti = true;
                await arayaGiren();
            }

            return kullanici;
        }

        public Task<Kullanici?> GetByIdAsync(int kullaniciId, CancellationToken cancellationToken = default) => ic.GetByIdAsync(kullaniciId, cancellationToken);

        public Task<KullaniciYetkiBilgisi?> YetkiBilgisiGetirAsync(int kullaniciId, CancellationToken cancellationToken = default)
            => ic.YetkiBilgisiGetirAsync(kullaniciId, cancellationToken);

        public Task<IReadOnlyList<Kullanici>> GetAllAsync(CancellationToken cancellationToken = default) => ic.GetAllAsync(cancellationToken);

        public Task<bool> HerhangiBiriVarMiAsync(CancellationToken cancellationToken = default) => ic.HerhangiBiriVarMiAsync(cancellationToken);

        public Task<bool> KullaniciAdiKullaniliyorMuAsync(string kullaniciAdi, CancellationToken cancellationToken = default)
            => ic.KullaniciAdiKullaniliyorMuAsync(kullaniciAdi, cancellationToken);

        public Task<int> AktifAdminSayisiAsync(CancellationToken cancellationToken = default) => ic.AktifAdminSayisiAsync(cancellationToken);

        public Task AddAsync(Kullanici kullanici, CancellationToken cancellationToken = default) => ic.AddAsync(kullanici, cancellationToken);
    }
}
