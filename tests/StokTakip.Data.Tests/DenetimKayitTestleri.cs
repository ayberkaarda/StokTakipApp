using Microsoft.EntityFrameworkCore;
using StokTakip.Application.Dtos;
using StokTakip.Data.Tests.Altyapi;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Data.Tests;

/// <summary>Servis işlemlerinin denetim kayıtları: içerik ve işlemle atomiklik.</summary>
public class DenetimKayitTestleri
{
    private readonly DenetimTestVeritabani _db = new();

    private async Task<(DenetimKapsami Kapsam, Kullanici Kullanici)> OturumluKapsamAsync(KullaniciRolu rol = KullaniciRolu.Operator)
    {
        var kullanici = await _db.KullaniciEkleAsync(rol == KullaniciRolu.Admin ? "yonetici" : "ali", rol);
        var kapsam = new DenetimKapsami(_db);
        kapsam.OturumAc(kullanici);
        return (kapsam, kullanici);
    }

    private async Task<UrunDto> UrunEkleAsync(string ad, int stok)
    {
        using var kapsam = new DenetimKapsami(_db);
        return await kapsam.StokService.UrunEkleAsync(new UrunEkleIstegi(ad, stok));
    }

    [Fact]
    public async Task StokGuncelle_eski_yeni_fark_ve_islemi_yapani_tek_kayitta_yazar()
    {
        var urun = await UrunEkleAsync("Klavye", 118);
        var (kapsam, ali) = await OturumluKapsamAsync();
        using (kapsam)
        {
            await kapsam.StokService.StokGuncelleAsync(urun.UrunId, 98);
        }

        var kayit = Assert.Single(await _db.KayitlarAsync(), x => x.Islem == DenetimIslemi.StokGuncellendi);
        Assert.Equal(ali.KullaniciId, kayit.KullaniciId);
        Assert.Equal("ali", kayit.KullaniciAdi);
        Assert.Equal("Ürün", kayit.VarlikTuru);
        Assert.Equal(urun.UrunId, kayit.VarlikId);
        Assert.Equal("Klavye", kayit.VarlikAdi);
        Assert.Equal("118", kayit.EskiDeger);
        Assert.Equal("98", kayit.YeniDeger);
        Assert.Equal(-20, kayit.Fark);
        Assert.Equal("Klavye stoğu 118 → 98 (−20)", kayit.Ozet);
    }

    [Fact]
    public async Task Oturumsuz_islem_sistem_adina_kaydedilir()
    {
        var urun = await UrunEkleAsync("Kalem", 5);

        var kayit = Assert.Single(await _db.KayitlarAsync());
        Assert.Equal(DenetimIslemi.UrunEklendi, kayit.Islem);
        Assert.Null(kayit.KullaniciId);
        Assert.Equal("sistem", kayit.KullaniciAdi);
        Assert.Equal(urun.UrunId, kayit.VarlikId);
        Assert.Equal("Kalem", kayit.VarlikAdi);
    }

    [Fact]
    public async Task Islem_geri_alininca_kayit_da_yazilmaz_eszamanlilik_cakismasi()
    {
        var urun = await UrunEkleAsync("Klavye", 10);
        var (kapsam, _) = await OturumluKapsamAsync();
        using (kapsam)
        {
            await Assert.ThrowsAsync<EszamanlilikException>(
                () => kapsam.StokService.StokGuncelleAsync(urun.UrunId, 3, beklenenRowVersion: [9, 9, 9, 9, 9, 9, 9, 9]));
        }

        Assert.Equal(10, (await _db.UrunAsync(urun.UrunId)).Stok);
        Assert.DoesNotContain(await _db.KayitlarAsync(), x => x.Islem == DenetimIslemi.StokGuncellendi);
    }

    [Fact]
    public async Task Yetersiz_stokla_reddedilen_sipariste_kayit_yok()
    {
        var urun = await UrunEkleAsync("Klavye", 2);
        var (kapsam, _) = await OturumluKapsamAsync();
        using (kapsam)
        {
            await Assert.ThrowsAsync<YetersizStokException>(
                () => kapsam.SiparisService.SiparisOlusturAsync(new SiparisOlusturIstegi(urun.UrunId, 3)));
        }

        Assert.DoesNotContain(await _db.KayitlarAsync(), x => x.Islem == DenetimIslemi.SiparisOlusturuldu);
    }

    public static readonly TheoryData<string> Islemler = new() { "UrunEkle", "StokGuncelle", "UrunGuncelle", "UrunSil", "SiparisOlustur", "SiparisSil", "KategoriEkle", "KategoriSil" };

    [Theory]
    [MemberData(nameof(Islemler))]
    public async Task Kayit_yazilamazsa_islem_de_geri_alinir(string islem)
    {
        var urun = await UrunEkleAsync("Klavye", 10);
        var silinecek = await UrunEkleAsync("Silinecek", 1);
        int siparisId;
        int kategoriId;
        using (var hazirlik = new DenetimKapsami(_db))
        {
            siparisId = (await hazirlik.SiparisService.SiparisOlusturAsync(new SiparisOlusturIstegi(urun.UrunId, 4))).SiparisId;
            kategoriId = (await hazirlik.KategoriService.KategoriEkleAsync("Kırtasiye")).KategoriId;
        }

        var oncekiKayitSayisi = (await _db.KayitlarAsync()).Count;
        _db.YazimHatasi.Etkin = true;
        using (var kapsam = new DenetimKapsami(_db))
        {
            Func<Task> calistir = islem switch
            {
                "UrunEkle" => () => kapsam.StokService.UrunEkleAsync(new UrunEkleIstegi("Yeni", 1)),
                "StokGuncelle" => () => kapsam.StokService.StokGuncelleAsync(urun.UrunId, 1),
                "UrunGuncelle" => () => kapsam.StokService.UrunGuncelleAsync(new UrunGuncelleIstegi(urun.UrunId, "Yeni ad", 1)),
                "UrunSil" => () => kapsam.StokService.UrunSilAsync(silinecek.UrunId),
                "SiparisOlustur" => () => kapsam.SiparisService.SiparisOlusturAsync(new SiparisOlusturIstegi(urun.UrunId, 1)),
                "SiparisSil" => () => kapsam.SiparisService.SiparisSilAsync(siparisId),
                "KategoriEkle" => () => kapsam.KategoriService.KategoriEkleAsync("Gıda"),
                "KategoriSil" => () => kapsam.KategoriService.KategoriSilAsync(kategoriId),
                _ => throw new ArgumentOutOfRangeException(nameof(islem)),
            };
            await Assert.ThrowsAsync<InvalidOperationException>(calistir);
        }

        _db.YazimHatasi.Etkin = false;
        await using var context = _db.ContextOlustur();
        var urunler = await context.Urunler.AsNoTracking().OrderBy(x => x.UrunId).ToListAsync();
        Assert.Equal(new[] { "Klavye", "Silinecek" }, urunler.Select(x => x.UrunAdi));
        Assert.Equal(6, urunler[0].Stok);
        Assert.Single(await context.Siparisler.AsNoTracking().ToListAsync());
        Assert.Equal(["Kırtasiye"], await context.Kategoriler.Select(x => x.Ad).ToListAsync());
        Assert.Equal(oncekiKayitSayisi, (await _db.KayitlarAsync()).Count);
    }

    [Fact]
    public async Task Siparis_olustur_ve_sil_urun_adet_kalan_stok_ve_iade_farkini_yazar()
    {
        var urun = await UrunEkleAsync("Klavye", 50);
        var (kapsam, _) = await OturumluKapsamAsync(KullaniciRolu.Admin);
        using (kapsam)
        {
            var siparis = await kapsam.SiparisService.SiparisOlusturAsync(new SiparisOlusturIstegi(urun.UrunId, 10));
            await kapsam.StokService.StokGuncelleAsync(urun.UrunId, 40);
            await kapsam.SiparisService.SiparisSilAsync(siparis.SiparisId);

            var kayitlar = await _db.KayitlarAsync();
            var olustur = Assert.Single(kayitlar, x => x.Islem == DenetimIslemi.SiparisOlusturuldu);
            Assert.Equal("Sipariş", olustur.VarlikTuru);
            Assert.Equal(siparis.SiparisId, olustur.VarlikId);
            Assert.Equal("Klavye", olustur.VarlikAdi);
            Assert.Equal(-10, olustur.Fark);
            Assert.Equal("50", olustur.EskiDeger);
            Assert.Equal("40", olustur.YeniDeger);
            Assert.Contains("kalan stok 40", olustur.Ozet);

            var sil = Assert.Single(kayitlar, x => x.Islem == DenetimIslemi.SiparisSilindi);
            Assert.Equal(siparis.SiparisId, sil.VarlikId);
            Assert.Equal("Klavye", sil.VarlikAdi);
            Assert.Equal(10, sil.Fark);
            Assert.Equal("40", sil.EskiDeger);
            Assert.Equal("50", sil.YeniDeger);
            Assert.Contains($"#{siparis.SiparisId}", sil.Ozet);
            Assert.Contains("40 → 50 (+10)", sil.Ozet);
            Assert.Equal("yonetici", sil.KullaniciAdi);
        }
    }

    [Fact]
    public async Task UrunGuncelle_ad_stok_ve_esik_degisimini_ayri_kayitlara_yazar()
    {
        var urun = await UrunEkleAsync("Klavye", 20);
        var (kapsam, _) = await OturumluKapsamAsync(KullaniciRolu.Admin);
        using (kapsam)
        {
            await kapsam.StokService.UrunGuncelleAsync(new UrunGuncelleIstegi(urun.UrunId, "Mekanik Klavye", 25, DusukStokEsigi: 3));
            await kapsam.StokService.UrunGuncelleAsync(new UrunGuncelleIstegi(urun.UrunId, "Mekanik Klavye", 25, DusukStokEsigi: 3));
        }

        var kayitlar = (await _db.KayitlarAsync()).Skip(1).ToList();
        Assert.Equal(
            new[] { DenetimIslemi.UrunGuncellendi, DenetimIslemi.StokGuncellendi, DenetimIslemi.EsikDegisti },
            kayitlar.Select(x => x.Islem));
        Assert.All(kayitlar, x => Assert.Equal("Mekanik Klavye", x.VarlikAdi));
        Assert.Equal("Ad=Klavye", kayitlar[0].EskiDeger);
        Assert.Equal("Ad=Mekanik Klavye", kayitlar[0].YeniDeger);
        Assert.Equal(5, kayitlar[1].Fark);
        Assert.Equal(("10", "3"), (kayitlar[2].EskiDeger, kayitlar[2].YeniDeger));
        Assert.Null(kayitlar[2].Fark);
    }

    [Fact]
    public async Task Urun_silme_ve_kategori_islemleri_kaydedilir()
    {
        var urun = await UrunEkleAsync("Silgi", 7);
        var (kapsam, _) = await OturumluKapsamAsync(KullaniciRolu.Admin);
        using (kapsam)
        {
            await kapsam.StokService.UrunSilAsync(urun.UrunId);
            var kategori = await kapsam.KategoriService.KategoriEkleAsync("Kırtasiye");
            await kapsam.KategoriService.KategoriYenidenAdlandirAsync(kategori.KategoriId, "Ofis");
            await kapsam.KategoriService.KategoriSilAsync(kategori.KategoriId);

            var kayitlar = (await _db.KayitlarAsync()).Skip(1).ToList();
            Assert.Equal(
                new[] { DenetimIslemi.UrunSilindi, DenetimIslemi.KategoriEklendi, DenetimIslemi.KategoriAdiDegisti, DenetimIslemi.KategoriSilindi },
                kayitlar.Select(x => x.Islem));
            Assert.Equal(("Silgi", urun.UrunId), (kayitlar[0].VarlikAdi, kayitlar[0].VarlikId));
            Assert.Contains("Stok=7", kayitlar[0].EskiDeger);
            Assert.All(kayitlar.Skip(1), x => Assert.Equal(("Kategori", kategori.KategoriId), (x.VarlikTuru, x.VarlikId)));
            Assert.Equal(("Kırtasiye", "Ofis"), (kayitlar[2].EskiDeger, kayitlar[2].YeniDeger));
            Assert.Equal("Ofis", kayitlar[3].VarlikAdi);
        }
    }

    [Fact]
    public async Task Siparis_csv_aktarimi_satir_sayisiyla_kaydedilir()
    {
        var urun = await UrunEkleAsync("Klavye", 50);
        var (kapsam, _) = await OturumluKapsamAsync(KullaniciRolu.Admin);
        using (kapsam)
        {
            await kapsam.SiparisService.SiparisOlusturAsync(new SiparisOlusturIstegi(urun.UrunId, 1));
            await kapsam.SiparisService.SiparisOlusturAsync(new SiparisOlusturIstegi(urun.UrunId, 2));
            await kapsam.DisaAktarimService.SiparisleriCsvAktarAsync(new SiparisGecmisiSorgusu(), new MemoryStream());
        }

        var kayit = Assert.Single(await _db.KayitlarAsync(), x => x.Islem == DenetimIslemi.CsvAktarildi);
        Assert.Equal("Sipariş", kayit.VarlikTuru);
        Assert.Equal("Satır=2", kayit.YeniDeger);
        Assert.Equal("yonetici", kayit.KullaniciAdi);
    }
}
