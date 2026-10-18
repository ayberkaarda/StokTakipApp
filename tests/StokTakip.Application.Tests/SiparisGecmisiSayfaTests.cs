using NSubstitute;
using StokTakip.Application.Dtos;
using StokTakip.Application.Services;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Tests;

public class SiparisGecmisiSayfaTests
{
    private readonly IUrunRepository _urunler = Substitute.For<IUrunRepository>();
    private readonly ISiparisRepository _siparisler = Substitute.For<ISiparisRepository>();
    private readonly IUnitOfWork _isBirimi = Substitute.For<IUnitOfWork>();
    private readonly CancellationToken _token = new CancellationTokenSource().Token;
    private readonly SiparisService _servis;

    private SiparisFiltresi? _gelenFiltre;
    private int _gelenSayfa;
    private int _gelenBoyut;

    public SiparisGecmisiSayfaTests()
    {
        _servis = new SiparisService(_siparisler, _urunler, _isBirimi, TimeProvider.System);
        _siparisler.GetGecmisSayfaAsync(Arg.Any<SiparisFiltresi>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                _gelenFiltre = ci.ArgAt<SiparisFiltresi>(0);
                _gelenSayfa = ci.ArgAt<int>(1);
                _gelenBoyut = ci.ArgAt<int>(2);
                return new SayfaliSonuc<Siparis>(Array.Empty<Siparis>(), 0, _gelenSayfa, _gelenBoyut);
            });
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(int.MinValue, 1)]
    [InlineData(3, 3)]
    public async Task Sayfa_birden_kucukse_bire_cekilir(int istenen, int beklenen)
    {
        await _servis.SiparisGecmisiSayfaGetirAsync(new(Sayfa: istenen), _token);

        Assert.Equal(beklenen, _gelenSayfa);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(int.MinValue, 1)]
    [InlineData(1, 1)]
    [InlineData(50, 50)]
    [InlineData(200, 200)]
    [InlineData(201, 200)]
    [InlineData(int.MaxValue, 200)]
    public async Task SayfaBoyutu_1_ile_200_arasina_kistirilir(int istenen, int beklenen)
    {
        await _servis.SiparisGecmisiSayfaGetirAsync(new(SayfaBoyutu: istenen), _token);

        Assert.Equal(beklenen, _gelenBoyut);
    }

    [Fact]
    public async Task Baslangic_bitisten_sonraysa_GecersizGirdiException_firlatilir_ve_repository_cagrilmaz()
    {
        var sorgu = new SiparisGecmisiSorgusu(new DateTime(2026, 5, 2), new DateTime(2026, 5, 1));

        var ex = await Assert.ThrowsAsync<GecersizGirdiException>(
            () => _servis.SiparisGecmisiSayfaGetirAsync(sorgu, _token));

        Assert.Equal(nameof(SiparisGecmisiSorgusu.Baslangic), ex.AlanAdi);
        await _siparisler.DidNotReceiveWithAnyArgs().GetGecmisSayfaAsync(default!, default, default, default);
    }

    [Fact]
    public async Task Baslangic_ve_bitis_esitse_kabul_edilir()
    {
        var gun = new DateTime(2026, 5, 1);

        await _servis.SiparisGecmisiSayfaGetirAsync(new(gun, gun), _token);

        Assert.Equal(gun, _gelenFiltre!.Baslangic);
        Assert.Equal(gun, _gelenFiltre.Bitis);
    }

    [Fact]
    public async Task Ayni_gun_icinde_baslangic_saati_bitis_saatinden_sonraysa_reddedilir()
    {
        var sorgu = new SiparisGecmisiSorgusu(new DateTime(2026, 5, 1, 15, 0, 0), new DateTime(2026, 5, 1, 9, 0, 0));

        await Assert.ThrowsAsync<GecersizGirdiException>(() => _servis.SiparisGecmisiSayfaGetirAsync(sorgu, _token));
    }

    [Fact]
    public async Task Tek_tarafli_tarih_araligi_dogrulamaya_takilmaz()
    {
        await _servis.SiparisGecmisiSayfaGetirAsync(new(Baslangic: DateTime.MaxValue), _token);
        Assert.Equal(DateTime.MaxValue, _gelenFiltre!.Baslangic);
        Assert.Null(_gelenFiltre.Bitis);

        await _servis.SiparisGecmisiSayfaGetirAsync(new(Bitis: DateTime.MinValue), _token);
        Assert.Null(_gelenFiltre!.Baslangic);
        Assert.Equal(DateTime.MinValue, _gelenFiltre.Bitis);
    }

    [Theory]
    [InlineData("  kalem  ", "kalem")]
    [InlineData("\tKırmızı Kalem\n", "Kırmızı Kalem")]
    [InlineData("a  b", "a  b")]
    public async Task UrunAdi_bastan_ve_sondan_kirpilir(string girdi, string beklenen)
    {
        await _servis.SiparisGecmisiSayfaGetirAsync(new(UrunAdi: girdi), _token);

        Assert.Equal(beklenen, _gelenFiltre!.UrunAdi);
    }

    [Fact]
    public async Task UrunAdi_null_ise_null_iletilir()
    {
        await _servis.SiparisGecmisiSayfaGetirAsync(new(UrunAdi: null), _token);

        Assert.Null(_gelenFiltre!.UrunAdi);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UrunAdi_bos_veya_bosluksa_filtre_anlamsiz_hale_gelir(string girdi)
    {
        await _servis.SiparisGecmisiSayfaGetirAsync(new(UrunAdi: girdi), _token);

        Assert.True(string.IsNullOrWhiteSpace(_gelenFiltre!.UrunAdi));
    }

    [Fact]
    public async Task UrunAdi_100_karakterde_kesilir()
    {
        var uzun = new string('a', 100) + "BCDEF";

        await _servis.SiparisGecmisiSayfaGetirAsync(new(UrunAdi: uzun), _token);

        Assert.Equal(new string('a', 100), _gelenFiltre!.UrunAdi);
    }

    [Fact]
    public async Task UrunAdi_tam_100_karakterse_degismez()
    {
        var tam = new string('x', 99) + "Y";

        await _servis.SiparisGecmisiSayfaGetirAsync(new(UrunAdi: tam), _token);

        Assert.Equal(tam, _gelenFiltre!.UrunAdi);
    }

    [Fact]
    public async Task UrunAdi_once_kirpilir_sonra_kesilir()
    {
        var girdi = "     " + new string('k', 100) + "     ";

        await _servis.SiparisGecmisiSayfaGetirAsync(new(UrunAdi: girdi), _token);

        Assert.Equal(new string('k', 100), _gelenFiltre!.UrunAdi);
    }

    [Fact]
    public async Task Filtre_sayfa_boyut_ve_token_repositorye_aynen_iletilir()
    {
        var baslangic = new DateTime(2026, 1, 1, 8, 30, 0);
        var bitis = new DateTime(2026, 1, 31);
        var sorgu = new SiparisGecmisiSorgusu(baslangic, bitis, " Defter ", 4, 25);

        await _servis.SiparisGecmisiSayfaGetirAsync(sorgu, _token);

        await _siparisler.Received(1).GetGecmisSayfaAsync(
            new SiparisFiltresi(baslangic, bitis, "Defter", null), 4, 25, _token);
        Assert.Null(_gelenFiltre!.UrunId);
    }

    [Fact]
    public async Task Varsayilan_sorgu_bos_filtre_ile_ilk_sayfayi_20lik_ister()
    {
        await _servis.SiparisGecmisiSayfaGetirAsync(new(), _token);

        await _siparisler.Received(1).GetGecmisSayfaAsync(new SiparisFiltresi(), 1, 20, _token);
    }

    [Fact]
    public async Task Sorgu_null_ise_ArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _servis.SiparisGecmisiSayfaGetirAsync(null!, _token));
    }

    [Fact]
    public async Task Sonuc_DTOya_eksiksiz_ve_sirasi_korunarak_eslenir()
    {
        var kalem = UrunIdli(new Urun("Kalem", 100), 7);
        var defter = UrunIdli(new Urun("Defter", 100), 9);
        var s1 = SiparisIdli(Siparis.Olustur(kalem, 3, new DateTime(2026, 3, 2, 10, 0, 0)), 41);
        var s2 = SiparisIdli(Siparis.Olustur(defter, 5, new DateTime(2026, 3, 1, 9, 15, 0)), 40);
        _siparisler.GetGecmisSayfaAsync(Arg.Any<SiparisFiltresi>(), 2, 2, _token)
            .Returns(new SayfaliSonuc<Siparis>(new[] { s1, s2 }, 7, 2, 2));

        var sonuc = await _servis.SiparisGecmisiSayfaGetirAsync(new(Sayfa: 2, SayfaBoyutu: 2), _token);

        Assert.Equal(7, sonuc.ToplamKayit);
        Assert.Equal(2, sonuc.Sayfa);
        Assert.Equal(2, sonuc.SayfaBoyutu);
        Assert.Equal(4, sonuc.ToplamSayfa);
        Assert.Collection(sonuc.Ogeler,
            d => Assert.Equal(new SiparisDto(41, 7, "Kalem", 3, new DateTime(2026, 3, 2, 10, 0, 0)), d),
            d => Assert.Equal(new SiparisDto(40, 9, "Defter", 5, new DateTime(2026, 3, 1, 9, 15, 0)), d));
    }

    [Fact]
    public async Task Bos_sonuc_bos_liste_ve_sifir_sayfa_olarak_eslenir()
    {
        var sonuc = await _servis.SiparisGecmisiSayfaGetirAsync(new(Sayfa: 5, SayfaBoyutu: 10), _token);

        Assert.Empty(sonuc.Ogeler);
        Assert.Equal(0, sonuc.ToplamKayit);
        Assert.Equal(0, sonuc.ToplamSayfa);
        Assert.Equal(5, sonuc.Sayfa);
        Assert.Equal(10, sonuc.SayfaBoyutu);
    }

    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(1, 20, 1)]
    [InlineData(19, 20, 1)]
    [InlineData(20, 20, 1)]
    [InlineData(21, 20, 2)]
    [InlineData(40, 20, 2)]
    [InlineData(41, 20, 3)]
    [InlineData(250, 200, 2)]
    [InlineData(7, 1, 7)]
    [InlineData(int.MaxValue, 1, int.MaxValue)]
    [InlineData(int.MaxValue, 200, 10737419)]
    public void ToplamSayfa_tavan_bolme_ile_hesaplanir(int toplamKayit, int boyut, int beklenen)
    {
        Assert.Equal(beklenen, new SayfaliSonuc<int>(Array.Empty<int>(), toplamKayit, 1, boyut).ToplamSayfa);
        Assert.Equal(beklenen, new SayfaliSonucDto<int>(Array.Empty<int>(), toplamKayit, 1, boyut).ToplamSayfa);
    }

    [Fact]
    public void ToplamSayfa_kayit_arttikca_azalmaz()
    {
        var onceki = 0;
        for (var kayit = 0; kayit <= 101; kayit++)
        {
            var sayfa = new SayfaliSonuc<int>(Array.Empty<int>(), kayit, 1, 10).ToplamSayfa;
            Assert.InRange(sayfa, onceki, onceki + 1);
            Assert.Equal((kayit + 9) / 10, sayfa);
            onceki = sayfa;
        }
    }

    [Fact]
    public void SiparisFiltresi_varsayilanlari_bostur()
    {
        var filtre = new SiparisFiltresi();

        Assert.Null(filtre.Baslangic);
        Assert.Null(filtre.Bitis);
        Assert.Null(filtre.UrunAdi);
        Assert.Null(filtre.UrunId);
    }

    private static Urun UrunIdli(Urun urun, int id)
    {
        typeof(Urun).GetProperty(nameof(Urun.UrunId))!.SetValue(urun, id);
        return urun;
    }

    private static Siparis SiparisIdli(Siparis siparis, int id)
    {
        typeof(Siparis).GetProperty(nameof(Siparis.SiparisId))!.SetValue(siparis, id);
        return siparis;
    }
}
