using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data.Tests.Altyapi;
using StokTakip.Domain;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Data.Tests;

/// <summary>KategoriService, StokService (kategori ve eşik alanları) ve şema kısıtları, SQLite üzerinde.</summary>
public class KategoriVeEsikTestleri
{
    private static async Task<int> KategoriEkleAsync(TestVeritabani db, string ad)
    {
        using var kapsam = new KategoriliServisKapsami(db);
        return (await kapsam.KategoriService.KategoriEkleAsync(ad)).KategoriId;
    }

    private static async Task<int> UrunEkleAsync(TestVeritabani db, string ad, int stok, int? kategoriId = null, int esik = 10)
    {
        using var kapsam = new KategoriliServisKapsami(db);
        return (await kapsam.StokService.UrunEkleAsync(new(ad, stok, kategoriId, esik))).UrunId;
    }

    private static async Task<int?> KategoriIdOkuAsync(TestVeritabani db, int urunId)
        => (await Okuma.UrunAsync(db, urunId)).KategoriId;

    [Fact]
    public async Task Kategoriler_kirpilarak_kaydedilir_ve_ada_gore_sirali_listelenir()
    {
        using var db = new TestVeritabani();
        var ofis = await KategoriEkleAsync(db, "  Ofis ");
        var gida = await KategoriEkleAsync(db, "Gıda");

        using var kapsam = new KategoriliServisKapsami(db);
        var liste = await kapsam.KategoriService.KategorileriListeleAsync();

        Assert.Equal([gida, ofis], liste.Select(x => x.KategoriId));
        Assert.Equal(["Gıda", "Ofis"], liste.Select(x => x.Ad));
    }

    [Theory]
    [InlineData("Ofis")]
    [InlineData("OFIS")]
    [InlineData(" ofis ")]
    public async Task Ayni_ad_buyuk_kucuk_harf_ve_bosluktan_bagimsiz_ikinci_kez_eklenemez(string ikinciAd)
    {
        using var db = new TestVeritabani();
        await KategoriEkleAsync(db, "Ofis");

        using (var kapsam = new KategoriliServisKapsami(db))
        {
            var hata = await Assert.ThrowsAsync<GecersizGirdiException>(() => kapsam.KategoriService.KategoriEkleAsync(ikinciAd));
            Assert.Equal("Ad", hata.AlanAdi);
        }

        using (var kapsam = new KategoriliServisKapsami(db))
        {
            Assert.Single(await kapsam.KategoriService.KategorileriListeleAsync());
        }
    }

    [Fact]
    public async Task Benzersiz_indeks_servisi_atlayan_ayni_adi_reddeder()
    {
        using var db = new TestVeritabani();
        await KategoriEkleAsync(db, "Ofis");

        await using var context = db.ContextOlustur();
        var ex = await Assert.ThrowsAsync<SqliteException>(
            () => context.Database.ExecuteSqlAsync($"INSERT INTO Kategoriler (Ad) VALUES ('Ofis')"));

        Assert.Contains("UNIQUE", ex.Message);
    }

    [Fact]
    public async Task Yeniden_adlandirma_kendi_adinin_yazimini_degistirebilir_baska_kategorinin_adini_alamaz()
    {
        using var db = new TestVeritabani();
        var ofis = await KategoriEkleAsync(db, "ofis");
        await KategoriEkleAsync(db, "Gıda");

        using (var kapsam = new KategoriliServisKapsami(db))
        {
            Assert.Equal("Ofis", (await kapsam.KategoriService.KategoriYenidenAdlandirAsync(ofis, "Ofis")).Ad);
            await Assert.ThrowsAsync<GecersizGirdiException>(() => kapsam.KategoriService.KategoriYenidenAdlandirAsync(ofis, "Gıda"));
            await Assert.ThrowsAsync<KayitBulunamadiException>(() => kapsam.KategoriService.KategoriYenidenAdlandirAsync(999, "X"));
        }

        using (var kapsam = new KategoriliServisKapsami(db))
        {
            var liste = await kapsam.KategoriService.KategorileriListeleAsync();
            Assert.Equal(["Gıda", "Ofis"], liste.Select(x => x.Ad));
        }
    }

    [Fact]
    public async Task Kategori_silinince_urunleri_silinmez_kategorisiz_kalir()
    {
        using var db = new TestVeritabani();
        var kirtasiye = await KategoriEkleAsync(db, "Kırtasiye");
        var gida = await KategoriEkleAsync(db, "Gıda");
        var kalem = await UrunEkleAsync(db, "Kalem", 10, kirtasiye);
        var defter = await UrunEkleAsync(db, "Defter", 5, kirtasiye);
        var elma = await UrunEkleAsync(db, "Elma", 3, gida);

        // Defter'e sipariş: Siparisler FK'sinin Restrict olması kategori silmeyi etkilememeli.
        using (var kapsam = new ServisKapsami(db))
        {
            await kapsam.SiparisService.SiparisOlusturAsync(new(defter, 1));
        }

        // Ürünler bu kapsamda belleğe yüklenmez; boşaltmayı veritabanı (ON DELETE SET NULL) yapar.
        using (var kapsam = new KategoriliServisKapsami(db))
        {
            await kapsam.KategoriService.KategoriSilAsync(kirtasiye);
        }

        Assert.Null(await KategoriIdOkuAsync(db, kalem));
        Assert.Null(await KategoriIdOkuAsync(db, defter));
        Assert.Equal(gida, await KategoriIdOkuAsync(db, elma));
        Assert.Equal(4, (await Okuma.UrunAsync(db, defter)).Stok);

        using (var kapsam = new KategoriliServisKapsami(db))
        {
            var urunler = await kapsam.StokService.UrunleriListeleAsync();
            Assert.Equal(3, urunler.Count);
            Assert.Equal([null, null, "Gıda"], urunler.Select(x => x.KategoriAdi));
            Assert.Equal(["Gıda"], (await kapsam.KategoriService.KategorileriListeleAsync()).Select(x => x.Ad));
        }
    }

    [Fact]
    public async Task Urunleri_bellekte_izlenirken_silinen_kategori_de_urunleri_kategorisiz_birakir()
    {
        using var db = new TestVeritabani();
        var kirtasiye = await KategoriEkleAsync(db, "Kırtasiye");
        var kalem = await UrunEkleAsync(db, "Kalem", 10, kirtasiye);

        using (var kapsam = new KategoriliServisKapsami(db))
        {
            var oncekiListe = await kapsam.StokService.UrunleriListeleAsync();
            Assert.Equal("Kırtasiye", oncekiListe.Single().KategoriAdi);

            await kapsam.KategoriService.KategoriSilAsync(kirtasiye);

            var sonrakiListe = await kapsam.StokService.UrunleriListeleAsync();
            Assert.Null(sonrakiListe.Single().KategoriId);
        }

        Assert.Null(await KategoriIdOkuAsync(db, kalem));
    }

    [Fact]
    public async Task Olmayan_kategori_silinemez()
    {
        using var db = new TestVeritabani();
        using var kapsam = new KategoriliServisKapsami(db);
        await Assert.ThrowsAsync<KayitBulunamadiException>(() => kapsam.KategoriService.KategoriSilAsync(5));
    }

    [Fact]
    public async Task Urun_kategoriyle_eklenir_baska_kategoriye_tasinir_ve_kategorisiz_birakilir()
    {
        using var db = new TestVeritabani();
        var kirtasiye = await KategoriEkleAsync(db, "Kırtasiye");
        var ofis = await KategoriEkleAsync(db, "Ofis");
        var kalem = await UrunEkleAsync(db, "Kalem", 10, kirtasiye);

        using (var kapsam = new KategoriliServisKapsami(db))
        {
            var dto = await kapsam.StokService.UrunGetirAsync(kalem);
            Assert.Equal(kirtasiye, dto!.KategoriId);
            Assert.Equal("Kırtasiye", dto.KategoriAdi);
        }

        using (var kapsam = new KategoriliServisKapsami(db))
        {
            var dto = await kapsam.StokService.UrunGuncelleAsync(new(kalem, "Kalem", 10, KategoriId: ofis, KategoriGuncellensin: true));
            Assert.Equal("Ofis", dto.KategoriAdi);
        }

        Assert.Equal(ofis, await KategoriIdOkuAsync(db, kalem));

        // Kategori alanı gönderilmeyen eski tarz güncelleme kategoriyi korur.
        using (var kapsam = new KategoriliServisKapsami(db))
        {
            await kapsam.StokService.UrunGuncelleAsync(new(kalem, "Kurşun Kalem", 9));
        }

        Assert.Equal(ofis, await KategoriIdOkuAsync(db, kalem));

        using (var kapsam = new KategoriliServisKapsami(db))
        {
            var dto = await kapsam.StokService.UrunGuncelleAsync(new(kalem, "Kurşun Kalem", 9, KategoriGuncellensin: true));
            Assert.Null(dto.KategoriAdi);
        }

        Assert.Null(await KategoriIdOkuAsync(db, kalem));
    }

    [Fact]
    public async Task Olmayan_kategoriyle_urun_eklenemez_ve_yabanci_anahtar_ham_SQLi_de_engeller()
    {
        using var db = new TestVeritabani();

        using (var kapsam = new KategoriliServisKapsami(db))
        {
            var hata = await Assert.ThrowsAsync<GecersizGirdiException>(
                () => kapsam.StokService.UrunEkleAsync(new("Kalem", 1, KategoriId: 42)));
            Assert.Equal("KategoriId", hata.AlanAdi);
        }

        using (var kapsam = new KategoriliServisKapsami(db))
        {
            Assert.Empty(await kapsam.StokService.UrunleriListeleAsync());
        }

        await using var context = db.ContextOlustur();
        var ex = await Assert.ThrowsAsync<SqliteException>(() => context.Database.ExecuteSqlAsync(
            $"INSERT INTO Urunler (UrunAdi, Stok, DusukStokEsigi, KategoriID) VALUES ('Kalem', 1, 10, 42)"));
        Assert.Contains("FOREIGN KEY", ex.Message);
    }

    [Fact]
    public async Task Sifir_esik_varsayilana_donmeden_kaydedilir_verilmeyen_esik_on_olur()
    {
        using var db = new TestVeritabani();
        var sifir = await UrunEkleAsync(db, "Kalem", 0, esik: 0);
        var varsayilan = await Okuma.UrunEkleAsync(db, "Defter", 6);

        Assert.Equal(0, (await Okuma.UrunAsync(db, sifir)).DusukStokEsigi);
        var defter = await Okuma.UrunAsync(db, varsayilan);
        Assert.Equal(10, defter.DusukStokEsigi);
        Assert.Equal(StokDurumu.Dusuk, defter.StokDurumu);
    }

    [Fact]
    public async Task Esik_sutunu_verilmeden_eklenen_satir_veritabani_varsayilani_onu_alir()
    {
        using var db = new TestVeritabani();
        await using (var context = db.ContextOlustur())
        {
            await context.Database.ExecuteSqlAsync($"INSERT INTO Urunler (UrunAdi, Stok) VALUES ('Eski', 3)");
        }

        await using (var context = db.ContextOlustur())
        {
            var urun = await context.Urunler.AsNoTracking().SingleAsync();
            Assert.Equal(10, urun.DusukStokEsigi);
            Assert.Null(urun.KategoriId);
        }
    }

    [Fact]
    public async Task Negatif_esik_CHECK_kisitina_takilir_servis_de_reddeder()
    {
        using var db = new TestVeritabani();
        var urunId = await UrunEkleAsync(db, "Kalem", 10, esik: 4);

        using (var kapsam = new KategoriliServisKapsami(db))
        {
            await Assert.ThrowsAsync<GecersizGirdiException>(
                () => kapsam.StokService.UrunGuncelleAsync(new(urunId, "Kalem", 10, DusukStokEsigi: -1)));
        }

        await using var context = db.ContextOlustur();
        var ex = await Assert.ThrowsAsync<SqliteException>(
            () => context.Database.ExecuteSqlAsync($"UPDATE Urunler SET DusukStokEsigi = -1 WHERE UrunID = {urunId}"));
        Assert.Contains("CK_Urunler_DusukStokEsigi", ex.Message);
        Assert.Equal(4, (await Okuma.UrunAsync(db, urunId)).DusukStokEsigi);
    }

    [Fact]
    public async Task Esik_guncellenir_verilmezse_korunur()
    {
        using var db = new TestVeritabani();
        var urunId = await UrunEkleAsync(db, "Kalem", 10, esik: 4);

        using (var kapsam = new KategoriliServisKapsami(db))
        {
            await kapsam.StokService.UrunGuncelleAsync(new(urunId, "Kalem", 10, DusukStokEsigi: 25));
        }

        Assert.Equal(25, (await Okuma.UrunAsync(db, urunId)).DusukStokEsigi);

        using (var kapsam = new ServisKapsami(db))
        {
            await kapsam.StokService.UrunGuncelleAsync(new(urunId, "Kalem", 12));
            await kapsam.StokService.StokGuncelleAsync(urunId, 11);
        }

        var urun = await Okuma.UrunAsync(db, urunId);
        Assert.Equal(25, urun.DusukStokEsigi);
        Assert.Equal(StokDurumu.Kritik, urun.StokDurumu);
    }
}
