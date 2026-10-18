using NSubstitute;
using StokTakip.Application.Dtos;
using StokTakip.Application.Services;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Tests;

public class StokServiceKategoriVeEsikTests
{
    private readonly IUrunRepository _urunler = Substitute.For<IUrunRepository>();
    private readonly IKategoriRepository _kategoriler = Substitute.For<IKategoriRepository>();
    private readonly IUnitOfWork _isBirimi = Substitute.For<IUnitOfWork>();
    private readonly CancellationToken _token = new CancellationTokenSource().Token;
    private readonly StokService _servis;

    public StokServiceKategoriVeEsikTests() => _servis = new StokService(_urunler, _kategoriler, _isBirimi);

    [Fact]
    public async Task UrunEkle_EsikVeKategoriIle_KaydederVeDtoyaYansitir()
    {
        var kategori = KategoriServiceTests.KategoriOlustur(3, "Kırtasiye");
        _kategoriler.GetByIdAsync(3, _token).Returns(kategori);

        var sonuc = await _servis.UrunEkleAsync(new("Kalem", 4, KategoriId: 3, DusukStokEsigi: 8), _token);

        Assert.Equal(3, sonuc.KategoriId);
        Assert.Equal("Kırtasiye", sonuc.KategoriAdi);
        Assert.Equal(8, sonuc.DusukStokEsigi);
        Assert.Equal(StokDurumu.Kritik, sonuc.StokDurumu);
        await _urunler.Received(1).AddAsync(
            Arg.Is<Urun>(u => u.KategoriId == 3 && ReferenceEquals(u.Kategori, kategori) && u.DusukStokEsigi == 8), _token);
        await _isBirimi.Received(1).SaveChangesAsync(_token);
    }

    [Fact]
    public async Task UrunEkle_OpsiyonelAlanlarVerilmezse_KategorisizVeEsikOn()
    {
        var sonuc = await _servis.UrunEkleAsync(new("Kalem", 11), _token);

        Assert.Null(sonuc.KategoriId);
        Assert.Null(sonuc.KategoriAdi);
        Assert.Equal(10, sonuc.DusukStokEsigi);
        Assert.Equal(StokDurumu.Yeterli, sonuc.StokDurumu);
        Assert.Empty(_kategoriler.ReceivedCalls());
    }

    [Fact]
    public async Task UrunEkle_NegatifEsik_RepositoryeGitmedenReddeder()
    {
        var hata = await Assert.ThrowsAsync<GecersizGirdiException>(
            () => _servis.UrunEkleAsync(new("Kalem", 4, DusukStokEsigi: -1), _token));

        Assert.Equal(nameof(Urun.DusukStokEsigi), hata.AlanAdi);
        Assert.Empty(_urunler.ReceivedCalls());
        Assert.Empty(_kategoriler.ReceivedCalls());
        Assert.Empty(_isBirimi.ReceivedCalls());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    [InlineData(99)]
    public async Task UrunEkle_OlmayanKategori_KaydetmedenReddeder(int kategoriId)
    {
        var hata = await Assert.ThrowsAsync<GecersizGirdiException>(
            () => _servis.UrunEkleAsync(new("Kalem", 4, KategoriId: kategoriId), _token));

        Assert.Equal(nameof(Urun.KategoriId), hata.AlanAdi);
        Assert.Equal("Seçilen kategori bulunamadı.", hata.Message);
        await _urunler.DidNotReceive().AddAsync(Arg.Any<Urun>(), Arg.Any<CancellationToken>());
        Assert.Empty(_isBirimi.ReceivedCalls());
    }

    [Fact]
    public async Task UrunGuncelle_OpsiyonelAlanlarVerilmezse_EsikVeKategoriKorunur()
    {
        var kategori = KategoriServiceTests.KategoriOlustur(3, "Kırtasiye");
        var urun = new Urun("Kalem", 10, 4);
        urun.KategoriyeAta(kategori);
        _urunler.GetByIdAsync(7, _token).Returns(urun);

        var sonuc = await _servis.UrunGuncelleAsync(new(7, "Defter", 2), _token);

        Assert.Equal(4, urun.DusukStokEsigi);
        Assert.Same(kategori, urun.Kategori);
        Assert.Equal(new UrunDto(0, "Defter", 2, null, 3, "Kırtasiye", 4), sonuc);
        Assert.Empty(_kategoriler.ReceivedCalls());
        await _isBirimi.Received(1).SaveChangesAsync(_token);
    }

    [Fact]
    public async Task UrunGuncelle_EsikVeKategoriDegisir()
    {
        var yeni = KategoriServiceTests.KategoriOlustur(5, "Ofis");
        var urun = new Urun("Kalem", 10);
        urun.KategoriyeAta(KategoriServiceTests.KategoriOlustur(3, "Kırtasiye"));
        _urunler.GetByIdAsync(7, _token).Returns(urun);
        _kategoriler.GetByIdAsync(5, _token).Returns(yeni);

        var sonuc = await _servis.UrunGuncelleAsync(new(7, "Kalem", 10, DusukStokEsigi: 0, KategoriId: 5, KategoriGuncellensin: true), _token);

        Assert.Equal(0, urun.DusukStokEsigi);
        Assert.Equal(5, urun.KategoriId);
        Assert.Equal("Ofis", sonuc.KategoriAdi);
        Assert.Equal(StokDurumu.Yeterli, sonuc.StokDurumu);
        await _isBirimi.Received(1).SaveChangesAsync(_token);
    }

    [Fact]
    public async Task UrunGuncelle_KategoriGuncellensinVeNull_UrunKategorisizKalir()
    {
        var urun = new Urun("Kalem", 10);
        urun.KategoriyeAta(KategoriServiceTests.KategoriOlustur(3, "Kırtasiye"));
        _urunler.GetByIdAsync(7, _token).Returns(urun);

        var sonuc = await _servis.UrunGuncelleAsync(new(7, "Kalem", 10, KategoriId: null, KategoriGuncellensin: true), _token);

        Assert.Null(urun.KategoriId);
        Assert.Null(urun.Kategori);
        Assert.Null(sonuc.KategoriAdi);
        Assert.Empty(_kategoriler.ReceivedCalls());
    }

    [Fact]
    public async Task UrunGuncelle_NegatifEsik_HicbirAlaniDegistirmez()
    {
        var urun = new Urun("Kalem", 10, 4);
        _urunler.GetByIdAsync(7, _token).Returns(urun);

        await Assert.ThrowsAsync<GecersizGirdiException>(
            () => _servis.UrunGuncelleAsync(new(7, "Defter", 2, DusukStokEsigi: -1), _token));

        Assert.Equal("Kalem", urun.UrunAdi);
        Assert.Equal(10, urun.Stok);
        Assert.Equal(4, urun.DusukStokEsigi);
        Assert.Empty(_urunler.ReceivedCalls());
        Assert.Empty(_isBirimi.ReceivedCalls());
    }

    [Fact]
    public async Task UrunGuncelle_OlmayanKategori_HicbirAlaniDegistirmez()
    {
        var kategori = KategoriServiceTests.KategoriOlustur(3, "Kırtasiye");
        var urun = new Urun("Kalem", 10, 4);
        urun.KategoriyeAta(kategori);
        _urunler.GetByIdAsync(7, _token).Returns(urun);

        await Assert.ThrowsAsync<GecersizGirdiException>(() => _servis.UrunGuncelleAsync(
            new(7, "Defter", 2, DusukStokEsigi: 1, KategoriId: 99, KategoriGuncellensin: true), _token, new byte[] { 1 }));

        Assert.Equal("Kalem", urun.UrunAdi);
        Assert.Equal(10, urun.Stok);
        Assert.Equal(4, urun.DusukStokEsigi);
        Assert.Same(kategori, urun.Kategori);
        _urunler.DidNotReceive().SetBeklenenRowVersion(Arg.Any<Urun>(), Arg.Any<byte[]>());
        Assert.Empty(_isBirimi.ReceivedCalls());
    }

    [Fact]
    public async Task UrunGuncelle_KategoriGuncellensinFalse_KategoriIdYokSayilir()
    {
        var urun = new Urun("Kalem", 10);
        _urunler.GetByIdAsync(7, _token).Returns(urun);

        await _servis.UrunGuncelleAsync(new(7, "Kalem", 10, KategoriId: 99), _token);

        Assert.Null(urun.KategoriId);
        Assert.Empty(_kategoriler.ReceivedCalls());
    }

    [Fact]
    public async Task KategoriDeposuOlmadan_KategoriAtamasi_YapilandirmaHatasiVerir()
    {
        var servis = new StokService(_urunler, _isBirimi);
        await Assert.ThrowsAsync<InvalidOperationException>(() => servis.UrunEkleAsync(new("Kalem", 1, KategoriId: 3), _token));
        Assert.Empty(_isBirimi.ReceivedCalls());

        // Kategori istemeyen çağrılar eski kurucuyla çalışmaya devam eder.
        Assert.Equal(10, (await servis.UrunEkleAsync(new("Kalem", 1), _token)).DusukStokEsigi);
    }

    [Fact]
    public void UrunDto_StokDurumuEsitligeKatilmaz_HesaplanmisDoner()
    {
        var dto = new UrunDto(1, "Kalem", 6, DusukStokEsigi: 10);
        Assert.Equal(StokDurumu.Dusuk, dto.StokDurumu);
        Assert.Equal(new UrunDto(1, "Kalem", 6), dto);
        Assert.Equal(StokDurumu.Kritik, (dto with { Stok = 5 }).StokDurumu);
    }
}
