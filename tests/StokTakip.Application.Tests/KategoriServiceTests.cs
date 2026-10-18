using NSubstitute;
using StokTakip.Application.Dtos;
using StokTakip.Application.Services;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Tests;

public class KategoriServiceTests
{
    private readonly IKategoriRepository _kategoriler = Substitute.For<IKategoriRepository>();
    private readonly IUnitOfWork _isBirimi = Substitute.For<IUnitOfWork>();
    private readonly CancellationToken _token = new CancellationTokenSource().Token;
    private readonly KategoriService _servis;

    public KategoriServiceTests() => _servis = new KategoriService(_kategoriler, _isBirimi);

    internal static Kategori KategoriOlustur(int id, string ad)
    {
        var kategori = new Kategori(ad);
        typeof(Kategori).GetProperty(nameof(Kategori.KategoriId))!.SetValue(kategori, id);
        return kategori;
    }

    [Fact]
    public async Task Listele_RepositorySirasiyla_DtoDoner()
    {
        _kategoriler.GetAllAsync(_token).Returns(new[] { KategoriOlustur(2, "Gıda"), KategoriOlustur(1, "Kırtasiye") });
        var sonuc = await _servis.KategorileriListeleAsync(_token);
        Assert.Equal(new[] { new KategoriDto(2, "Gıda"), new KategoriDto(1, "Kırtasiye") }, sonuc);
    }

    [Fact]
    public async Task Ekle_GecerliAd_KirpilmisAdlaBenzersizlikDenetlenipKaydedilir()
    {
        var sonuc = await _servis.KategoriEkleAsync("  Kırtasiye ", _token);

        Assert.Equal("Kırtasiye", sonuc.Ad);
        await _kategoriler.Received(1).AdKullaniliyorMuAsync("Kırtasiye", null, _token);
        await _kategoriler.Received(1).AddAsync(Arg.Is<Kategori>(k => k.Ad == "Kırtasiye"), _token);
        await _isBirimi.Received(1).SaveChangesAsync(_token);
    }

    [Fact]
    public async Task Ekle_KullanilanAd_TurkceMesajlaKaydetmedenReddeder()
    {
        _kategoriler.AdKullaniliyorMuAsync("Kırtasiye", null, _token).Returns(true);

        var hata = await Assert.ThrowsAsync<GecersizGirdiException>(() => _servis.KategoriEkleAsync("Kırtasiye", _token));

        Assert.Equal("'Kırtasiye' adında bir kategori zaten var.", hata.Message);
        Assert.Equal(nameof(Kategori.Ad), hata.AlanAdi);
        await _kategoriler.DidNotReceive().AddAsync(Arg.Any<Kategori>(), Arg.Any<CancellationToken>());
        Assert.Empty(_isBirimi.ReceivedCalls());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task Ekle_BosVeyaUzunAd_RepositoryeGitmedenReddeder(string? ad)
    {
        await Assert.ThrowsAsync<GecersizGirdiException>(() => _servis.KategoriEkleAsync(ad!, _token));
        await Assert.ThrowsAsync<GecersizGirdiException>(() => _servis.KategoriEkleAsync(new string('a', 101), _token));
        Assert.Empty(_kategoriler.ReceivedCalls());
        Assert.Empty(_isBirimi.ReceivedCalls());
    }

    [Fact]
    public async Task YenidenAdlandir_KendiAdiHaricTutularakDenetlenirVeKaydedilir()
    {
        var kategori = KategoriOlustur(3, "Kirtasiye");
        _kategoriler.GetByIdAsync(3, _token).Returns(kategori);

        var sonuc = await _servis.KategoriYenidenAdlandirAsync(3, " Kırtasiye ", _token);

        Assert.Equal(new KategoriDto(3, "Kırtasiye"), sonuc);
        Assert.Equal("Kırtasiye", kategori.Ad);
        await _kategoriler.Received(1).AdKullaniliyorMuAsync("Kırtasiye", 3, _token);
        await _isBirimi.Received(1).SaveChangesAsync(_token);
    }

    [Fact]
    public async Task YenidenAdlandir_BaskaKategorininAdi_VarligiDegistirmeden_Reddeder()
    {
        var kategori = KategoriOlustur(3, "Ofis");
        _kategoriler.GetByIdAsync(3, _token).Returns(kategori);
        _kategoriler.AdKullaniliyorMuAsync("Gıda", 3, _token).Returns(true);

        await Assert.ThrowsAsync<GecersizGirdiException>(() => _servis.KategoriYenidenAdlandirAsync(3, "Gıda", _token));

        Assert.Equal("Ofis", kategori.Ad);
        Assert.Empty(_isBirimi.ReceivedCalls());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(9)]
    public async Task YenidenAdlandirVeSil_KategoriYok_KayitBulunamadi(int id)
    {
        var adHatasi = await Assert.ThrowsAsync<KayitBulunamadiException>(() => _servis.KategoriYenidenAdlandirAsync(id, "Yeni", _token));
        var silmeHatasi = await Assert.ThrowsAsync<KayitBulunamadiException>(() => _servis.KategoriSilAsync(id, _token));

        Assert.All(new[] { adHatasi, silmeHatasi }, h => Assert.Equal("Kategori", h.KayitTuru));
        Assert.All(new[] { adHatasi, silmeHatasi }, h => Assert.Equal(id, h.Id));
        _kategoriler.DidNotReceive().Remove(Arg.Any<Kategori>());
        Assert.Empty(_isBirimi.ReceivedCalls());
    }

    [Fact]
    public async Task Sil_VarolanKategori_KaldirilipKaydedilir()
    {
        var kategori = KategoriOlustur(4, "Gıda");
        _kategoriler.GetByIdAsync(4, _token).Returns(kategori);

        await _servis.KategoriSilAsync(4, _token);

        _kategoriler.Received(1).Remove(kategori);
        await _isBirimi.Received(1).SaveChangesAsync(_token);
    }
}
