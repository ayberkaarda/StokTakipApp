using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using StokTakip.Application.Denetim;
using StokTakip.Application.Kimlik;
using StokTakip.Data.Repositories;
using StokTakip.Data.Tests.Altyapi;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Data.Tests;

public class DenetimKimlikTestleri
{
    private const string Parola = "Gizli-Parola-913";
    private static readonly DateTime T0 = new(2026, 10, 18, 9, 0, 0, DateTimeKind.Utc);

    private sealed class Kapsam : IDisposable
    {
        public Kapsam(KimlikTestVeritabani db, IOturumBaglami oturum, Func<IKullaniciRepository, IKullaniciRepository>? sarmala = null)
        {
            Context = db.ContextOlustur();
            var unitOfWork = new UnitOfWork(Context);
            var repository = new KullaniciRepository(Context);
            var kaydedici = new DenetimKaydedici(new DenetimRepository(Context), unitOfWork, oturum);
            Kimlik = new KimlikService(
                sarmala?.Invoke(repository) ?? repository, unitOfWork, TestParolaHasher.Instance, oturum,
                new SabitZamanSaglayici(T0), NullLogger<KimlikService>.Instance, kaydedici);
        }

        public StokTakipDbContext Context { get; }

        public KimlikService Kimlik { get; }

        public void Dispose() => Context.Dispose();
    }

    private static async Task<List<DenetimKaydi>> KayitlarAsync(KimlikTestVeritabani db)
    {
        await using var context = db.ContextOlustur();
        return await context.DenetimKayitlari.AsNoTracking().OrderBy(x => x.DenetimId).ToListAsync();
    }

    private static async Task<T> CalistirAsync<T>(KimlikTestVeritabani db, IOturumBaglami oturum, Func<KimlikService, Task<T>> islem)
    {
        using var kapsam = new Kapsam(db, oturum);
        return await islem(kapsam.Kimlik);
    }

    private static async Task CalistirAsync(KimlikTestVeritabani db, IOturumBaglami oturum, Func<KimlikService, Task> islem)
    {
        using var kapsam = new Kapsam(db, oturum);
        await islem(kapsam.Kimlik);
    }

    [Fact]
    public async Task Kimlik_islemleri_kaydedilir_parola_hicbir_kayitta_yoktur()
    {
        using var db = new KimlikTestVeritabani();
        var oturum = new OturumBaglami();
        var admin = await CalistirAsync(db, oturum, k => k.IlkAdminOlusturAsync("yonetici", Parola));
        await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => CalistirAsync(db, oturum, k => k.GirisYapAsync("yonetici", Parola + "x")));
        await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => CalistirAsync(db, oturum, k => k.GirisYapAsync("hayalet", Parola)));
        await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => CalistirAsync(db, oturum, k => k.GirisYapAsync("boşluklu ad\r\n", Parola)));
        await CalistirAsync(db, oturum, k => k.GirisYapAsync("YONETICI", Parola));
        var ali = await CalistirAsync(db, oturum, k => k.KullaniciOlusturAsync("ali", Parola + "-x7", KullaniciRolu.Operator));
        await CalistirAsync(db, oturum, k => k.RolDegistirAsync(ali.KullaniciId, KullaniciRolu.Admin));
        await CalistirAsync(db, oturum, k => k.AktiflikDegistirAsync(ali.KullaniciId, false));
        await Assert.ThrowsAsync<GecersizGirdiException>(() => CalistirAsync(db, oturum, k => k.ParolaDegistirAsync(Parola + "yanlis", "Yeni-Parola-555")));
        await CalistirAsync(db, oturum, k => k.ParolaDegistirAsync(Parola, "Yeni-Parola-555"));
        await CalistirAsync(db, oturum, k => k.CikisYapAsync());
        Assert.False(oturum.OturumAcikMi);

        var kayitlar = await KayitlarAsync(db);
        Assert.Equal(
            new[]
            {
                DenetimIslemi.IlkAdminOlusturuldu, DenetimIslemi.GirisBasarisiz, DenetimIslemi.GirisBasarisiz, DenetimIslemi.GirisBasarisiz,
                DenetimIslemi.GirisBasarili, DenetimIslemi.KullaniciOlusturuldu, DenetimIslemi.RolDegisti, DenetimIslemi.AktiflikDegisti,
                DenetimIslemi.ParolaDegisti, DenetimIslemi.CikisYapildi,
            },
            kayitlar.Select(x => x.Islem));

        var ilk = kayitlar[0];
        Assert.Equal((null, "sistem", admin.KullaniciId, "yonetici"), (ilk.KullaniciId, ilk.KullaniciAdi, ilk.VarlikId, ilk.VarlikAdi));

        var yanlisParola = kayitlar[1];
        Assert.Equal((admin.KullaniciId, "yonetici"), (yanlisParola.KullaniciId, yanlisParola.KullaniciAdi));
        var bilinmeyen = kayitlar[2];
        Assert.Null(bilinmeyen.KullaniciId);
        Assert.Contains("hayalet", bilinmeyen.Ozet);
        var gecersiz = kayitlar[3];
        Assert.Null(gecersiz.KullaniciId);
        Assert.DoesNotContain("boşluklu", gecersiz.Ozet + gecersiz.KullaniciAdi + gecersiz.YeniDeger + gecersiz.VarlikAdi);

        Assert.Equal((admin.KullaniciId, "yonetici"), (kayitlar[4].KullaniciId, kayitlar[4].KullaniciAdi));
        Assert.All(kayitlar.Skip(4), x => Assert.Equal(admin.KullaniciId, x.KullaniciId));
        Assert.Equal((ali.KullaniciId, "ali", "Rol=Operator"), (kayitlar[5].VarlikId, kayitlar[5].VarlikAdi, kayitlar[5].YeniDeger));
        Assert.Equal(("Operator", "Admin"), (kayitlar[6].EskiDeger, kayitlar[6].YeniDeger));
        Assert.Equal(("Aktif", "Pasif"), (kayitlar[7].EskiDeger, kayitlar[7].YeniDeger));
        Assert.Null(kayitlar[8].EskiDeger);
        Assert.Null(kayitlar[8].YeniDeger);

        foreach (var kayit in kayitlar)
        {
            var metin = string.Join("|", kayit.KullaniciAdi, kayit.VarlikTuru, kayit.VarlikAdi, kayit.EskiDeger, kayit.YeniDeger, kayit.Ozet);
            Assert.DoesNotContain("Gizli-Parola", metin, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Yeni-Parola", metin, StringComparison.OrdinalIgnoreCase);
        }

        await using var context = db.ContextOlustur();
        foreach (var kullanici in await context.Kullanicilar.AsNoTracking().ToListAsync())
        {
            var hash = Convert.ToBase64String(kullanici.ParolaHash);
            Assert.DoesNotContain(kayitlar, x => (x.Ozet + x.EskiDeger + x.YeniDeger).Contains(hash));
        }
    }

    [Fact]
    public async Task Giris_kaydi_yazilamazsa_oturum_acik_kalmaz()
    {
        using var db = new KimlikTestVeritabani();
        var oturum = new OturumBaglami();
        await CalistirAsync(db, oturum, k => k.IlkAdminOlusturAsync("yonetici", Parola));
        await using (var context = db.ContextOlustur())
        {
            await context.Database.ExecuteSqlRawAsync("DROP TABLE DenetimKayitlari");
        }

        await Assert.ThrowsAnyAsync<Exception>(() => CalistirAsync(db, oturum, k => k.GirisYapAsync("yonetici", Parola)));
        Assert.False(oturum.OturumAcikMi);
    }

    private sealed class BariyerliKullaniciRepository(IKullaniciRepository ic, Barrier bariyer) : IKullaniciRepository
    {
        private int _sayac;

        public async Task<bool> HerhangiBiriVarMiAsync(CancellationToken cancellationToken = default)
        {
            var sonuc = await ic.HerhangiBiriVarMiAsync(cancellationToken);
            if (Interlocked.Increment(ref _sayac) == 1)
            {
                await Task.Run(() => bariyer.SignalAndWait(TimeSpan.FromSeconds(30)), cancellationToken);
            }

            return sonuc;
        }

        public Task<Kullanici?> GetByIdAsync(int kullaniciId, CancellationToken cancellationToken = default) => ic.GetByIdAsync(kullaniciId, cancellationToken);

        public Task<KullaniciYetkiBilgisi?> YetkiBilgisiGetirAsync(int kullaniciId, CancellationToken cancellationToken = default) => ic.YetkiBilgisiGetirAsync(kullaniciId, cancellationToken);

        public Task<Kullanici?> GetByKullaniciAdiAsync(string kullaniciAdi, CancellationToken cancellationToken = default) => ic.GetByKullaniciAdiAsync(kullaniciAdi, cancellationToken);

        public Task<IReadOnlyList<Kullanici>> GetAllAsync(CancellationToken cancellationToken = default) => ic.GetAllAsync(cancellationToken);

        public Task<bool> KullaniciAdiKullaniliyorMuAsync(string kullaniciAdi, CancellationToken cancellationToken = default) => ic.KullaniciAdiKullaniliyorMuAsync(kullaniciAdi, cancellationToken);

        public Task<int> AktifAdminSayisiAsync(CancellationToken cancellationToken = default) => ic.AktifAdminSayisiAsync(cancellationToken);

        public Task AddAsync(Kullanici kullanici, CancellationToken cancellationToken = default) => ic.AddAsync(kullanici, cancellationToken);
    }

    [Fact]
    public async Task Ilk_kurulum_yarisinda_biri_basarili_digeri_kurulum_zaten_tamamlandi_alir()
    {
        using var db = new KimlikTestVeritabani(dosyada: true);
        using var bariyer = new Barrier(2);

        async Task<object> DeneAsync(string ad)
        {
            using var kapsam = new Kapsam(db, new OturumBaglami(), r => new BariyerliKullaniciRepository(r, bariyer));
            try
            {
                return await kapsam.Kimlik.IlkAdminOlusturAsync(ad, Parola);
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        var sonuclar = await Task.WhenAll(Task.Run(() => DeneAsync("birinci")), Task.Run(() => DeneAsync("ikinci")));

        Assert.Single(sonuclar.OfType<KullaniciDto>());
        var hata = Assert.IsType<GecersizIslemException>(Assert.Single(sonuclar.OfType<Exception>()));
        Assert.Equal("Kurulum zaten tamamlandı.", hata.Message);
        await using var context = db.ContextOlustur();
        Assert.Equal(1, await context.Kullanicilar.CountAsync());
        Assert.Single(await KayitlarAsync(db), x => x.Islem == DenetimIslemi.IlkAdminOlusturuldu);
    }
}
