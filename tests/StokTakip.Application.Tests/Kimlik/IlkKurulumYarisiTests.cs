using System.Data;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using StokTakip.Application.Kimlik;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Tests.Kimlik;

public class IlkKurulumYarisiTests
{
    private const string Parola = "Ilk-Parola-001";
    private readonly IKullaniciRepository _depo = Substitute.For<IKullaniciRepository>();
    private readonly IUnitOfWork _isBirimi = Substitute.For<IUnitOfWork>();
    private readonly KimlikService _servis;

    public IlkKurulumYarisiTests()
        => _servis = new KimlikService(_depo, _isBirimi, new HizliParolaHasher(), new OturumBaglami(), TimeProvider.System);

    [Fact]
    public async Task Transaction_icindeki_kontrol_baskasinin_kurulumunu_gorurse_kurulum_zaten_tamamlandi()
    {
        _depo.HerhangiBiriVarMiAsync(Arg.Any<CancellationToken>()).Returns(false, true);

        var hata = await Assert.ThrowsAsync<GecersizIslemException>(() => _servis.IlkAdminOlusturAsync("yonetici", Parola));

        Assert.Equal(KimlikService.KurulumTamamlandiMesaji, hata.Message);
        Assert.Equal("Kurulum zaten tamamlandı.", hata.Message);
        await _depo.DidNotReceive().AddAsync(Arg.Any<Kullanici>(), Arg.Any<CancellationToken>());
        await _isBirimi.Received(1).BeginTransactionAsync(IsolationLevel.Serializable, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Kilitlenme_ya_da_benzersizlik_hatasi_kurulum_tamamlandiysa_anlamli_mesaja_cevrilir()
    {
        _depo.HerhangiBiriVarMiAsync(Arg.Any<CancellationToken>()).Returns(false, false, true);
        _isBirimi.SaveChangesAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("deadlock victim 1205"));

        var hata = await Assert.ThrowsAsync<GecersizIslemException>(() => _servis.IlkAdminOlusturAsync("yonetici", Parola));

        Assert.Equal("Kurulum zaten tamamlandı.", hata.Message);
    }

    [Fact]
    public async Task Kurulum_tamamlanmamissa_asil_hata_korunur()
    {
        _depo.HerhangiBiriVarMiAsync(Arg.Any<CancellationToken>()).Returns(false);
        var asil = new InvalidOperationException("bağlantı koptu");
        _isBirimi.SaveChangesAsync(Arg.Any<CancellationToken>()).ThrowsAsync(asil);

        Assert.Same(asil, await Assert.ThrowsAsync<InvalidOperationException>(() => _servis.IlkAdminOlusturAsync("yonetici", Parola)));
    }

    [Fact]
    public async Task Iptal_cevrilmez()
    {
        _depo.HerhangiBiriVarMiAsync(Arg.Any<CancellationToken>()).Returns(false, false, true);
        _isBirimi.SaveChangesAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new OperationCanceledException());

        await Assert.ThrowsAsync<OperationCanceledException>(() => _servis.IlkAdminOlusturAsync("yonetici", Parola));
    }

    [Fact]
    public void Giris_ipucu_degismedi()
        => Assert.Equal("Çok sayıda hatalı deneme hesabı geçici olarak kilitleyebilir.", GirisMesajlari.GenelIpucu);
}
