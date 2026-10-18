using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.Tests;

public class StokDurumuVeEsikTests
{
    [Theory]
    // Eşik 10: 0–5 Kritik, 6–10 Düşük, 11+ Yeterli.
    [InlineData(0, 10, StokDurumu.Kritik)]
    [InlineData(5, 10, StokDurumu.Kritik)]
    [InlineData(6, 10, StokDurumu.Dusuk)]
    [InlineData(10, 10, StokDurumu.Dusuk)]
    [InlineData(11, 10, StokDurumu.Yeterli)]
    // Tek eşikte yarım kesirlidir: 5 için 2 Kritik, 3 Düşük.
    [InlineData(2, 5, StokDurumu.Kritik)]
    [InlineData(3, 5, StokDurumu.Dusuk)]
    [InlineData(5, 5, StokDurumu.Dusuk)]
    [InlineData(6, 5, StokDurumu.Yeterli)]
    // Eşik 1: yalnızca 0 Kritik, 1 Düşük.
    [InlineData(0, 1, StokDurumu.Kritik)]
    [InlineData(1, 1, StokDurumu.Dusuk)]
    [InlineData(2, 1, StokDurumu.Yeterli)]
    // Eşik 0: stok 0 her zaman Kritik, pozitif stok Yeterli.
    [InlineData(0, 0, StokDurumu.Kritik)]
    [InlineData(1, 0, StokDurumu.Yeterli)]
    // Taşma olmaz.
    [InlineData(int.MaxValue, int.MaxValue, StokDurumu.Dusuk)]
    [InlineData(int.MaxValue / 2, int.MaxValue, StokDurumu.Kritik)]
    [InlineData(int.MaxValue / 2 + 1, int.MaxValue, StokDurumu.Dusuk)]
    public void StokDurumu_SinirDegerleri(int stok, int esik, StokDurumu beklenen)
    {
        Assert.Equal(beklenen, Urun.StokDurumunuHesapla(stok, esik));
        Assert.Equal(beklenen, new Urun("Kalem", stok, esik).StokDurumu);
    }

    [Fact]
    public void StokDurumu_StokVeEsikDegisinceYenidenHesaplanir()
    {
        var urun = new Urun("Kalem", 11);
        Assert.Equal(StokDurumu.Yeterli, urun.StokDurumu);
        urun.StoktanDus(1);
        Assert.Equal(StokDurumu.Dusuk, urun.StokDurumu);
        urun.DusukStokEsiginiAyarla(20);
        Assert.Equal(StokDurumu.Kritik, urun.StokDurumu);
        urun.DusukStokEsiginiAyarla(0);
        Assert.Equal(StokDurumu.Yeterli, urun.StokDurumu);
    }

    [Fact]
    public void Urun_EsikVerilmezse_VarsayilanOn()
    {
        Assert.Equal(10, Urun.VarsayilanDusukStokEsigi);
        Assert.Equal(10, new Urun("Kalem", 0).DusukStokEsigi);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Urun_NegatifEsik_TurkceMesajlaReddedilir(int esik)
    {
        var hata = Assert.Throws<GecersizGirdiException>(() => new Urun("Kalem", 5, esik));
        Assert.Equal(nameof(Urun.DusukStokEsigi), hata.AlanAdi);
        Assert.Equal("Düşük stok eşiği negatif olamaz.", hata.Message);

        var urun = new Urun("Kalem", 5, 7);
        Assert.Throws<GecersizGirdiException>(() => urun.DusukStokEsiginiAyarla(esik));
        Assert.Equal(7, urun.DusukStokEsigi);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public void Urun_SifirVeUstuEsik_KabulEdilir(int esik)
    {
        var urun = new Urun("Kalem", 5, esik);
        Assert.Equal(esik, urun.DusukStokEsigi);
        urun.DusukStokEsiginiAyarla(3);
        Assert.Equal(3, urun.DusukStokEsigi);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Kategori_BosAd_Reddedilir(string? ad)
    {
        var hata = Assert.Throws<GecersizGirdiException>(() => new Kategori(ad!));
        Assert.Equal(nameof(Kategori.Ad), hata.AlanAdi);
        Assert.Equal("Kategori adı boş olamaz.", hata.Message);
    }

    [Fact]
    public void Kategori_AdSiniri_YuzKabulYuzBirRed_KirpilarakSaklanir()
    {
        Assert.Equal(100, new Kategori(new string('a', 100)).Ad.Length);
        Assert.Equal(100, new Kategori("  " + new string('a', 100) + "  ").Ad.Length);
        Assert.Throws<GecersizGirdiException>(() => new Kategori(new string('a', 101)));

        var kategori = new Kategori(" Kırtasiye ");
        Assert.Equal("Kırtasiye", kategori.Ad);
        Assert.Throws<GecersizGirdiException>(() => kategori.YenidenAdlandir(new string('b', 101)));
        Assert.Equal("Kırtasiye", kategori.Ad);
        kategori.YenidenAdlandir(" Ofis ");
        Assert.Equal("Ofis", kategori.Ad);
    }

    [Fact]
    public void Urun_KategoriyeAtanirVeKategorisizBirakilabilir()
    {
        var urun = new Urun("Kalem", 5);
        Assert.Null(urun.KategoriId);
        var kategori = new Kategori("Kırtasiye");

        urun.KategoriyeAta(kategori);
        Assert.Same(kategori, urun.Kategori);
        Assert.Equal(kategori.KategoriId, urun.KategoriId);

        urun.KategoriyeAta(null);
        Assert.Null(urun.Kategori);
        Assert.Null(urun.KategoriId);
    }
}
