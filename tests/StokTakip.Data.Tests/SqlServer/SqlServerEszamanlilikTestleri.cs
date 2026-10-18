using Microsoft.EntityFrameworkCore;
using StokTakip.Application.Dtos;
using StokTakip.Application.Kimlik;
using StokTakip.Data.Tests.Altyapi;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Data.Tests.SqlServer;

/// <summary>
/// Gerçek rowversion ve iki ayrı gerçek bağlantı. Yarışlar bariyerle kurulur: iki işlem de kaydı aynı sürümle okur,
/// sonra aynı anda yazar. SQLite'taki tek bağlantılı testlerin üretemediği satır kilidi beklemesi burada gerçekten olur.
/// </summary>
public sealed class SqlServerEszamanlilikTestleri(SqlServerVeritabani db) : IClassFixture<SqlServerVeritabani>, IAsyncLifetime
{
    private static readonly DateTime T0 = new(2026, 10, 18, 9, 0, 0, DateTimeKind.Utc);

    public Task InitializeAsync() => db.Etkin ? db.SifirlaAsync() : Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;

    private static async Task<object> SonucAsync<T>(Func<Task<T>> islem)
    {
        try
        {
            return (await islem())!;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    [SqlServerFact]
    public async Task Rowversion_her_guncellemede_veritabaninda_artar()
    {
        var urunId = await db.UrunEkleAsync("Kalem", 10);
        var ilk = (await db.UrunAsync(urunId)).RowVersion!;

        using (var kapsam = new SqlServisKapsami(db))
        {
            await kapsam.StokService.StokGuncelleAsync(urunId, 11);
        }

        var sonraki = (await db.UrunAsync(urunId)).RowVersion!;
        Assert.Equal(8, ilk.Length);
        Assert.True(BitConverter.ToUInt64(sonraki.Reverse().ToArray()) > BitConverter.ToUInt64(ilk.Reverse().ToArray()));
    }

    [SqlServerFact]
    public async Task Eski_rowversion_ile_iki_baglamdan_guncelleme_DbUpdateConcurrencyException_UnitOfWork_ile_EszamanlilikException_verir()
    {
        var urunId = await db.UrunEkleAsync("Kalem", 10);

        await using var a = db.ContextOlustur();
        await using var b = db.ContextOlustur();
        var urunA = await a.Urunler.AsTracking().SingleAsync(x => x.UrunId == urunId);
        var urunB = await b.Urunler.AsTracking().SingleAsync(x => x.UrunId == urunId);

        urunA.StokAyarla(20);
        await a.SaveChangesAsync();

        urunB.StokAyarla(30);
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => b.SaveChangesAsync());
        await Assert.ThrowsAsync<EszamanlilikException>(() => new UnitOfWork(b).SaveChangesAsync());

        Assert.Equal(20, (await db.UrunAsync(urunId)).Stok);
    }

    [SqlServerFact]
    public async Task UrunRepository_eski_surumle_stok_urun_guncelleme_ve_silme_EszamanlilikException_verir_veri_ezilmez()
    {
        var urunId = await db.UrunEkleAsync("Kalem", 10);
        var eski = (await db.UrunAsync(urunId)).RowVersion!;

        using (var kapsamA = new SqlServisKapsami(db))
        {
            await kapsamA.StokService.StokGuncelleAsync(urunId, 25, beklenenRowVersion: eski);
        }

        using (var kapsamB = new SqlServisKapsami(db))
        {
            await Assert.ThrowsAsync<EszamanlilikException>(() => kapsamB.StokService.StokGuncelleAsync(urunId, 3, beklenenRowVersion: eski));
            await Assert.ThrowsAsync<EszamanlilikException>(
                () => kapsamB.StokService.UrunGuncelleAsync(new UrunGuncelleIstegi(urunId, "Yeni Ad", 3), beklenenRowVersion: eski));
            await Assert.ThrowsAsync<EszamanlilikException>(() => kapsamB.StokService.UrunSilAsync(urunId, beklenenRowVersion: eski));

            // Başarısız işlemler aynı kapsamda yeniden gönderilmez.
            Assert.True((await kapsamB.StokService.UrunEkleAsync(new UrunEkleIstegi("Defter", 1))).UrunId > 0);
        }

        var urun = await db.UrunAsync(urunId);
        Assert.Equal((25, "Kalem"), (urun.Stok, urun.UrunAdi));
    }

    [SqlServerFact]
    public async Task SiparisRepository_urun_guncel_siparis_surumu_eskiyse_ikinci_silme_EszamanlilikException_alir()
    {
        var urunId = await db.UrunEkleAsync("Kalem", 10);
        int siparisId;
        using (var kapsam = new SqlServisKapsami(db))
        {
            siparisId = (await kapsam.SiparisService.SiparisOlusturAsync(new SiparisOlusturIstegi(urunId, 4))).SiparisId;
        }

        using var kapsamA = new SqlServisKapsami(db);
        using var kapsamB = new SqlServisKapsami(db);
        var siparisB = (await kapsamB.SiparisRepository.GetByIdAsync(siparisId))!;
        await kapsamA.SiparisService.SiparisSilAsync(siparisId);

        await kapsamB.Context.Entry(siparisB.Urun!).ReloadAsync();
        Assert.Equal(10, siparisB.Urun!.Stok);
        await Assert.ThrowsAsync<EszamanlilikException>(() => kapsamB.SiparisService.SiparisSilAsync(siparisId));

        Assert.Equal(10, (await db.UrunAsync(urunId)).Stok);
        Assert.True(Assert.Single(await db.TumSiparislerAsync()).IsDeleted);
    }

    [SqlServerFact]
    public async Task Son_stok_icin_iki_paralel_siparis_biri_basarili_digeri_reddedilir_stok_negatife_dusmez()
    {
        var urunId = await db.UrunEkleAsync("Son Kalem", 1);
        using var bariyer = new Barrier(2);

        async Task<object> DeneAsync()
        {
            using var kapsam = new SqlServisKapsami(db, urunSarmala: r => new BariyerliUrunRepository(r, bariyer));
            return await SonucAsync(() => kapsam.SiparisService.SiparisOlusturAsync(new SiparisOlusturIstegi(urunId, 1)));
        }

        var sonuclar = await Task.WhenAll(Task.Run(DeneAsync), Task.Run(DeneAsync));

        Assert.Single(sonuclar.OfType<SiparisDto>());
        var hata = Assert.Single(sonuclar.OfType<Exception>());
        Assert.True(hata is EszamanlilikException or YetersizStokException, $"Beklenmeyen hata: {hata}");
        Assert.Equal(0, (await db.UrunAsync(urunId)).Stok);
        Assert.Single(await db.TumSiparislerAsync());
        Assert.Single(await db.DenetimKayitlariAsync(), x => x.Islem == DenetimIslemi.SiparisOlusturuldu);
    }

    [SqlServerFact]
    public async Task Yirmi_paralel_siparis_bes_stoklu_urunde_stok_tutarli_kalir_hata_turleri_beklenendir()
    {
        var urunId = await db.UrunEkleAsync("Kalem", 5);
        using var baslat = new ManualResetEventSlim();

        var gorevler = Enumerable.Range(0, 20).Select(_ => Task.Run(async () =>
        {
            baslat.Wait();
            using var kapsam = new SqlServisKapsami(db);
            return await SonucAsync(() => kapsam.SiparisService.SiparisOlusturAsync(new SiparisOlusturIstegi(urunId, 1)));
        })).ToArray();
        baslat.Set();
        var sonuclar = await Task.WhenAll(gorevler);

        var basarili = sonuclar.OfType<SiparisDto>().Count();
        Assert.All(sonuclar.OfType<Exception>(), h => Assert.True(h is EszamanlilikException or YetersizStokException, $"Beklenmeyen hata: {h}"));
        Assert.InRange(basarili, 1, 5);
        Assert.Equal(5 - basarili, (await db.UrunAsync(urunId)).Stok);
        Assert.Equal(basarili, (await db.TumSiparislerAsync()).Count);
    }

    [SqlServerFact]
    public async Task Ayni_siparis_iki_baglamdan_paralel_silinirse_stok_yalnizca_bir_kez_iade_edilir()
    {
        var urunId = await db.UrunEkleAsync("Kalem", 10);
        int siparisId;
        using (var kapsam = new SqlServisKapsami(db))
        {
            siparisId = (await kapsam.SiparisService.SiparisOlusturAsync(new SiparisOlusturIstegi(urunId, 4))).SiparisId;
        }

        using var bariyer = new Barrier(2);

        async Task<object> SilAsync()
        {
            using var kapsam = new SqlServisKapsami(db, siparisSarmala: r => new BariyerliSiparisRepository(r, bariyer));
            return await SonucAsync(async () =>
            {
                await kapsam.SiparisService.SiparisSilAsync(siparisId);
                return "silindi";
            });
        }

        var sonuclar = await Task.WhenAll(Task.Run(SilAsync), Task.Run(SilAsync));

        Assert.Single(sonuclar.OfType<string>());
        var hata = Assert.Single(sonuclar.OfType<Exception>());
        Assert.True(hata is EszamanlilikException or GecersizIslemException, $"Beklenmeyen hata: {hata}");
        Assert.Equal(10, (await db.UrunAsync(urunId)).Stok);
        Assert.True(Assert.Single(await db.TumSiparislerAsync()).IsDeleted);
        Assert.Single(await db.DenetimKayitlariAsync(), x => x.Islem == DenetimIslemi.SiparisSilindi);
    }

    [SqlServerFact]
    public async Task Ilk_kurulum_yarisi_on_denetimi_ikisi_de_gecerse_biri_basarili_digeri_kurulum_zaten_tamamlandi_alir()
    {
        await IlkKurulumYarisiAsync(bekleyenCagri: 1);
    }

    /// <summary>
    /// İkisi de Serializable transaction içinde "kullanıcı yok" okur (aralık kilitleri tutulur), sonra ikisi de ekler:
    /// SQL Server birini kilitlenme kurbanı seçer. Kaybedenin hatası aynı mesaja çevrilmelidir.
    /// </summary>
    [SqlServerFact]
    public async Task Ilk_kurulum_yarisi_Serializable_transaction_icinde_kesisirse_biri_basarili_digeri_kurulum_zaten_tamamlandi_alir()
    {
        await IlkKurulumYarisiAsync(bekleyenCagri: 2);
    }

    private async Task IlkKurulumYarisiAsync(int bekleyenCagri)
    {
        using var bariyer = new Barrier(2);

        async Task<object> DeneAsync(string ad)
        {
            using var kapsam = new SqlServisKapsami(
                db, new SabitZamanSaglayici(T0), new OturumBaglami(), kullaniciSarmala: r => new BariyerliKullaniciRepository(r, bariyer, bekleyenCagri));
            return await SonucAsync(() => kapsam.KimlikService.IlkAdminOlusturAsync(ad, SqlServerVeritabani.Parola));
        }

        var sonuclar = await Task.WhenAll(Task.Run(() => DeneAsync("birinci")), Task.Run(() => DeneAsync("ikinci")));

        Assert.Single(sonuclar.OfType<KullaniciDto>());
        var hata = Assert.Single(sonuclar.OfType<Exception>());
        Assert.True(hata is GecersizIslemException, $"Beklenmeyen hata: {hata}");
        Assert.Equal("Kurulum zaten tamamlandı.", hata.Message);
        await using var context = db.ContextOlustur();
        Assert.Equal(1, await context.Kullanicilar.CountAsync());
        Assert.Single(await db.DenetimKayitlariAsync(), x => x.Islem == DenetimIslemi.IlkAdminOlusturuldu);
    }

    [SqlServerFact]
    public async Task Dort_paralel_yanlis_giriste_sayac_tam_dort_olur_kayip_guncelleme_yoktur()
    {
        var kayit = await ParalelYanlisGirisAsync(4);

        Assert.Equal(4, kayit.BasarisizDenemeSayisi);
        Assert.Null(kayit.KilitBitis);
    }

    [SqlServerFact]
    public async Task Sekiz_paralel_yanlis_giriste_sayac_tam_bes_olur_hesap_bir_kez_kilitlenir()
    {
        var kayit = await ParalelYanlisGirisAsync(8);

        Assert.Equal(Kullanici.KilitEsigi, kayit.BasarisizDenemeSayisi);
        Assert.NotNull(kayit.KilitBitis);
        Assert.Equal(T0 + Kullanici.KilitSuresi, DateTime.SpecifyKind(kayit.KilitBitis!.Value, DateTimeKind.Utc));
    }

    private async Task<Kullanici> ParalelYanlisGirisAsync(int adet)
    {
        await db.KullaniciEkleAsync("yonetici", KullaniciRolu.Admin);
        var ali = await db.KullaniciEkleAsync("ali", KullaniciRolu.Operator);
        using var baslat = new ManualResetEventSlim();

        var gorevler = Enumerable.Range(0, adet).Select(_ => Task.Run(async () =>
        {
            baslat.Wait();
            using var kapsam = new SqlServisKapsami(db, new SabitZamanSaglayici(T0), new OturumBaglami());
            return await SonucAsync(() => kapsam.KimlikService.GirisYapAsync("ali", "Yanlis-Parola-999"));
        })).ToArray();
        baslat.Set();
        var sonuclar = await Task.WhenAll(gorevler);

        Assert.All(sonuclar, s => Assert.IsType<GecersizKimlikBilgisiException>(s));
        var basarisizKayitlar = (await db.DenetimKayitlariAsync()).Where(x => x.Islem == DenetimIslemi.GirisBasarisiz).ToList();
        Assert.Equal(adet, basarisizKayitlar.Count);
        Assert.All(basarisizKayitlar, x => Assert.Equal(ali.KullaniciId, x.KullaniciId));

        await using var context = db.ContextOlustur();
        return await context.Kullanicilar.AsNoTracking().SingleAsync(x => x.KullaniciId == ali.KullaniciId);
    }
}
