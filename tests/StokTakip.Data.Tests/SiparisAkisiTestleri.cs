using StokTakip.Data.Tests.Altyapi;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Data.Tests;

/// <summary>SiparisService + gerçek repository'ler + UnitOfWork, SQLite üzerinde uçtan uca.</summary>
public class SiparisAkisiTestleri
{
    [Fact]
    public async Task Siparis_olusturulunca_stok_duser_ve_kayit_yazilir()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 10);

        using (var kapsam = new ServisKapsami(db))
        {
            var dto = await kapsam.SiparisService.SiparisOlusturAsync(new(urunId, 3));
            Assert.True(dto.SiparisId > 0);
            Assert.Equal("Kalem", dto.UrunAdi);
        }

        Assert.Equal(7, (await Okuma.UrunAsync(db, urunId)).Stok);
        var siparis = Assert.Single(await Okuma.TumSiparislerAsync(db));
        Assert.Equal(urunId, siparis.UrunId);
        Assert.Equal(3, siparis.Adet);
        Assert.False(siparis.IsDeleted);
    }

    [Fact]
    public async Task Siparis_olusturulunca_urunun_RowVersion_degeri_degisir()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 10);
        var onceki = (await Okuma.UrunAsync(db, urunId)).RowVersion;

        using (var kapsam = new ServisKapsami(db))
        {
            await kapsam.SiparisService.SiparisOlusturAsync(new(urunId, 1));
        }

        Assert.NotNull(onceki);
        Assert.NotEqual(onceki, (await Okuma.UrunAsync(db, urunId)).RowVersion);
    }

    [Fact]
    public async Task Stok_yetersizken_siparis_reddedilir_stok_degismez_ve_kayit_olusmaz()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Defter", 3);

        using (var kapsam = new ServisKapsami(db))
        {
            var ex = await Assert.ThrowsAsync<YetersizStokException>(
                () => kapsam.SiparisService.SiparisOlusturAsync(new(urunId, 5)));
            Assert.Equal(3, ex.MevcutStok);
            Assert.Equal(5, ex.IstenenAdet);
        }

        Assert.Equal(3, (await Okuma.UrunAsync(db, urunId)).Stok);
        Assert.Empty(await Okuma.TumSiparislerAsync(db));
    }

    [Fact]
    public async Task Ayni_kapsamda_ikinci_siparis_stogu_asarsa_ilk_siparis_korunur_ikincisi_geri_alinir()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Silgi", 3);

        using (var kapsam = new ServisKapsami(db))
        {
            await kapsam.SiparisService.SiparisOlusturAsync(new(urunId, 2));
            await Assert.ThrowsAsync<YetersizStokException>(
                () => kapsam.SiparisService.SiparisOlusturAsync(new(urunId, 2)));
        }

        Assert.Equal(1, (await Okuma.UrunAsync(db, urunId)).Stok);
        Assert.Single(await Okuma.TumSiparislerAsync(db));
    }

    [Fact]
    public async Task Olmayan_urune_siparis_KayitBulunamadi_hatasi_verir()
    {
        using var db = new TestVeritabani();
        using var kapsam = new ServisKapsami(db);

        await Assert.ThrowsAsync<KayitBulunamadiException>(
            () => kapsam.SiparisService.SiparisOlusturAsync(new(999, 1)));
        Assert.Empty(await Okuma.TumSiparislerAsync(db));
    }

    [Fact]
    public async Task Siparis_silinince_yumusak_silinir_stok_iade_edilir_ve_gecmiste_gorunmez()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 10);
        int silinecekId;
        using (var kapsam = new ServisKapsami(db))
        {
            silinecekId = (await kapsam.SiparisService.SiparisOlusturAsync(new(urunId, 4))).SiparisId;
            await kapsam.SiparisService.SiparisOlusturAsync(new(urunId, 1));
        }

        using (var kapsam = new ServisKapsami(db))
        {
            await kapsam.SiparisService.SiparisSilAsync(silinecekId);
        }

        Assert.Equal(9, (await Okuma.UrunAsync(db, urunId)).Stok);

        // Satır fiziksel olarak duruyor, yalnızca IsDeleted işaretli.
        var tumu = await Okuma.TumSiparislerAsync(db);
        Assert.Equal(2, tumu.Count);
        Assert.True(tumu.Single(x => x.SiparisId == silinecekId).IsDeleted);

        using (var kapsam = new ServisKapsami(db))
        {
            var gecmis = await kapsam.SiparisService.SiparisGecmisiniGetirAsync();
            var kalan = Assert.Single(gecmis);
            Assert.NotEqual(silinecekId, kalan.SiparisId);

            // Query filter: düz DbSet sorgusu da silineni görmez.
            Assert.Single(kapsam.Context.Siparisler.ToList());
        }
    }

    [Fact]
    public async Task Silinmis_siparisi_tekrar_silmek_GecersizIslem_verir_ve_stok_ikinci_kez_iade_edilmez()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 10);
        int siparisId;
        using (var kapsam = new ServisKapsami(db))
        {
            siparisId = (await kapsam.SiparisService.SiparisOlusturAsync(new(urunId, 4))).SiparisId;
            await kapsam.SiparisService.SiparisSilAsync(siparisId);
        }

        using (var kapsam = new ServisKapsami(db))
        {
            await Assert.ThrowsAsync<GecersizIslemException>(
                () => kapsam.SiparisService.SiparisSilAsync(siparisId));
        }

        Assert.Equal(10, (await Okuma.UrunAsync(db, urunId)).Stok);
    }

    [Fact]
    public async Task Olmayan_siparisi_silmek_KayitBulunamadi_hatasi_verir()
    {
        using var db = new TestVeritabani();
        using var kapsam = new ServisKapsami(db);

        await Assert.ThrowsAsync<KayitBulunamadiException>(() => kapsam.SiparisService.SiparisSilAsync(12345));
    }

    [Fact]
    public async Task Siparis_ozeti_bos_veritabaninda_sifir_sifir_doner()
    {
        using var db = new TestVeritabani();
        using var kapsam = new ServisKapsami(db);

        var ozet = await kapsam.SiparisService.SiparisOzetiGetirAsync();

        Assert.Equal(0, ozet.ToplamSiparis);
        Assert.Equal(0, ozet.ToplamAdet);
    }

    [Fact]
    public async Task Siparis_ozeti_silinen_siparisleri_saymaz()
    {
        using var db = new TestVeritabani();
        var kalem = await Okuma.UrunEkleAsync(db, "Kalem", 100);
        var defter = await Okuma.UrunEkleAsync(db, "Defter", 100);

        using var kapsam = new ServisKapsami(db);
        await kapsam.SiparisService.SiparisOlusturAsync(new(kalem, 3));
        var silinecek = await kapsam.SiparisService.SiparisOlusturAsync(new(defter, 50));
        await kapsam.SiparisService.SiparisOlusturAsync(new(defter, 7));
        await kapsam.SiparisService.SiparisSilAsync(silinecek.SiparisId);

        var ozet = await kapsam.SiparisService.SiparisOzetiGetirAsync();

        Assert.Equal(2, ozet.ToplamSiparis);
        Assert.Equal(10, ozet.ToplamAdet);
    }

    [Fact]
    public async Task Siparis_ozeti_toplami_int_MaxValue_u_asarsa_repository_long_doner_servis_ust_sinira_doyurur()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Toptan", int.MaxValue);

        using var kapsam = new ServisKapsami(db);
        await kapsam.SiparisService.SiparisOlusturAsync(new(urunId, int.MaxValue));
        await kapsam.StokService.StokGuncelleAsync(urunId, int.MaxValue);
        await kapsam.SiparisService.SiparisOlusturAsync(new(urunId, int.MaxValue));

        var (sayi, toplam) = await kapsam.SiparisRepository.GetOzetAsync();
        Assert.Equal(2, sayi);
        Assert.Equal(2L * int.MaxValue, toplam);

        var ozet = await kapsam.SiparisService.SiparisOzetiGetirAsync();
        Assert.Equal(2, ozet.ToplamSiparis);
        Assert.Equal(int.MaxValue, ozet.ToplamAdet);
    }

    [Fact]
    public async Task Tum_siparisler_silinince_ozet_yeniden_sifir_sifir_olur()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 5);

        using var kapsam = new ServisKapsami(db);
        var siparis = await kapsam.SiparisService.SiparisOlusturAsync(new(urunId, 2));
        await kapsam.SiparisService.SiparisSilAsync(siparis.SiparisId);

        var ozet = await kapsam.SiparisService.SiparisOzetiGetirAsync();

        Assert.Equal(0, ozet.ToplamSiparis);
        Assert.Equal(0, ozet.ToplamAdet);
    }

    [Fact]
    public async Task Siparis_gecmisi_tarihe_gore_yeniden_eskiye_ayni_tarihte_Id_ye_gore_azalan_siralanir()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 100);
        var zaman = new SabitZamanSaglayici(new DateTime(2026, 5, 10, 12, 0, 0));

        int orta, eski, yeni, yeniAyniTarih;
        using (var kapsam = new ServisKapsami(db, zaman))
        {
            orta = (await kapsam.SiparisService.SiparisOlusturAsync(new(urunId, 1))).SiparisId;
            zaman.Simdi = new DateTime(2026, 1, 1, 8, 30, 0);
            eski = (await kapsam.SiparisService.SiparisOlusturAsync(new(urunId, 1))).SiparisId;
            zaman.Simdi = new DateTime(2026, 9, 30, 23, 59, 59);
            yeni = (await kapsam.SiparisService.SiparisOlusturAsync(new(urunId, 1))).SiparisId;
            yeniAyniTarih = (await kapsam.SiparisService.SiparisOlusturAsync(new(urunId, 1))).SiparisId;
        }

        using (var kapsam = new ServisKapsami(db))
        {
            var gecmis = await kapsam.SiparisService.SiparisGecmisiniGetirAsync();

            Assert.Equal([yeniAyniTarih, yeni, orta, eski], gecmis.Select(x => x.SiparisId));
            Assert.Equal(new DateTime(2026, 1, 1, 8, 30, 0), gecmis[^1].Tarih);
            Assert.All(gecmis, x => Assert.Equal("Kalem", x.UrunAdi));
        }
    }
}
