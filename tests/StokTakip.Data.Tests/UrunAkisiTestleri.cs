using StokTakip.Data.Tests.Altyapi;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Data.Tests;

/// <summary>StokService + UrunRepository + UnitOfWork, SQLite üzerinde.</summary>
public class UrunAkisiTestleri
{
    [Fact]
    public async Task Urun_eklenince_kalici_olur_ve_listede_Id_sirasiyla_gelir()
    {
        using var db = new TestVeritabani();
        var ilk = await Okuma.UrunEkleAsync(db, "  Kalem  ", 10);
        var ikinci = await Okuma.UrunEkleAsync(db, "Defter", 0);

        using var kapsam = new ServisKapsami(db);
        var liste = await kapsam.StokService.UrunleriListeleAsync();

        Assert.Equal([ilk, ikinci], liste.Select(x => x.UrunId));
        Assert.Equal("Kalem", liste[0].UrunAdi);
        Assert.Equal(0, liste[1].Stok);
        Assert.All(liste, x => Assert.NotNull(x.RowVersion));
    }

    [Fact]
    public async Task Urun_getir_olmayan_veya_gecersiz_Id_icin_null_doner()
    {
        using var db = new TestVeritabani();
        using var kapsam = new ServisKapsami(db);

        Assert.Null(await kapsam.StokService.UrunGetirAsync(42));
        Assert.Null(await kapsam.StokService.UrunGetirAsync(0));
    }

    [Fact]
    public async Task Iki_yuz_karakterlik_ad_kabul_edilir_iki_yuz_bir_karakterlik_ad_reddedilir()
    {
        using var db = new TestVeritabani();
        using (var kapsam = new ServisKapsami(db))
        {
            var sinirda = await kapsam.StokService.UrunEkleAsync(new(new string('a', Urun.UrunAdiMaksimumUzunluk), 1));
            Assert.Equal(200, (await Okuma.UrunAsync(db, sinirda.UrunId)).UrunAdi.Length);

            var ex = await Assert.ThrowsAsync<GecersizGirdiException>(
                () => kapsam.StokService.UrunEkleAsync(new(new string('b', 201), 1)));
            Assert.Equal(nameof(Urun.UrunAdi), ex.AlanAdi);
        }

        using (var kapsam = new ServisKapsami(db))
        {
            Assert.Single(await kapsam.StokService.UrunleriListeleAsync());
        }
    }

    [Fact]
    public async Task Guncellemede_uzun_ad_reddedilir_ve_ad_ile_stok_yarim_guncellenmez()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 10);

        using (var kapsam = new ServisKapsami(db))
        {
            await Assert.ThrowsAsync<GecersizGirdiException>(
                () => kapsam.StokService.UrunGuncelleAsync(new(urunId, new string('x', 201), 99)));
        }

        var urun = await Okuma.UrunAsync(db, urunId);
        Assert.Equal("Kalem", urun.UrunAdi);
        Assert.Equal(10, urun.Stok);
    }

    [Fact]
    public async Task Urun_guncellenince_ad_stok_ve_RowVersion_degisir()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 10);
        var onceki = (await Okuma.UrunAsync(db, urunId)).RowVersion!;

        using (var kapsam = new ServisKapsami(db))
        {
            var dto = await kapsam.StokService.UrunGuncelleAsync(new(urunId, "Kurşun Kalem", 4), beklenenRowVersion: onceki);
            Assert.NotEqual(onceki, dto.RowVersion);
        }

        var urun = await Okuma.UrunAsync(db, urunId);
        Assert.Equal("Kurşun Kalem", urun.UrunAdi);
        Assert.Equal(4, urun.Stok);
    }

    [Fact]
    public async Task Negatif_stok_guncellemesi_reddedilir()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 10);

        using (var kapsam = new ServisKapsami(db))
        {
            await Assert.ThrowsAsync<GecersizGirdiException>(() => kapsam.StokService.StokGuncelleAsync(urunId, -1));
        }

        Assert.Equal(10, (await Okuma.UrunAsync(db, urunId)).Stok);
    }

    [Fact]
    public async Task Siparisi_olmayan_urun_silinebilir()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 10);

        using (var kapsam = new ServisKapsami(db))
        {
            await kapsam.StokService.UrunSilAsync(urunId);
        }

        using (var kapsam = new ServisKapsami(db))
        {
            Assert.Null(await kapsam.StokService.UrunGetirAsync(urunId));
        }
    }

    [Fact]
    public async Task Siparisi_olan_urun_silinemez()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 10);

        using (var kapsam = new ServisKapsami(db))
        {
            await kapsam.SiparisService.SiparisOlusturAsync(new(urunId, 1));
            await Assert.ThrowsAsync<GecersizIslemException>(() => kapsam.StokService.UrunSilAsync(urunId));
        }

        Assert.Equal(9, (await Okuma.UrunAsync(db, urunId)).Stok);
    }

    [Fact]
    public async Task Siparisi_silinmis_olsa_da_urun_silinemez()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 10);

        using (var kapsam = new ServisKapsami(db))
        {
            var siparis = await kapsam.SiparisService.SiparisOlusturAsync(new(urunId, 1));
            await kapsam.SiparisService.SiparisSilAsync(siparis.SiparisId);
        }

        using (var kapsam = new ServisKapsami(db))
        {
            await Assert.ThrowsAsync<GecersizIslemException>(() => kapsam.StokService.UrunSilAsync(urunId));
        }

        Assert.Equal(10, (await Okuma.UrunAsync(db, urunId)).Stok);
    }

    [Fact]
    public async Task Olmayan_urunu_silmek_veya_guncellemek_KayitBulunamadi_verir()
    {
        using var db = new TestVeritabani();
        using var kapsam = new ServisKapsami(db);

        await Assert.ThrowsAsync<KayitBulunamadiException>(() => kapsam.StokService.UrunSilAsync(7));
        await Assert.ThrowsAsync<KayitBulunamadiException>(() => kapsam.StokService.StokGuncelleAsync(7, 1));
        await Assert.ThrowsAsync<KayitBulunamadiException>(() => kapsam.StokService.UrunGuncelleAsync(new(7, "Ad", 1)));
    }
}
