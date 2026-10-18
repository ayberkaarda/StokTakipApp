using NSubstitute;
using StokTakip.Application.Denetim;
using StokTakip.Application.Kimlik;
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

    private const string HamHata = "Violation of UNIQUE KEY 'IX_Kullanicilar' gizli.kullanici sunucu=SQL01";

    [Fact]
    public async Task Basarisiz_giris_kaydi_yazilamazsa_ham_istisna_metni_gunluge_yazilmaz()
    {
        var depo = new BellekKullaniciRepository();
        var ozet = new HizliParolaHasher().Hashle("Dogru-Parola-42");
        await depo.AddAsync(new Domain.Entities.Kullanici("gizli.kullanici", ozet.Hash, ozet.Tuz, ozet.Iterasyon, Domain.KullaniciRolu.Operator));
        var kaydedici = Substitute.For<IDenetimKaydedici>();
        kaydedici.KaydetVeYazAsync(Arg.Any<DenetimOlayi>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException(HamHata, new TimeoutException(HamHata)));
        var logger = new KayitTutanLogger<KimlikService>();
        var servis = new KimlikService(
            depo, Substitute.For<IUnitOfWork>(), new HizliParolaHasher(), new OturumBaglami(), TimeProvider.System, logger, kaydedici);

        await Assert.ThrowsAsync<GecersizKimlikBilgisiException>(() => servis.GirisYapAsync("gizli.kullanici", "Yanlis-Parola-99"));

        Assert.Contains(logger.Mesajlar, m => m.Contains("Başarısız giriş denetim kaydına yazılamadı", StringComparison.Ordinal));
        Assert.Contains(logger.Mesajlar, m => m.Contains(nameof(InvalidOperationException), StringComparison.Ordinal));
#if DEBUG
        Assert.Contains(logger.Mesajlar, m => m.Contains(HamHata, StringComparison.Ordinal));
#else
        Assert.DoesNotContain(logger.Mesajlar, m => m.Contains("gizli.kullanici", StringComparison.Ordinal) || m.Contains("SQL01", StringComparison.Ordinal));
#endif
    }

    [Fact]
    public async Task Geri_alma_hatasi_ham_istisna_metni_olmadan_gunluge_yazilir()
    {
        var urunler = Substitute.For<IUrunRepository>();
        urunler.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(new Domain.Entities.Urun("Kalem", 5));
        var isBirimi = Substitute.For<IUnitOfWork>();
        var transaction = Substitute.For<IUnitOfWorkTransaction>();
        transaction.RollbackAsync(Arg.Any<CancellationToken>()).Returns<Task>(_ => throw new InvalidOperationException(HamHata));
        isBirimi.BeginTransactionAsync(Arg.Any<System.Data.IsolationLevel>(), Arg.Any<CancellationToken>()).Returns(transaction);
        isBirimi.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns<Task<int>>(_ => throw new TimeoutException("asıl"));
        var logger = new KayitTutanLogger<SiparisService>();
        var servis = new SiparisService(Substitute.For<ISiparisRepository>(), urunler, isBirimi, TimeProvider.System, logger);

        await Assert.ThrowsAsync<TimeoutException>(() => servis.SiparisOlusturAsync(new Dtos.SiparisOlusturIstegi(1, 1)));

        Assert.Contains(logger.Mesajlar, m => m.Contains("geri alınamadı", StringComparison.Ordinal));
#if !DEBUG
        Assert.DoesNotContain(logger.Mesajlar, m => m.Contains("SQL01", StringComparison.Ordinal));
#endif
    }
}
