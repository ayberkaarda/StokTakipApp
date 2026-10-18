using NSubstitute;
using StokTakip.Application.DisaAktarim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Kimlik;
using StokTakip.Application.Services;
using StokTakip.Application.Tests.Kimlik;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.Tests;

public class CsvYetkiOnKontrolTests
{
    private readonly ISiparisDisaAktarimService _ic = Substitute.For<ISiparisDisaAktarimService>();
    private readonly BellekKullaniciRepository _depo = new();
    private readonly OturumBaglami _oturum = new();

    private YetkiDenetimliSiparisDisaAktarimService Servis() => new(_ic, new OturumYetkiDenetcisi(_oturum, _depo));

    private Kullanici OturumAc(KullaniciRolu rol)
    {
        var k = new Kullanici("kullanici", [1], [1], 1, rol);
        _depo.AddAsync(k).GetAwaiter().GetResult();
        _oturum.OturumAc(new OturumKullanicisi(k.KullaniciId, k.KullaniciAdi, rol));
        return k;
    }

    [Fact]
    public async Task YetkiKontrolu_oturum_yokken_reddeder()
    {
        var hata = await Assert.ThrowsAsync<YetkisizIslemException>(() => Servis().YetkiKontrolEtAsync());
        Assert.Equal(YetkisizIslemException.OturumGerekliMesaji, hata.Message);
        Assert.Empty(_ic.ReceivedCalls());
    }

    [Fact]
    public async Task YetkiKontrolu_Operator_icin_reddeder()
    {
        OturumAc(KullaniciRolu.Operator);
        await Assert.ThrowsAsync<YetkisizIslemException>(() => Servis().YetkiKontrolEtAsync());
        Assert.Empty(_ic.ReceivedCalls());
    }

    [Fact]
    public async Task YetkiKontrolu_Admin_icin_gecer_ve_veri_yazmaz()
    {
        OturumAc(KullaniciRolu.Admin);
        await Servis().YetkiKontrolEtAsync();
        await _ic.DidNotReceiveWithAnyArgs().SiparisleriCsvAktarAsync(default!, default!, default);
    }

    [Fact]
    public async Task YetkiKontrolu_veritabaninda_pasiflestirilen_Admin_icin_reddeder()
    {
        OturumAc(KullaniciRolu.Admin).AktiflikAyarla(false);
        await Assert.ThrowsAsync<YetkisizIslemException>(() => Servis().YetkiKontrolEtAsync());
        Assert.Null(_oturum.MevcutKullanici);
    }

    [Fact]
    public async Task Yetkisiz_aktarim_akisa_tek_bayt_yazmaz()
    {
        OturumAc(KullaniciRolu.Operator);
        using var akis = new MemoryStream();

        await Assert.ThrowsAsync<YetkisizIslemException>(() => Servis().SiparisleriCsvAktarAsync(new SiparisGecmisiSorgusu(), akis));

        Assert.Equal(0, akis.Length);
        Assert.Empty(_ic.ReceivedCalls());
    }

    [Fact]
    public async Task Ic_servisin_yetki_kontrolu_denetimsizdir()
    {
        var ic = new SiparisDisaAktarimService(Substitute.For<ISiparisService>());
        await ic.YetkiKontrolEtAsync();
    }
}
