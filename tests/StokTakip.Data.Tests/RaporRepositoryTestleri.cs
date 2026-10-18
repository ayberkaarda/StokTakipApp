using Microsoft.EntityFrameworkCore;
using StokTakip.Data.Repositories;
using StokTakip.Data.Tests.Altyapi;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Raporlar;

namespace StokTakip.Data.Tests;

/// <summary>RaporRepository, SQLite üzerinde gerçek sorgularla.</summary>
public class RaporRepositoryTestleri
{
    private static readonly DateTime Gun = new(2026, 5, 10);

    // ---- En çok satanlar ----

    [Fact]
    public async Task EnCokSatanlar_toplam_adet_azalan_esitlikte_urun_adi_artan()
    {
        using var db = new TestVeritabani();
        var beta = await Okuma.UrunEkleAsync(db, "Beta", 1000);
        var alfa = await Okuma.UrunEkleAsync(db, "Alfa", 1000);
        var gama = await Okuma.UrunEkleAsync(db, "Gama", 1000);
        var delta = await Okuma.UrunEkleAsync(db, "Delta", 1000);
        await SiparisEkleAsync(db, beta, Gun, 10);
        await SiparisEkleAsync(db, alfa, Gun, 4);
        await SiparisEkleAsync(db, alfa, Gun.AddHours(1), 6);
        await SiparisEkleAsync(db, gama, Gun, 15);
        await SiparisEkleAsync(db, delta, Gun, 3);

        var sonuc = await EnCokSatanlarAsync(db, null, null, 10);

        Assert.Equal(new[]
        {
            new UrunSatisOzeti(gama, "Gama", 15, 1),
            new UrunSatisOzeti(alfa, "Alfa", 10, 2),
            new UrunSatisOzeti(beta, "Beta", 10, 1),
            new UrunSatisOzeti(delta, "Delta", 3, 1),
        }, sonuc);
    }

    [Fact]
    public async Task EnCokSatanlar_silinmis_siparisler_haric()
    {
        using var db = new TestVeritabani();
        var x = await Okuma.UrunEkleAsync(db, "X", 1000);
        var y = await Okuma.UrunEkleAsync(db, "Y", 1000);
        var z = await Okuma.UrunEkleAsync(db, "Z", 1000);
        await SiparisEkleAsync(db, x, Gun, 5);
        await SilAsync(db, await SiparisEkleAsync(db, x, Gun, 100));
        await SiparisEkleAsync(db, y, Gun, 50);
        await SilAsync(db, await SiparisEkleAsync(db, z, Gun, 999));

        var sonuc = await EnCokSatanlarAsync(db, null, null, 10);

        Assert.Equal(new[]
        {
            new UrunSatisOzeti(y, "Y", 50, 1),
            new UrunSatisOzeti(x, "X", 5, 1),
        }, sonuc);
    }

    [Fact]
    public async Task EnCokSatanlar_bitis_gunu_tamamen_dahil_baslangic_gun_basindan_itibaren()
    {
        using var db = new TestVeritabani();
        var u = await Okuma.UrunEkleAsync(db, "Kalem", 1000);
        await SiparisEkleAsync(db, u, Gun.AddTicks(-1), 1);                 // önceki gün: hariç
        await SiparisEkleAsync(db, u, Gun, 2);                              // dahil
        await SiparisEkleAsync(db, u, Gun.AddDays(2).AddHours(23).AddMinutes(59).AddSeconds(59), 4); // bitiş günü: dahil
        await SiparisEkleAsync(db, u, Gun.AddDays(3).AddTicks(-1), 8);     // bitiş gününün son tick'i: dahil
        await SiparisEkleAsync(db, u, Gun.AddDays(3), 16);                  // ertesi gün gece yarısı: hariç

        var sonuc = await EnCokSatanlarAsync(db, Gun.AddHours(15), Gun.AddDays(2).AddHours(8), 10);

        var tek = Assert.Single(sonuc);
        Assert.Equal(14, tek.ToplamAdet);
        Assert.Equal(3, tek.SiparisSayisi);
    }

    [Fact]
    public async Task EnCokSatanlar_tek_tarih_sinirlari_ve_tarihsiz()
    {
        using var db = new TestVeritabani();
        var u = await Okuma.UrunEkleAsync(db, "Kalem", 1000);
        await SiparisEkleAsync(db, u, Gun.AddDays(-1), 1);
        await SiparisEkleAsync(db, u, Gun, 2);
        await SiparisEkleAsync(db, u, Gun.AddDays(1), 4);

        Assert.Equal(6, Assert.Single(await EnCokSatanlarAsync(db, Gun, null, 5)).ToplamAdet);
        Assert.Equal(3, Assert.Single(await EnCokSatanlarAsync(db, null, Gun, 5)).ToplamAdet);
        Assert.Equal(7, Assert.Single(await EnCokSatanlarAsync(db, null, null, 5)).ToplamAdet);
        Assert.Equal(7, Assert.Single(await EnCokSatanlarAsync(db, DateTime.MinValue, DateTime.MaxValue, 5)).ToplamAdet);
    }

    [Fact]
    public async Task EnCokSatanlar_adet_siniri_uygulanir()
    {
        using var db = new TestVeritabani();
        for (var i = 1; i <= 5; i++)
        {
            var u = await Okuma.UrunEkleAsync(db, $"U{i}", 1000);
            await SiparisEkleAsync(db, u, Gun, i * 10);
        }

        var uc = await EnCokSatanlarAsync(db, null, null, 3);
        var bir = await EnCokSatanlarAsync(db, null, null, 1);

        Assert.Equal(new[] { "U5", "U4", "U3" }, uc.Select(x => x.UrunAdi));
        Assert.Equal("U5", Assert.Single(bir).UrunAdi);
    }

    [Fact]
    public async Task Bos_veritabaninda_tum_raporlar_bos_doner()
    {
        using var db = new TestVeritabani();
        using var context = db.ContextOlustur();
        var depo = new RaporRepository(context);

        Assert.Empty(await depo.EnCokSatanlariGetirAsync(null, null, 10));
        Assert.Empty(await depo.GunlukOzetAsync(Gun, Gun.AddDays(30)));
        Assert.Empty(await depo.AylikOzetAsync(Gun, Gun.AddDays(30)));
    }

    // ---- Günlük özet ----

    [Fact]
    public async Task Gunluk_gune_gore_gruplar_bos_gunler_listede_yok_silinmisler_haric()
    {
        using var db = new TestVeritabani();
        var a = await Okuma.UrunEkleAsync(db, "A", 1000);
        var b = await Okuma.UrunEkleAsync(db, "B", 1000);
        await SiparisEkleAsync(db, a, Gun.AddHours(8), 3);
        await SiparisEkleAsync(db, b, Gun.AddHours(23).AddMinutes(59), 4);
        await SilAsync(db, await SiparisEkleAsync(db, a, Gun.AddHours(12), 100));
        await SilAsync(db, await SiparisEkleAsync(db, a, Gun.AddDays(1).AddHours(12), 100)); // yalnızca silinmiş: gün yok
        await SiparisEkleAsync(db, a, Gun.AddDays(3), 7);
        await SiparisEkleAsync(db, a, Gun.AddDays(5), 1);                                    // aralık dışı

        using var context = db.ContextOlustur();
        var sonuc = await new RaporRepository(context).GunlukOzetAsync(Gun, Gun.AddDays(4));

        Assert.Equal(new[]
        {
            new DonemOzeti(Gun, 2, 7),
            new DonemOzeti(Gun.AddDays(3), 1, 7),
        }, sonuc);
        Assert.All(sonuc, x => Assert.Equal(TimeSpan.Zero, x.Donem.TimeOfDay));
    }

    [Fact]
    public async Task Gunluk_bitis_gunu_dahil_ertesi_gun_haric()
    {
        using var db = new TestVeritabani();
        var a = await Okuma.UrunEkleAsync(db, "A", 1000);
        await SiparisEkleAsync(db, a, Gun.AddDays(1).AddTicks(-1), 2);
        await SiparisEkleAsync(db, a, Gun.AddDays(1), 5);

        using var context = db.ContextOlustur();
        var sonuc = await new RaporRepository(context).GunlukOzetAsync(Gun, Gun);

        Assert.Equal(new[] { new DonemOzeti(Gun, 1, 2) }, sonuc);
    }

    // ---- Aylık özet ----

    [Fact]
    public async Task Aylik_31_ocak_23_59_ve_1_subat_00_00_ayri_aylara_duser()
    {
        using var db = new TestVeritabani();
        var a = await Okuma.UrunEkleAsync(db, "A", 1000);
        await SiparisEkleAsync(db, a, new DateTime(2026, 1, 31, 23, 59, 0), 3);
        await SiparisEkleAsync(db, a, new DateTime(2026, 1, 31, 23, 59, 59).AddTicks(9_999_999), 1);
        await SiparisEkleAsync(db, a, new DateTime(2026, 2, 1, 0, 0, 0), 5);

        using var context = db.ContextOlustur();
        var depo = new RaporRepository(context);
        var ikiAy = await depo.AylikOzetAsync(new DateTime(2026, 1, 1), new DateTime(2026, 2, 28));
        var yalnizOcak = await depo.AylikOzetAsync(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31));
        var yalnizSubat = await depo.AylikOzetAsync(new DateTime(2026, 2, 1), new DateTime(2026, 2, 1));

        Assert.Equal(new[]
        {
            new DonemOzeti(new DateTime(2026, 1, 1), 2, 4),
            new DonemOzeti(new DateTime(2026, 2, 1), 1, 5),
        }, ikiAy);
        Assert.Equal(new[] { new DonemOzeti(new DateTime(2026, 1, 1), 2, 4) }, yalnizOcak);
        Assert.Equal(new[] { new DonemOzeti(new DateTime(2026, 2, 1), 1, 5) }, yalnizSubat);
    }

    [Fact]
    public async Task Aylik_yil_gecisi_ayni_ay_numarasi_farkli_yillar_karismaz_silinmisler_haric()
    {
        using var db = new TestVeritabani();
        var a = await Okuma.UrunEkleAsync(db, "A", 1000);
        await SiparisEkleAsync(db, a, new DateTime(2025, 1, 15), 1);
        await SiparisEkleAsync(db, a, new DateTime(2025, 12, 31, 23, 59, 59), 2);
        await SiparisEkleAsync(db, a, new DateTime(2026, 1, 1), 4);
        await SilAsync(db, await SiparisEkleAsync(db, a, new DateTime(2026, 1, 20), 50));
        await SilAsync(db, await SiparisEkleAsync(db, a, new DateTime(2025, 6, 1), 50));

        using var context = db.ContextOlustur();
        var sonuc = await new RaporRepository(context).AylikOzetAsync(new DateTime(2025, 1, 1), new DateTime(2026, 12, 31));

        Assert.Equal(new[]
        {
            new DonemOzeti(new DateTime(2025, 1, 1), 1, 1),
            new DonemOzeti(new DateTime(2025, 12, 1), 1, 2),
            new DonemOzeti(new DateTime(2026, 1, 1), 1, 4),
        }, sonuc);
    }

    [Fact]
    public async Task Aylik_baslangic_ay_ortasindaysa_ayin_onceki_gunleri_sayilmaz()
    {
        using var db = new TestVeritabani();
        var a = await Okuma.UrunEkleAsync(db, "A", 1000);
        await SiparisEkleAsync(db, a, new DateTime(2026, 3, 5), 10);
        await SiparisEkleAsync(db, a, new DateTime(2026, 3, 20), 1);

        using var context = db.ContextOlustur();
        var sonuc = await new RaporRepository(context).AylikOzetAsync(new DateTime(2026, 3, 15), new DateTime(2026, 3, 31));

        Assert.Equal(new[] { new DonemOzeti(new DateTime(2026, 3, 1), 1, 1) }, sonuc);
    }

    // ---- long toplam ----

    [Fact]
    public async Task Toplam_adet_int_sinirini_asinca_tasmadan_long_doner()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Toptan", int.MaxValue);
        await using (var context = db.ContextOlustur())
        {
            var urun = await context.Urunler.AsTracking().SingleAsync(x => x.UrunId == urunId);
            context.Siparisler.Add(Siparis.Olustur(urun, int.MaxValue, Gun.AddHours(1)));
            urun.StokAyarla(int.MaxValue);
            context.Siparisler.Add(Siparis.Olustur(urun, int.MaxValue, Gun.AddHours(2)));
            await context.SaveChangesAsync();
        }

        const long beklenen = 2L * int.MaxValue;
        using var okuma = db.ContextOlustur();
        var depo = new RaporRepository(okuma);

        var satis = Assert.Single(await depo.EnCokSatanlariGetirAsync(null, null, 10));
        var gunluk = Assert.Single(await depo.GunlukOzetAsync(Gun, Gun));
        var aylik = Assert.Single(await depo.AylikOzetAsync(Gun, Gun));

        Assert.Equal(beklenen, satis.ToplamAdet);
        Assert.Equal(2, satis.SiparisSayisi);
        Assert.Equal(beklenen, gunluk.ToplamAdet);
        Assert.Equal(beklenen, aylik.ToplamAdet);
    }

    // ---- Yardımcılar ----

    private static async Task<IReadOnlyList<UrunSatisOzeti>> EnCokSatanlarAsync(
        TestVeritabani db, DateTime? baslangic, DateTime? bitis, int adet)
    {
        using var context = db.ContextOlustur();
        return await new RaporRepository(context).EnCokSatanlariGetirAsync(baslangic, bitis, adet);
    }

    internal static async Task<int> SiparisEkleAsync(TestVeritabani db, int urunId, DateTime tarih, int adet)
    {
        await using var context = db.ContextOlustur();
        var urun = await context.Urunler.AsTracking().SingleAsync(x => x.UrunId == urunId);
        var siparis = Siparis.Olustur(urun, adet, tarih);
        context.Siparisler.Add(siparis);
        await context.SaveChangesAsync();
        return siparis.SiparisId;
    }

    internal static async Task SilAsync(TestVeritabani db, int siparisId)
    {
        await using var context = db.ContextOlustur();
        var siparis = await context.Siparisler.AsTracking().Include(x => x.Urun).SingleAsync(x => x.SiparisId == siparisId);
        siparis.Sil(siparis.Urun!);
        await context.SaveChangesAsync();
    }
}
