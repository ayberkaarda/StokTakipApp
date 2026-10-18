using Microsoft.Extensions.DependencyInjection;
using StokTakip.Application.Dtos;
using StokTakip.Application.Kimlik;
using StokTakip.Application.Raporlama;
using StokTakip.Application.Services;
using StokTakip.Domain;

namespace StokTakip.Data.Tests.SqlServer;

/// <summary>
/// Rapor ve sipariş geçmişi sorgularının SQL Server çevirisi (GroupBy Tarih.Date / Year-Month, long toplama,
/// sabit anahtarla gruplama, OFFSET/FETCH) ve gün sonu dahil tarih aralığı. Siparişler ham SQL ile, stok
/// sınırına takılmadan ve datetime2(7)'nin son anına (23:59:59.9999999) kadar yazılır.
/// </summary>
public sealed class SqlServerRaporTestleri(SqlServerVeritabani db) : IClassFixture<SqlServerVeritabani>, IAsyncLifetime
{
    private const long Iki = 2_000_000_000;

    private int _kalem;
    private int _silgi;

    public async Task InitializeAsync()
    {
        if (!db.Etkin)
        {
            return;
        }

        await db.SifirlaAsync();
        _kalem = await db.UrunEkleAsync("Kalem", 0);
        _silgi = await db.UrunEkleAsync("Silgi", 0);
        await db.KomutAsync($"""
            INSERT INTO Siparisler (UrunID, Adet, Tarih, IsDeleted) VALUES
            ({_kalem}, {Iki}, '2026-03-31T23:59:59.9999999', 0),
            ({_kalem}, {Iki}, '2026-04-01T00:00:00', 0),
            ({_silgi}, 5, '2026-04-01T08:00:00', 0),
            ({_kalem}, {Iki}, '2026-04-01T12:00:00', 0),
            ({_kalem}, 999, '2026-04-15T10:00:00', 1),
            ({_silgi}, 7, '2026-04-30T23:59:59.9999999', 0),
            ({_silgi}, 1, '2026-05-01T00:00:00', 0);
            """);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [SqlServerFact]
    public async Task En_cok_satanlar_long_toplamla_ve_gun_sonu_dahil_aralikla_hesaplanir()
    {
        using var kapsam = new SqlServisKapsami(db);

        var tumu = await kapsam.RaporService.EnCokSatanlariGetirAsync(null, null, 10);
        var nisanBiri = await kapsam.RaporService.EnCokSatanlariGetirAsync(new DateTime(2026, 4, 1, 15, 0, 0), new DateTime(2026, 4, 1), 10);
        var martSonu = await kapsam.RaporService.EnCokSatanlariGetirAsync(new DateTime(2026, 3, 31), new DateTime(2026, 3, 31), 10);
        var enGenis = await kapsam.RaporService.EnCokSatanlariGetirAsync(null, DateTime.MaxValue, 1);

        Assert.Equal(
            new[] { new UrunSatisOzetiDto(_kalem, "Kalem", 3 * Iki, 3), new UrunSatisOzetiDto(_silgi, "Silgi", 13, 3) },
            tumu);
        Assert.Equal(
            new[] { new UrunSatisOzetiDto(_kalem, "Kalem", 2 * Iki, 2), new UrunSatisOzetiDto(_silgi, "Silgi", 5, 1) },
            nisanBiri);
        Assert.Equal(new[] { new UrunSatisOzetiDto(_kalem, "Kalem", Iki, 1) }, martSonu);
        Assert.Equal(new[] { new UrunSatisOzetiDto(_kalem, "Kalem", 3 * Iki, 3) }, enGenis);
    }

    [SqlServerFact]
    public async Task Gunluk_ozet_Tarih_Date_ile_gruplanir_bos_gunler_doldurulur()
    {
        using var kapsam = new SqlServisKapsami(db);

        var ozet = await kapsam.RaporService.GunlukOzetAsync(new DateTime(2026, 3, 31, 18, 0, 0), new DateTime(2026, 4, 2));

        Assert.Equal(
            new[]
            {
                new DonemOzetiDto(new DateTime(2026, 3, 31), 1, Iki),
                new DonemOzetiDto(new DateTime(2026, 4, 1), 3, 2 * Iki + 5),
                new DonemOzetiDto(new DateTime(2026, 4, 2), 0, 0),
            },
            ozet);
    }

    [SqlServerFact]
    public async Task Aylik_ozet_yil_ve_ay_ile_gruplanir_ay_sonu_son_ani_dahildir()
    {
        using var kapsam = new SqlServisKapsami(db);

        var ozet = await kapsam.RaporService.AylikOzetAsync(new DateTime(2026, 3, 15), new DateTime(2026, 5, 31));

        Assert.Equal(
            new[]
            {
                new DonemOzetiDto(new DateTime(2026, 3, 1), 1, Iki),
                new DonemOzetiDto(new DateTime(2026, 4, 1), 4, 2 * Iki + 12),
                new DonemOzetiDto(new DateTime(2026, 5, 1), 1, 1),
            },
            ozet);
    }

    [SqlServerFact]
    public async Task Siparis_ozeti_sabit_anahtarli_gruplamayla_hesaplanir_int_toplam_doyurulur()
    {
        using var kapsam = new SqlServisKapsami(db);

        Assert.Equal((6, 3 * Iki + 13), await kapsam.SiparisRepository.GetOzetAsync());
        Assert.Equal(new SiparisOzetiDto(6, int.MaxValue), await kapsam.SiparisService.SiparisOzetiGetirAsync());

        await db.KomutAsync("UPDATE Siparisler SET IsDeleted = 1");
        Assert.Equal(new SiparisOzetiDto(0, 0), await kapsam.SiparisService.SiparisOzetiGetirAsync());
    }

    [SqlServerFact]
    public async Task Sayfali_siparis_gecmisi_tarih_azalan_bitis_gunu_sonuna_kadar_dahil_baslangic_ani_dahil()
    {
        using var kapsam = new SqlServisKapsami(db);
        var nisan = new SiparisGecmisiSorgusu(new DateTime(2026, 4, 1), new DateTime(2026, 4, 30), SayfaBoyutu: 3);

        var ilk = await kapsam.SiparisService.SiparisGecmisiSayfaGetirAsync(nisan);
        var ikinci = await kapsam.SiparisService.SiparisGecmisiSayfaGetirAsync(nisan with { Sayfa = 2 });
        var ucuncu = await kapsam.SiparisService.SiparisGecmisiSayfaGetirAsync(nisan with { Sayfa = 3 });
        var ogleden = await kapsam.SiparisService.SiparisGecmisiSayfaGetirAsync(nisan with { Baslangic = new DateTime(2026, 4, 1, 10, 0, 0), SayfaBoyutu = 50 });
        var sinirsiz = await kapsam.SiparisService.SiparisGecmisiSayfaGetirAsync(new SiparisGecmisiSorgusu(DateTime.MinValue, DateTime.MaxValue, SayfaBoyutu: 50));

        Assert.Equal((4, 2), (ilk.ToplamKayit, ilk.ToplamSayfa));
        Assert.Equal(
            new[] { new DateTime(2026, 4, 30, 23, 59, 59).AddTicks(9_999_999), new DateTime(2026, 4, 1, 12, 0, 0), new DateTime(2026, 4, 1, 8, 0, 0) },
            ilk.Ogeler.Select(x => x.Tarih));
        Assert.Equal(new[] { new DateTime(2026, 4, 1) }, ikinci.Ogeler.Select(x => x.Tarih));
        Assert.Empty(ucuncu.Ogeler);
        Assert.Equal(2, ogleden.ToplamKayit);
        Assert.Equal(6, sinirsiz.ToplamKayit);
        Assert.Equal(new[] { "Silgi", "Kalem", "Silgi" }, ilk.Ogeler.Select(x => x.UrunAdi));
    }

    [SqlServerFact]
    public async Task Tum_gecmis_ve_urun_filtresi_silinmisleri_haric_tutar()
    {
        using var kapsam = new SqlServisKapsami(db);

        var tumu = await kapsam.SiparisService.SiparisGecmisiniGetirAsync();
        var silgi = await kapsam.SiparisService.SiparisGecmisiSayfaGetirAsync(new SiparisGecmisiSorgusu(UrunId: _silgi));

        Assert.Equal(6, tumu.Count);
        Assert.Equal(tumu.OrderByDescending(x => x.Tarih).ThenByDescending(x => x.SiparisId), tumu);
        Assert.Equal(3, silgi.ToplamKayit);
        Assert.All(silgi.Ogeler, x => Assert.Equal(_silgi, x.UrunId));
    }

    [SqlServerFact]
    public async Task Raporlar_uretim_bilesiminde_Operator_oturumuyla_calisir()
    {
        var operatorKullanici = await db.KullaniciEkleAsync("ali", KullaniciRolu.Operator);
        await using var sp = db.UygulamaOlustur();
        sp.GetRequiredService<IOturumBaglami>().OturumAc(
            new OturumKullanicisi(operatorKullanici.KullaniciId, operatorKullanici.KullaniciAdi, operatorKullanici.Rol));

        await using var scope = sp.CreateAsyncScope();
        var rapor = scope.ServiceProvider.GetRequiredService<IRaporService>();
        Assert.IsType<YetkiDenetimliRaporService>(rapor);

        Assert.Equal(2, (await rapor.EnCokSatanlariGetirAsync(null, null, 5)).Count);
        Assert.Equal(31, (await rapor.GunlukOzetAsync(new DateTime(2026, 4, 1), new DateTime(2026, 5, 1))).Count);
        Assert.Equal(3, (await rapor.AylikOzetAsync(new DateTime(2026, 3, 1), new DateTime(2026, 5, 1))).Count);
        Assert.Equal(6, (await scope.ServiceProvider.GetRequiredService<ISiparisService>().SiparisOzetiGetirAsync()).ToplamSiparis);
    }
}
