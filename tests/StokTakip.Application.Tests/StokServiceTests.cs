using NSubstitute;
using StokTakip.Application.Dtos;
using StokTakip.Application.Services;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Tests;

public class StokServiceTests
{
    private readonly IUrunRepository _urunler = Substitute.For<IUrunRepository>();
    private readonly IUnitOfWork _isBirimi = Substitute.For<IUnitOfWork>();
    private readonly CancellationToken _token = new CancellationTokenSource().Token;
    private readonly StokService _servis;

    public StokServiceTests() => _servis = new StokService(_urunler, _isBirimi);

    [Theory]
    [InlineData(null, 0, "UrunAdi")]
    [InlineData("", 0, "UrunAdi")]
    [InlineData("   ", 0, "UrunAdi")]
    [InlineData("Kalem", -1, "Stok")]
    public async Task UrunEkle_GecersizGirdi_KaydetmedenReddeder(string? ad, int stok, string alan)
    {
        var hata = await Assert.ThrowsAsync<GecersizGirdiException>(() => _servis.UrunEkleAsync(new(ad!, stok), _token));
        Assert.Equal(alan, hata.AlanAdi);
        Assert.Empty(_urunler.ReceivedCalls());
        Assert.Empty(_isBirimi.ReceivedCalls());
    }

    [Fact]
    public async Task UrunEkle_IkiYuzKarakterUstuAd_Reddedilir()
    {
        await Assert.ThrowsAsync<GecersizGirdiException>(() => _servis.UrunEkleAsync(new(new string('a', 201), 0), _token));
        Assert.Empty(_urunler.ReceivedCalls());
        Assert.Empty(_isBirimi.ReceivedCalls());
    }

    [Fact]
    public async Task UrunEkle_GecerliGirdi_AdiKirparVeKaydeder()
    {
        var sonuc = await _servis.UrunEkleAsync(new(" Kalem ", 4), _token);
        Assert.Equal("Kalem", sonuc.UrunAdi);
        Assert.Equal(4, sonuc.Stok);
        await _urunler.Received(1).AddAsync(Arg.Is<Urun>(u => u.UrunAdi == "Kalem" && u.Stok == 4), _token);
        await _isBirimi.Received(1).SaveChangesAsync(_token);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task StokGuncelle_GecerliStok_RowVersionSeceneginiIletir(bool surumVar)
    {
        var urun = new Urun("Kalem", 10);
        byte[]? surum = surumVar ? new byte[] { 1, 2 } : null;
        _urunler.GetByIdAsync(7, _token).Returns(urun);
        var sonuc = await _servis.StokGuncelleAsync(7, 0, _token, surum);
        Assert.Equal(0, urun.Stok);
        Assert.Equal(0, sonuc.Stok);
        Assert.Equal("Kalem", sonuc.UrunAdi);
        if (surumVar)
            _urunler.Received(1).SetBeklenenRowVersion(urun, surum!);
        else
            _urunler.DidNotReceive().SetBeklenenRowVersion(Arg.Any<Urun>(), Arg.Any<byte[]>());
        await _isBirimi.Received(1).SaveChangesAsync(_token);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UrunGuncelle_AdVeStokGuncellenir_RowVersionIletilir(bool surumVar)
    {
        var urun = new Urun("Kalem", 10);
        byte[]? surum = surumVar ? new byte[] { 3, 4 } : null;
        _urunler.GetByIdAsync(7, _token).Returns(urun);
        var sonuc = await _servis.UrunGuncelleAsync(new(7, " Defter ", 12), _token, surum);
        Assert.Equal("Defter", urun.UrunAdi);
        Assert.Equal(12, urun.Stok);
        Assert.Equal("Defter", sonuc.UrunAdi);
        Assert.Equal(12, sonuc.Stok);
        if (surumVar)
            _urunler.Received(1).SetBeklenenRowVersion(urun, surum!);
        else
            _urunler.DidNotReceive().SetBeklenenRowVersion(Arg.Any<Urun>(), Arg.Any<byte[]>());
        await _isBirimi.Received(1).SaveChangesAsync(_token);
    }

    [Theory]
    [InlineData("", 2)]
    [InlineData("Defter", -1)]
    [InlineData(null, 2)]
    public async Task UrunGuncelle_GecersizGirdi_VarligiDegistirmez(string? ad, int stok)
    {
        var urun = new Urun("Kalem", 10);
        _urunler.GetByIdAsync(7, _token).Returns(urun);
        await Assert.ThrowsAsync<GecersizGirdiException>(() => _servis.UrunGuncelleAsync(new(7, ad!, stok), _token));
        Assert.Equal("Kalem", urun.UrunAdi);
        Assert.Equal(10, urun.Stok);
        Assert.Empty(_urunler.ReceivedCalls());
        Assert.Empty(_isBirimi.ReceivedCalls());
    }

    [Fact]
    public async Task UrunGuncelle_UzunAd_Reddedilir()
    {
        await Assert.ThrowsAsync<GecersizGirdiException>(() => _servis.UrunGuncelleAsync(new(7, new string('a', 201), 0), _token));
        Assert.Empty(_urunler.ReceivedCalls());
        Assert.Empty(_isBirimi.ReceivedCalls());
    }

    [Fact]
    public async Task StokGuncelle_NegatifStok_KaydetmedenReddeder()
    {
        await Assert.ThrowsAsync<GecersizGirdiException>(() => _servis.StokGuncelleAsync(7, -1, _token));
        Assert.Empty(_urunler.ReceivedCalls());
        Assert.Empty(_isBirimi.ReceivedCalls());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(7)]
    public async Task DegistirmeIslemleri_UrunYok_KayitBulunamadi(int id)
    {
        var stokHatasi = await Assert.ThrowsAsync<KayitBulunamadiException>(() => _servis.StokGuncelleAsync(id, 1, _token));
        var adHatasi = await Assert.ThrowsAsync<KayitBulunamadiException>(() => _servis.UrunGuncelleAsync(new(id, "Kalem", 1), _token));
        var silmeHatasi = await Assert.ThrowsAsync<KayitBulunamadiException>(() => _servis.UrunSilAsync(id, _token));
        Assert.All(new[] { stokHatasi, adHatasi, silmeHatasi }, h => Assert.Equal(id, h.Id));
        await _isBirimi.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        _urunler.DidNotReceive().Remove(Arg.Any<Urun>());
    }

    [Fact]
    public async Task UrunSil_SiparisliUrun_SilmedenReddeder()
    {
        _urunler.GetByIdAsync(7, _token).Returns(new Urun("Kalem", 10));
        _urunler.HasSiparisAsync(7, _token).Returns(true);
        await Assert.ThrowsAsync<GecersizIslemException>(() => _servis.UrunSilAsync(7, _token, new byte[] { 1 }));
        _urunler.DidNotReceive().Remove(Arg.Any<Urun>());
        _urunler.DidNotReceive().SetBeklenenRowVersion(Arg.Any<Urun>(), Arg.Any<byte[]>());
        Assert.Empty(_isBirimi.ReceivedCalls());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UrunSil_SiparissizUrun_SilerVeRowVersionIletir(bool surumVar)
    {
        var urun = new Urun("Kalem", 10);
        byte[]? surum = surumVar ? new byte[] { 1 } : null;
        _urunler.GetByIdAsync(7, _token).Returns(urun);
        await _servis.UrunSilAsync(7, _token, surum);
        await _urunler.Received(1).HasSiparisAsync(7, _token);
        _urunler.Received(1).Remove(urun);
        if (surumVar)
            _urunler.Received(1).SetBeklenenRowVersion(urun, surum!);
        else
            _urunler.DidNotReceive().SetBeklenenRowVersion(Arg.Any<Urun>(), Arg.Any<byte[]>());
        await _isBirimi.Received(1).SaveChangesAsync(_token);
    }

    [Fact]
    public async Task UrunleriListele_RepositoryDegerleriniDoner()
    {
        _urunler.GetAllAsync(_token).Returns(new[] { new Urun("Kalem", 2), new Urun("Defter", 3) });
        var sonuc = await _servis.UrunleriListeleAsync(_token);
        Assert.Equal(2, sonuc.Count);
        Assert.Equal(new UrunDto(0, "Kalem", 2), sonuc[0]);
        Assert.Equal(new UrunDto(0, "Defter", 3), sonuc[1]);
        await _urunler.Received(1).GetAllAsync(_token);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(7)]
    public async Task UrunGetir_UrunYok_NullDoner(int id)
    {
        Assert.Null(await _servis.UrunGetirAsync(id, _token));
        if (id <= 0)
            Assert.Empty(_urunler.ReceivedCalls());
        else
            await _urunler.Received(1).GetByIdAsync(id, _token);
    }

    [Fact]
    public async Task UrunGetir_UrunVar_DtoDoner()
    {
        _urunler.GetByIdAsync(7, _token).Returns(new Urun("Kalem", 8));
        Assert.Equal(new UrunDto(0, "Kalem", 8), await _servis.UrunGetirAsync(7, _token));
    }

    [Fact]
    public async Task UrunIslemleri_NullIstek_Reddedilir()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _servis.UrunEkleAsync(null!, _token));
        await Assert.ThrowsAsync<ArgumentNullException>(() => _servis.UrunGuncelleAsync(null!, _token));
        Assert.Empty(_isBirimi.ReceivedCalls());
    }
}
