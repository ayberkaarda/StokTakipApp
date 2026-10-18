using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.Tests;

public class DomainKurallariTests
{
    [Fact]
    public void StoktanDus_YetersizStok_StokDegismez()
    {
        var urun = new Urun("Kalem", 2);
        var hata = Assert.Throws<YetersizStokException>(() => urun.StoktanDus(3));
        Assert.Equal(2, urun.Stok);
        Assert.Equal(urun.UrunId, hata.UrunId);
        Assert.Equal("Kalem", hata.UrunAdi);
        Assert.Equal(2, hata.MevcutStok);
        Assert.Equal(3, hata.IstenenAdet);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void StokIslemleri_GecersizAdet_StokDegismez(int adet)
    {
        var urun = new Urun("Kalem", 5);
        Assert.Throws<GecersizGirdiException>(() => urun.StoktanDus(adet));
        Assert.Throws<GecersizGirdiException>(() => urun.StokEkle(adet));
        Assert.Equal(5, urun.Stok);
    }

    [Fact]
    public void StokIslemleri_GecerliAdet_SifiraVeIntSinirinaUlasabilir()
    {
        var urun = new Urun("Kalem", int.MaxValue);
        urun.StoktanDus(int.MaxValue);
        Assert.Equal(0, urun.Stok);
        urun.StokEkle(int.MaxValue);
        Assert.Equal(int.MaxValue, urun.Stok);
    }

    [Fact]
    public void StokEkle_IntTasmasi_StokDegismez()
    {
        var urun = new Urun("Kalem", int.MaxValue);
        Assert.Throws<OverflowException>(() => urun.StokEkle(1));
        Assert.Equal(int.MaxValue, urun.Stok);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Urun_GecersizAd_Reddedilir(string? ad)
    {
        var hata = Assert.Throws<GecersizGirdiException>(() => new Urun(ad!, 0));
        Assert.Equal("UrunAdi", hata.AlanAdi);
        var urun = new Urun("Kalem", 0);
        Assert.Throws<GecersizGirdiException>(() => urun.AdiDegistir(ad!));
        Assert.Equal("Kalem", urun.UrunAdi);
    }

    [Fact]
    public void Urun_AdUzunluguSiniriVeBoslukKirpma_Dogrulanir()
    {
        var ad = new string('a', 200);
        var urun = new Urun(ad, 0);
        Assert.Equal(ad, urun.UrunAdi);
        Assert.Throws<GecersizGirdiException>(() => new Urun(ad + "a", 0));
        Assert.Throws<GecersizGirdiException>(() => urun.AdiDegistir(ad + "a"));
        urun.AdiDegistir(" Defter ");
        Assert.Equal("Defter", urun.UrunAdi);
    }

    [Fact]
    public void Urun_NegatifStok_ReddedilirVeMevcutStokKorunur()
    {
        Assert.Throws<GecersizGirdiException>(() => new Urun("Kalem", -1));
        var urun = new Urun("Kalem", 4);
        Assert.Throws<GecersizGirdiException>(() => urun.StokAyarla(-1));
        Assert.Equal(4, urun.Stok);
        urun.StokAyarla(0);
        Assert.Equal(0, urun.Stok);
    }

    [Fact]
    public void SiparisOlustur_GecerliAdet_AlanlariAtarVeStokDuser()
    {
        var urun = new Urun("Kalem", 10);
        var tarih = new DateTime(2026, 10, 18, 15, 0, 0);
        var siparis = Siparis.Olustur(urun, 3, tarih);
        Assert.Equal(7, urun.Stok);
        Assert.Same(urun, siparis.Urun);
        Assert.Equal(urun.UrunId, siparis.UrunId);
        Assert.Equal(3, siparis.Adet);
        Assert.Equal(tarih, siparis.Tarih);
        Assert.False(siparis.IsDeleted);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SiparisOlustur_GecersizAdet_StokDegismez(int adet)
    {
        var urun = new Urun("Kalem", 10);
        Assert.Throws<GecersizGirdiException>(() => Siparis.Olustur(urun, adet, DateTime.Today));
        Assert.Equal(10, urun.Stok);
    }

    [Fact]
    public void SiparisSil_IkinciSilme_ReddedilirVeStokBirKezIadeEdilir()
    {
        var urun = new Urun("Kalem", 10);
        var siparis = Siparis.Olustur(urun, 3, DateTime.Today);
        siparis.Sil(urun);
        Assert.True(siparis.IsDeleted);
        Assert.Equal(10, urun.Stok);
        Assert.Throws<GecersizIslemException>(() => siparis.Sil(urun));
        Assert.Equal(10, urun.Stok);
    }

    [Fact]
    public void SiparisSil_FarkliUrun_Reddedilir()
    {
        var urun = new Urun("Kalem", 10);
        var diger = new Urun("Defter", 5);
        typeof(Urun).GetProperty(nameof(Urun.UrunId))!.SetValue(diger, 2);
        var siparis = Siparis.Olustur(urun, 3, DateTime.Today);
        Assert.Throws<GecersizIslemException>(() => siparis.Sil(diger));
        Assert.False(siparis.IsDeleted);
        Assert.Equal(7, urun.Stok);
        Assert.Equal(5, diger.Stok);
    }

    [Fact]
    public void SiparisSil_StokIadesiTasarsa_SilinmisIsaretlenmez()
    {
        var urun = new Urun("Kalem", 1);
        var siparis = Siparis.Olustur(urun, 1, DateTime.Today);
        urun.StokAyarla(int.MaxValue);
        Assert.Throws<OverflowException>(() => siparis.Sil(urun));
        Assert.False(siparis.IsDeleted);
        Assert.Equal(int.MaxValue, urun.Stok);
    }

    [Fact]
    public void Siparis_NullUrun_Reddedilir()
    {
        Assert.Throws<ArgumentNullException>(() => Siparis.Olustur(null!, 1, DateTime.Today));
        var siparis = Siparis.Olustur(new Urun("Kalem", 1), 1, DateTime.Today);
        Assert.Throws<ArgumentNullException>(() => siparis.Sil(null!));
        Assert.False(siparis.IsDeleted);
    }
}
