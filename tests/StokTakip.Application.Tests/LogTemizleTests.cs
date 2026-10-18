using NSubstitute;
using StokTakip.Application.Services;
using StokTakip.Application.Tests.Kimlik;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Tests;

public class LogTemizleTests
{
    [Theory]
    [InlineData("Kalem\r\nSAHTE SATIR", "Kalem  SAHTE SATIR")]
    [InlineData("a\tb\nc\rd", "a b c d")]
    [InlineData("x\u0000y\u001bz\u0085w", "x y z w")]
    [InlineData("a\x2028b\x2029c", "a b c")]
    [InlineData("Gıda ürünleri", "Gıda ürünleri")]
    [InlineData("", "")]
    public void Kontrol_karakterleri_bosluga_cevrilir(string girdi, string beklenen)
        => Assert.Equal(beklenen, LogTemizle.Metin(girdi));

    [Fact]
    public void Null_bos_metne_cevrilir() => Assert.Equal(string.Empty, LogTemizle.Metin(null));

    [Fact]
    public void Uzunluk_200_ile_sinirlanir()
    {
        Assert.Equal(200, LogTemizle.MaksimumUzunluk);
        Assert.Equal(new string('a', 200), LogTemizle.Metin(new string('a', 500)));
        Assert.Equal(new string('a', 200), LogTemizle.Metin(new string('a', 200)));
    }

    [Fact]
    public void Kesme_noktasinda_yarim_vekil_cifti_birakilmaz()
    {
        var metin = new string('a', 199) + "😀" + "b";
        var sonuc = LogTemizle.Metin(metin);
        Assert.True(sonuc.Length <= 200);
        Assert.False(char.IsHighSurrogate(sonuc[^1]));
    }

    [Fact]
    public async Task Is_kurali_ihlali_logu_kullanici_metnindeki_satir_sonlarini_tasimaz()
    {
        var kategoriler = Substitute.For<IKategoriRepository>();
        kategoriler.AdKullaniliyorMuAsync(Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<CancellationToken>()).Returns(true);
        var logger = new KayitTutanLogger<KategoriService>();
        var servis = new KategoriService(kategoriler, Substitute.For<IUnitOfWork>(), logger);

        await Assert.ThrowsAsync<GecersizGirdiException>(() => servis.KategoriEkleAsync("Gıda\r\n[ERR] sahte\tsatir"));

        Assert.NotEmpty(logger.Mesajlar);
        Assert.All(logger.Mesajlar, m => Assert.DoesNotContain(m, c => c is '\r' or '\n' or '\t'));
        Assert.Contains(logger.Mesajlar, m => m.StartsWith("Mesaj=") && m.Contains("Gıda  [ERR] sahte satir"));
    }
}
