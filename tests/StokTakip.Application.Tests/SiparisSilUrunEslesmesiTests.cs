using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.Tests;

/// <summary>Siparis.Sil stok iadesini yalnızca siparişin kendi ürününe yapar; kaydedilmemiş ürünlerin ID'si 0'dır.</summary>
public class SiparisSilUrunEslesmesiTests
{
    [Fact]
    public void SiparisSil_KaydedilmemisFarkliUrun_IdlerSifirOlsaDaReddedilir()
    {
        var urun = new Urun("Kalem", 10);
        var diger = new Urun("Defter", 5);
        var siparis = Siparis.Olustur(urun, 3, DateTime.Today);
        Assert.Equal(0, urun.UrunId);
        Assert.Equal(0, diger.UrunId);

        Assert.Throws<GecersizIslemException>(() => siparis.Sil(diger));

        Assert.False(siparis.IsDeleted);
        Assert.Equal(7, urun.Stok);
        Assert.Equal(5, diger.Stok);
    }

    [Fact]
    public void SiparisSil_KaydedilmemisAyniUrun_StokIadeEdilir()
    {
        var urun = new Urun("Kalem", 10);
        var siparis = Siparis.Olustur(urun, 3, DateTime.Today);

        siparis.Sil(urun);

        Assert.True(siparis.IsDeleted);
        Assert.Equal(10, urun.Stok);
    }

    [Fact]
    public void SiparisSil_AyniIdliAyriOkunmusUrun_StokIadeEdilir()
    {
        var urun = new Urun("Kalem", 10);
        IdAta(urun, 4);
        var siparis = Siparis.Olustur(urun, 3, DateTime.Today);
        var ayriOkunan = new Urun("Kalem", 7);
        IdAta(ayriOkunan, 4);

        siparis.Sil(ayriOkunan);

        Assert.True(siparis.IsDeleted);
        Assert.Equal(10, ayriOkunan.Stok);
    }

    private static void IdAta(Urun urun, int id) =>
        typeof(Urun).GetProperty(nameof(Urun.UrunId))!.SetValue(urun, id);
}
