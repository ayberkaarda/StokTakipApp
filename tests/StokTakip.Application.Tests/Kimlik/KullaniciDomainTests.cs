using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.Tests.Kimlik;

public class KullaniciDomainTests
{
    private static readonly DateTime T0 = new(2026, 10, 18, 12, 0, 0, DateTimeKind.Utc);

    internal static Kullanici Olustur(string ad = "ali", KullaniciRolu rol = KullaniciRolu.Operator)
        => new(ad, [1, 2, 3], [4, 5, 6], 1000, rol);

    [Theory]
    [InlineData("Ali.Veli", "ali.veli")]
    [InlineData("  OPERATOR_1 ", "operator_1")]
    [InlineData("a-b", "a-b")]
    public void KullaniciAdi_KirpilirVeKucukHarfeCevrilir(string girdi, string beklenen)
        => Assert.Equal(beklenen, Olustur(girdi).KullaniciAdi);

    [Theory]
    [InlineData(null, "Kullanıcı adı boş olamaz.")]
    [InlineData("   ", "Kullanıcı adı boş olamaz.")]
    [InlineData("ab", "Kullanıcı adı 3 ile 50 karakter arasında olmalıdır.")]
    [InlineData("ayşe", "Kullanıcı adı yalnızca İngilizce harf (a-z), rakam, nokta, alt çizgi ve tire içerebilir.")]
    [InlineData("ali veli", "Kullanıcı adı yalnızca İngilizce harf (a-z), rakam, nokta, alt çizgi ve tire içerebilir.")]
    [InlineData("ali@x", "Kullanıcı adı yalnızca İngilizce harf (a-z), rakam, nokta, alt çizgi ve tire içerebilir.")]
    public void GecersizKullaniciAdi_TurkceMesajlaReddedilir(string? ad, string mesaj)
    {
        var hata = Assert.Throws<GecersizGirdiException>(() => Kullanici.KullaniciAdiDogrula(ad));
        Assert.Equal(mesaj, hata.Message);
        Assert.Equal(nameof(Kullanici.KullaniciAdi), hata.AlanAdi);
    }

    [Fact]
    public void KullaniciAdi_50KarakterGecerli_51Reddedilir()
    {
        Assert.Equal(50, Kullanici.KullaniciAdiDogrula(new string('a', 50)).Length);
        Assert.Throws<GecersizGirdiException>(() => Kullanici.KullaniciAdiDogrula(new string('a', 51)));
    }

    [Fact]
    public void Normallestir_GecersizAdda_HataFirlatmadanFalseDoner()
    {
        Assert.False(Kullanici.KullaniciAdiNormallestir(null, out _));
        Assert.False(Kullanici.KullaniciAdiNormallestir("ayşe", out _));
        Assert.True(Kullanici.KullaniciAdiNormallestir(" Ali ", out var ad));
        Assert.Equal("ali", ad);
    }

    [Fact]
    public void TanimsizRol_Reddedilir()
    {
        Assert.Throws<GecersizGirdiException>(() => Olustur(rol: (KullaniciRolu)7));
        Assert.Throws<GecersizGirdiException>(() => Olustur().RolAta((KullaniciRolu)(-1)));
    }

    [Fact]
    public void YeniKullanici_AktifKilitsizSayaciSifir()
    {
        var k = Olustur();
        Assert.True(k.Aktif);
        Assert.Equal(0, k.BasarisizDenemeSayisi);
        Assert.Null(k.KilitBitis);
        Assert.Null(k.SonGirisTarihi);
    }

    [Fact]
    public void BesinciBasarisizDenemede_15DakikaKilitlenir()
    {
        var k = Olustur();
        for (var i = 1; i < Kullanici.KilitEsigi; i++)
        {
            Assert.False(k.BasarisizGirisKaydet(T0));
            Assert.False(k.KilitliMi(T0));
        }

        Assert.True(k.BasarisizGirisKaydet(T0));

        Assert.Equal(5, Kullanici.KilitEsigi);
        Assert.Equal(5, k.BasarisizDenemeSayisi);
        Assert.Equal(T0.AddMinutes(15), k.KilitBitis);
        Assert.True(k.KilitliMi(T0.AddMinutes(15).AddTicks(-1)));
        Assert.False(k.KilitliMi(T0.AddMinutes(15)));
    }

    [Fact]
    public void KilitliykenDenemeler_SayilmazVeKilidiUzatmaz_BasariliGirisReddedilir()
    {
        var k = Olustur();
        for (var i = 0; i < 5; i++) k.BasarisizGirisKaydet(T0);

        Assert.False(k.BasarisizGirisKaydet(T0.AddMinutes(5)));
        Assert.Throws<HesapKilitliException>(() => k.BasariliGirisKaydet(T0.AddMinutes(10)));

        Assert.Equal(5, k.BasarisizDenemeSayisi);
        Assert.Equal(T0.AddMinutes(15), k.KilitBitis);
        Assert.Null(k.SonGirisTarihi);
    }

    [Fact]
    public void KilitBitince_IlkYanlisDeneme_SayaciYenidenBaslatir()
    {
        var k = Olustur();
        for (var i = 0; i < 5; i++) k.BasarisizGirisKaydet(T0);

        Assert.False(k.BasarisizGirisKaydet(T0.AddMinutes(15)));

        Assert.Equal(1, k.BasarisizDenemeSayisi);
        Assert.Null(k.KilitBitis);
    }

    [Fact]
    public void BasariliGiris_SayaciSifirlarVeSonGirisiYazar()
    {
        var k = Olustur();
        for (var i = 0; i < 4; i++) k.BasarisizGirisKaydet(T0);

        k.BasariliGirisKaydet(T0.AddMinutes(1));

        Assert.Equal(0, k.BasarisizDenemeSayisi);
        Assert.Null(k.KilitBitis);
        Assert.Equal(T0.AddMinutes(1), k.SonGirisTarihi);
    }

    [Fact]
    public void ParolayiAyarla_BosOzetVeyaGecersizIterasyonuReddeder_DizileriKopyalar()
    {
        var k = Olustur();
        Assert.Throws<ArgumentException>(() => k.ParolayiAyarla([], [1], 1));
        Assert.Throws<ArgumentException>(() => k.ParolayiAyarla([1], [], 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => k.ParolayiAyarla([1], [1], 0));

        var hash = new byte[] { 9, 9 };
        k.ParolayiAyarla(hash, [8], 2000);
        hash[0] = 0;
        Assert.Equal(new byte[] { 9, 9 }, k.ParolaHash);
        Assert.Equal(2000, k.Iterasyon);
    }

    [Fact]
    public void HesapKilitliException_GenelMesajParolaIcermez_GecersizKimlikMesajiSabit()
    {
        Assert.Equal("Kullanıcı adı veya parola hatalı.", new GecersizKimlikBilgisiException().Message);
        Assert.Equal(T0, new HesapKilitliException(T0).KilitBitisUtc);
        Assert.Equal("Bu işlem için yetkiniz yok.", new YetkisizIslemException().Message);
    }
}
