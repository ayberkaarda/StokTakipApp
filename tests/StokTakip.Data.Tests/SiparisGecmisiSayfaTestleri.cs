using Microsoft.EntityFrameworkCore;
using StokTakip.Data.Tests.Altyapi;
using StokTakip.Domain;
using StokTakip.Domain.Entities;

namespace StokTakip.Data.Tests;

/// <summary>SiparisRepository.GetGecmisSayfaAsync ve SiparisService.SiparisGecmisiSayfaGetirAsync, SQLite üzerinde.</summary>
public class SiparisGecmisiSayfaTestleri
{
    private static readonly DateTime Gun = new(2026, 5, 10);

    // ---- Tarih aralığı ----

    [Fact]
    public async Task Bitis_gununun_tamami_dahildir_ertesi_gun_gece_yarisi_haric()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 1000);
        var oncekiGunSonu = await SiparisEkleAsync(db, urunId, Gun.AddTicks(-1));
        var gunBasi = await SiparisEkleAsync(db, urunId, Gun);
        var gun2359 = await SiparisEkleAsync(db, urunId, Gun.AddHours(23).AddMinutes(59));
        var gunSonTick = await SiparisEkleAsync(db, urunId, Gun.AddDays(1).AddTicks(-1));
        var ertesiGun = await SiparisEkleAsync(db, urunId, Gun.AddDays(1));

        var sonuc = await SayfaAsync(db, new SiparisFiltresi(Bitis: Gun), 1, 50);

        Assert.Equal(new[] { gunSonTick, gun2359, gunBasi, oncekiGunSonu }, Idler(sonuc));
        Assert.Equal(4, sonuc.ToplamKayit);
        Assert.DoesNotContain(ertesiGun, Idler(sonuc));
    }

    [Fact]
    public async Task Bitis_saat_icerse_de_o_gunun_tamami_dahildir()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 1000);
        var aksam = await SiparisEkleAsync(db, urunId, Gun.AddHours(23).AddMinutes(59));
        await SiparisEkleAsync(db, urunId, Gun.AddDays(1));

        var sonuc = await SayfaAsync(db, new SiparisFiltresi(Bitis: Gun.AddHours(8)), 1, 50);

        Assert.Equal(new[] { aksam }, Idler(sonuc));
    }

    [Fact]
    public async Task Baslangic_ani_dahildir_oncesi_haric()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 1000);
        await SiparisEkleAsync(db, urunId, Gun.AddTicks(-1));
        var tam = await SiparisEkleAsync(db, urunId, Gun);
        var sonra = await SiparisEkleAsync(db, urunId, Gun.AddDays(3));

        var sonuc = await SayfaAsync(db, new SiparisFiltresi(Baslangic: Gun), 1, 50);

        Assert.Equal(new[] { sonra, tam }, Idler(sonuc));
    }

    [Fact]
    public async Task Ayni_gun_baslangic_ve_bitis_yalnizca_o_gunu_dondurur()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 1000);
        await SiparisEkleAsync(db, urunId, Gun.AddMinutes(-1));
        var sabah = await SiparisEkleAsync(db, urunId, Gun.AddHours(9));
        var gece = await SiparisEkleAsync(db, urunId, Gun.AddHours(23).AddMinutes(59).AddSeconds(59));
        await SiparisEkleAsync(db, urunId, Gun.AddDays(1));

        var sonuc = await SayfaAsync(db, new SiparisFiltresi(Gun, Gun), 1, 50);

        Assert.Equal(new[] { gece, sabah }, Idler(sonuc));
    }

    [Fact]
    public async Task Uc_deger_tarihler_hata_vermez()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 1000);
        var id = await SiparisEkleAsync(db, urunId, Gun);

        var sonuc = await SayfaAsync(db, new SiparisFiltresi(DateTime.MinValue, DateTime.MaxValue), 1, 50);

        Assert.Equal(new[] { id }, Idler(sonuc));
    }

    [Fact]
    public async Task Servis_uzerinden_bitis_gunu_dahil_ve_urun_adi_dolu_doner()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 1000);
        var gece = await SiparisEkleAsync(db, urunId, Gun.AddHours(23).AddMinutes(59), adet: 4);
        await SiparisEkleAsync(db, urunId, Gun.AddDays(1));

        using var kapsam = new ServisKapsami(db);
        var sonuc = await kapsam.SiparisService.SiparisGecmisiSayfaGetirAsync(new(Gun, Gun, "  kal  "));

        var dto = Assert.Single(sonuc.Ogeler);
        Assert.Equal(gece, dto.SiparisId);
        Assert.Equal(urunId, dto.UrunId);
        Assert.Equal("Kalem", dto.UrunAdi);
        Assert.Equal(4, dto.Adet);
        Assert.Equal(Gun.AddHours(23).AddMinutes(59), dto.Tarih);
        Assert.Equal(1, sonuc.ToplamKayit);
        Assert.Equal(1, sonuc.ToplamSayfa);
    }

    // ---- Ürün adı araması ----

    [Theory]
    [InlineData("Kalem")]
    [InlineData("kalem")]
    [InlineData("KALEM")]
    [InlineData("aLe")]
    public async Task Urun_adi_aramasi_buyuk_kucuk_harf_duyarsiz_ve_kismidir(string arama)
    {
        using var db = new TestVeritabani();
        var mavi = await SiparisEkleAsync(db, await Okuma.UrunEkleAsync(db, "Mavi KALEM", 100), Gun);
        var uc = await SiparisEkleAsync(db, await Okuma.UrunEkleAsync(db, "kalem ucu", 100), Gun.AddHours(1));
        await SiparisEkleAsync(db, await Okuma.UrunEkleAsync(db, "Defter", 100), Gun.AddHours(2));

        var sonuc = await SayfaAsync(db, new SiparisFiltresi(UrunAdi: arama), 1, 50);

        Assert.Equal(new[] { uc, mavi }, Idler(sonuc));
        Assert.Equal(2, sonuc.ToplamKayit);
    }

    [Theory]
    [InlineData("%", "100% Pamuk", "1000 Pamuk")]
    [InlineData("100%", "100% Pamuk", "1000 Pamuk")]
    [InlineData("_", "a_b Vida", "axb Vida")]
    [InlineData("a_b", "a_b Vida", "axb Vida")]
    [InlineData("[", "[A] Kutu", "A Kutu")]
    [InlineData("[A]", "[A] Kutu", "A Kutu")]
    [InlineData("[a-z]", "[a-z] Etiket", "b Etiket")]
    [InlineData("~", "x~y Kablo", "xy Kablo")]
    [InlineData("~y", "x~y Kablo", "xy Kablo")]
    [InlineData("~%", "z~%w Boya", "z%w Boya")]
    [InlineData("~_", "q~_r Bant", "q_r Bant")]
    public async Task Ozel_karakterler_literal_aranir_joker_gibi_davranmaz(string arama, string eslesen, string eslesmeyen)
    {
        using var db = new TestVeritabani();
        var hedef = await SiparisEkleAsync(db, await Okuma.UrunEkleAsync(db, eslesen, 10), Gun);
        await SiparisEkleAsync(db, await Okuma.UrunEkleAsync(db, eslesmeyen, 10), Gun.AddMinutes(1));
        await SiparisEkleAsync(db, await Okuma.UrunEkleAsync(db, "Defter", 10), Gun.AddMinutes(2));

        var sonuc = await SayfaAsync(db, new SiparisFiltresi(UrunAdi: arama), 1, 50);

        Assert.Equal(new[] { hedef }, Idler(sonuc));
        Assert.Equal(1, sonuc.ToplamKayit);
        Assert.Equal(eslesen, Assert.Single(sonuc.Ogeler).Urun!.UrunAdi);
    }

    [Fact]
    public async Task Eslesmeyen_urun_adi_bos_sonuc_ve_sifir_kayit_doner()
    {
        using var db = new TestVeritabani();
        await SiparisEkleAsync(db, await Okuma.UrunEkleAsync(db, "Kalem", 10), Gun);

        var sonuc = await SayfaAsync(db, new SiparisFiltresi(UrunAdi: "Silgi"), 1, 50);

        Assert.Empty(sonuc.Ogeler);
        Assert.Equal(0, sonuc.ToplamKayit);
        Assert.Equal(0, sonuc.ToplamSayfa);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Bos_urun_adi_filtre_uygulamaz(string? arama)
    {
        using var db = new TestVeritabani();
        await SiparisEkleAsync(db, await Okuma.UrunEkleAsync(db, "Kalem", 10), Gun);
        await SiparisEkleAsync(db, await Okuma.UrunEkleAsync(db, "Defter", 10), Gun);

        var sonuc = await SayfaAsync(db, new SiparisFiltresi(UrunAdi: arama), 1, 50);

        Assert.Equal(2, sonuc.ToplamKayit);
    }

    [Fact]
    public async Task Tarih_urun_adi_ve_UrunId_filtreleri_birlikte_uygulanir()
    {
        using var db = new TestVeritabani();
        var kalem = await Okuma.UrunEkleAsync(db, "Kalem", 100);
        var kursun = await Okuma.UrunEkleAsync(db, "Kursun Kalem", 100);
        await SiparisEkleAsync(db, kalem, Gun.AddDays(-1));
        var hedef = await SiparisEkleAsync(db, kalem, Gun.AddHours(12));
        await SiparisEkleAsync(db, kursun, Gun.AddHours(13));
        await SiparisEkleAsync(db, kalem, Gun.AddDays(1));

        var sonuc = await SayfaAsync(db, new SiparisFiltresi(Gun, Gun, "kalem", kalem), 1, 50);

        Assert.Equal(new[] { hedef }, Idler(sonuc));
        Assert.Equal(1, sonuc.ToplamKayit);
    }

    // ---- Silinmiş siparişler ----

    [Fact]
    public async Task Silinmis_siparisler_sonuca_ve_toplam_kayda_girmez()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 100);
        var a = await SiparisEkleAsync(db, urunId, Gun);
        var b = await SiparisEkleAsync(db, urunId, Gun.AddHours(1));
        var c = await SiparisEkleAsync(db, urunId, Gun.AddHours(2));
        using (var kapsam = new ServisKapsami(db))
        {
            await kapsam.SiparisService.SiparisSilAsync(b);
        }

        var tumu = await SayfaAsync(db, new SiparisFiltresi(), 1, 50);
        var filtreli = await SayfaAsync(db, new SiparisFiltresi(Gun, Gun, "kal"), 1, 1);

        Assert.Equal(new[] { c, a }, Idler(tumu));
        Assert.Equal(2, tumu.ToplamKayit);
        Assert.Equal(2, filtreli.ToplamKayit);
        Assert.Equal(2, filtreli.ToplamSayfa);
        Assert.Equal(new[] { c }, Idler(filtreli));
        Assert.Equal(new[] { a }, Idler(await SayfaAsync(db, new SiparisFiltresi(Gun, Gun, "kal"), 2, 1)));
    }

    // ---- Sıralama ----

    [Fact]
    public async Task Tarihe_gore_azalan_siralanir_eklenme_sirasindan_bagimsiz()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 100);
        var orta = await SiparisEkleAsync(db, urunId, Gun.AddDays(1));
        var eski = await SiparisEkleAsync(db, urunId, Gun);
        var yeni = await SiparisEkleAsync(db, urunId, Gun.AddDays(2));

        var sonuc = await SayfaAsync(db, new SiparisFiltresi(), 1, 50);

        Assert.Equal(new[] { yeni, orta, eski }, Idler(sonuc));
    }

    [Fact]
    public async Task Esit_tarihte_SiparisId_azalan_ile_kararli_siralanir_ve_sayfalar_arasi_tutarlidir()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 100);
        var idler = await TopluSiparisEkleAsync(db, urunId, Enumerable.Repeat(Gun, 7).ToList());

        var beklenen = idler.OrderByDescending(x => x).ToList();
        var birlesik = new List<int>();
        for (var sayfa = 1; sayfa <= 4; sayfa++)
            birlesik.AddRange(Idler(await SayfaAsync(db, new SiparisFiltresi(), sayfa, 2)));

        Assert.Equal(beklenen, birlesik);
        // Aynı sorgu tekrarlandığında aynı sıra gelir.
        Assert.Equal(beklenen.Take(2), Idler(await SayfaAsync(db, new SiparisFiltresi(), 1, 2)));
    }

    // ---- Sayfa sınırları ----

    [Fact]
    public async Task Son_sayfa_kismi_aralik_disi_sayfa_bos_liste_ve_dogru_toplam_doner()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 100);
        var idler = await TopluSiparisEkleAsync(db, urunId,
            Enumerable.Range(0, 5).Select(i => Gun.AddMinutes(i)).ToList());

        var son = await SayfaAsync(db, new SiparisFiltresi(), 3, 2);
        var disarida = await SayfaAsync(db, new SiparisFiltresi(), 4, 2);
        var cokUzak = await SayfaAsync(db, new SiparisFiltresi(), int.MaxValue, 200);

        Assert.Equal(new[] { idler[0] }, Idler(son));
        Assert.Equal(5, son.ToplamKayit);
        Assert.Equal(3, son.ToplamSayfa);
        Assert.Equal(3, son.Sayfa);
        Assert.Equal(2, son.SayfaBoyutu);

        Assert.Empty(disarida.Ogeler);
        Assert.Equal(5, disarida.ToplamKayit);
        Assert.Equal(3, disarida.ToplamSayfa);
        Assert.Equal(4, disarida.Sayfa);

        Assert.Empty(cokUzak.Ogeler);
        Assert.Equal(5, cokUzak.ToplamKayit);
    }

    [Fact]
    public async Task Sayfa_boyutu_kayit_sayisina_tam_bolununce_son_sayfa_dolu_sonraki_bos()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 100);
        await TopluSiparisEkleAsync(db, urunId, Enumerable.Range(0, 6).Select(i => Gun.AddMinutes(i)).ToList());

        Assert.Equal(3, (await SayfaAsync(db, new SiparisFiltresi(), 2, 3)).Ogeler.Count);
        var sonraki = await SayfaAsync(db, new SiparisFiltresi(), 3, 3);
        Assert.Empty(sonraki.Ogeler);
        Assert.Equal(2, sonraki.ToplamSayfa);
    }

    [Fact]
    public async Task Bos_veritabaninda_ilk_sayfa_bos_ve_sifir_sayfa()
    {
        using var db = new TestVeritabani();

        var sonuc = await SayfaAsync(db, new SiparisFiltresi(), 1, 20);

        Assert.Empty(sonuc.Ogeler);
        Assert.Equal(0, sonuc.ToplamKayit);
        Assert.Equal(0, sonuc.ToplamSayfa);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(-1, 10)]
    public async Task Repository_gecersiz_sayfa_veya_boyutu_reddeder(int sayfa, int boyut)
    {
        using var db = new TestVeritabani();
        using var kapsam = new ServisKapsami(db);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => kapsam.SiparisRepository.GetGecmisSayfaAsync(new SiparisFiltresi(), sayfa, boyut));
    }

    [Fact]
    public async Task Sonuctaki_siparislerin_Urun_navigasyonu_yuklu_gelir()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 100);
        await SiparisEkleAsync(db, urunId, Gun);

        var siparis = Assert.Single((await SayfaAsync(db, new SiparisFiltresi(), 1, 10)).Ogeler);

        Assert.NotNull(siparis.Urun);
        Assert.Equal("Kalem", siparis.Urun.UrunAdi);
    }

    // ---- 250 kayıtla tutarlılık ----

    [Theory]
    [InlineData(20)]
    [InlineData(7)]
    [InlineData(200)]
    [InlineData(250)]
    [InlineData(1000)]
    public async Task Iki_yuz_elli_kayitta_sayfalarin_birlesimi_tum_kayitlar_ve_tekrarsizdir(int boyut)
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 1000);
        // Her üç sipariş aynı dakikaya düşer; eşit tarihler sayfa sınırlarına da denk gelir.
        var tarihler = Enumerable.Range(0, 250).Select(i => Gun.AddMinutes(i / 3)).ToList();
        var idler = await TopluSiparisEkleAsync(db, urunId, tarihler);
        var beklenen = idler.Zip(tarihler)
            .OrderByDescending(x => x.Second).ThenByDescending(x => x.First)
            .Select(x => x.First).ToList();

        var ilk = await SayfaAsync(db, new SiparisFiltresi(), 1, boyut);
        var birlesik = new List<int>();
        for (var sayfa = 1; sayfa <= ilk.ToplamSayfa; sayfa++)
        {
            var s = await SayfaAsync(db, new SiparisFiltresi(), sayfa, boyut);
            Assert.Equal(250, s.ToplamKayit);
            Assert.Equal(sayfa < ilk.ToplamSayfa ? boyut : 250 - (boyut * (ilk.ToplamSayfa - 1)), s.Ogeler.Count);
            birlesik.AddRange(Idler(s));
        }

        Assert.Equal((250 + boyut - 1) / boyut, ilk.ToplamSayfa);
        Assert.Equal(250, birlesik.Count);
        Assert.Equal(250, birlesik.Distinct().Count());
        Assert.Equal(beklenen, birlesik);
        Assert.Empty((await SayfaAsync(db, new SiparisFiltresi(), ilk.ToplamSayfa + 1, boyut)).Ogeler);
    }

    // ---- Yardımcılar ----

    private static async Task<SayfaliSonuc<Siparis>> SayfaAsync(TestVeritabani db, SiparisFiltresi filtre, int sayfa, int boyut)
    {
        using var kapsam = new ServisKapsami(db);
        return await kapsam.SiparisRepository.GetGecmisSayfaAsync(filtre, sayfa, boyut);
    }

    private static int[] Idler(SayfaliSonuc<Siparis> sonuc) => sonuc.Ogeler.Select(x => x.SiparisId).ToArray();

    private static async Task<int> SiparisEkleAsync(TestVeritabani db, int urunId, DateTime tarih, int adet = 1)
        => (await TopluSiparisEkleAsync(db, urunId, [tarih], adet))[0];

    /// <summary>Siparişleri verilen tarihlerle, verilen sırada ekler; dönen ID'ler tarih listesiyle aynı sıradadır.</summary>
    private static async Task<List<int>> TopluSiparisEkleAsync(TestVeritabani db, int urunId, IReadOnlyList<DateTime> tarihler, int adet = 1)
    {
        await using var context = db.ContextOlustur();
        var urun = await context.Urunler.AsTracking().SingleAsync(x => x.UrunId == urunId);
        var siparisler = new List<Siparis>();
        foreach (var tarih in tarihler)
        {
            var siparis = Siparis.Olustur(urun, adet, tarih);
            context.Siparisler.Add(siparis);
            // Ayrı ayrı kaydedilir ki ID'ler liste sırasıyla artsın.
            await context.SaveChangesAsync();
            siparisler.Add(siparis);
        }

        return siparisler.Select(x => x.SiparisId).ToList();
    }
}
