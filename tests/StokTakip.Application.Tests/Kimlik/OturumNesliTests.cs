using NSubstitute;
using StokTakip.Application.Kimlik;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Tests.Kimlik;

public class OturumNesliTests
{
    private const string Parola = "Dogru-Parola-42";
    private static readonly OturumKullanicisi Ali = new(1, "ali", KullaniciRolu.Admin);
    private static readonly OturumKullanicisi Veli = new(2, "veli", KullaniciRolu.Operator);

    private readonly OturumBaglami _oturum = new();

    [Fact]
    public void Her_acma_ve_kapama_nesli_artirir()
    {
        var n0 = _oturum.Anlik.Nesil;
        _oturum.OturumAc(Ali);
        var n1 = _oturum.Anlik;
        _oturum.OturumuKapat();
        var n2 = _oturum.Anlik;

        Assert.True(n1.Nesil > n0);
        Assert.Equal(Ali, n1.Kullanici);
        Assert.True(n2.Nesil > n1.Nesil);
        Assert.Null(n2.Kullanici);
    }

    [Fact]
    public void Nesil_degistiyse_kosullu_acma_oturumu_acmaz()
    {
        var nesil = _oturum.Anlik.Nesil;
        _oturum.OturumuKapat();

        Assert.False(_oturum.OturumAc(Ali, nesil));
        Assert.Null(_oturum.MevcutKullanici);
    }

    [Fact]
    public void Nesil_ayniysa_kosullu_acma_oturumu_acar()
    {
        var nesil = _oturum.Anlik.Nesil;

        Assert.True(_oturum.OturumAc(Ali, nesil));
        Assert.Equal(Ali, _oturum.MevcutKullanici);
        Assert.True(_oturum.Anlik.Nesil > nesil);
    }

    [Fact]
    public void Kosullu_kapatma_arada_acilan_yeni_oturumu_kapatmaz()
    {
        _oturum.OturumAc(Ali);
        var eski = _oturum.Anlik.Nesil;
        _oturum.OturumAc(Veli);

        Assert.False(_oturum.OturumuKapat(eski));
        Assert.Equal(Veli, _oturum.MevcutKullanici);
        Assert.True(_oturum.OturumuKapat(_oturum.Anlik.Nesil));
        Assert.Null(_oturum.MevcutKullanici);
    }

    [Fact]
    public async Task Ayni_nesille_es_zamanli_kosullu_acmalardan_yalnizca_biri_basarili_olur()
    {
        for (var tur = 0; tur < 20; tur++)
        {
            var oturum = new OturumBaglami();
            var nesil = oturum.Anlik.Nesil;
            using var baslat = new ManualResetEventSlim();
            var gorevler = Enumerable.Range(1, 16).Select(i => Task.Run(() =>
            {
                baslat.Wait();
                return oturum.OturumAc(new OturumKullanicisi(i, $"k{i}", KullaniciRolu.Operator), nesil);
            })).ToArray();
            baslat.Set();
            var sonuclar = await Task.WhenAll(gorevler);

            Assert.Equal(1, sonuclar.Count(x => x));
            var kazanan = Array.IndexOf(sonuclar, true) + 1;
            Assert.Equal(kazanan, oturum.MevcutKullanici!.KullaniciId);
        }
    }

    // ---- Giriş/çıkış yarışı ----

    private readonly BellekKullaniciRepository _depo = new();
    private readonly HizliParolaHasher _hasher = new();

    private KimlikService Servis() => new(_depo, Substitute.For<IUnitOfWork>(), _hasher, _oturum, TimeProvider.System);

    private void Ekle(string ad)
    {
        var ozet = _hasher.Hashle(Parola);
        _depo.AddAsync(new Kullanici(ad, ozet.Hash, ozet.Tuz, ozet.Iterasyon, KullaniciRolu.Admin)).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Giris_surerken_cikis_yapilirsa_oturum_acilmaz_giris_iptal_edilir()
    {
        Ekle("ali");
        var servis = Servis();
        _depo.KullaniciAdiOkununca = () =>
        {
            servis.CikisYap();
            return Task.CompletedTask;
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => servis.GirisYapAsync("ali", Parola));

        Assert.Null(_oturum.MevcutKullanici);
    }

    [Fact]
    public async Task Iptal_edilen_giris_basarili_giris_kaydi_yazmaz()
    {
        Ekle("ali");
        var isBirimi = Substitute.For<IUnitOfWork>();
        var servis = new KimlikService(_depo, isBirimi, _hasher, _oturum, TimeProvider.System);
        _depo.KullaniciAdiOkununca = () =>
        {
            servis.CikisYap();
            return Task.CompletedTask;
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => servis.GirisYapAsync("ali", Parola));

        Assert.Null(_depo.Kayitlar[0].SonGirisTarihi);
        await isBirimi.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Giris_kaydedilirken_cikis_yapilirsa_oturum_acilmaz()
    {
        Ekle("ali");
        var isBirimi = Substitute.For<IUnitOfWork>();
        var servis = new KimlikService(_depo, isBirimi, _hasher, _oturum, TimeProvider.System);
        isBirimi.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(_ =>
        {
            servis.CikisYap();
            return 1;
        });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => servis.GirisYapAsync("ali", Parola));

        Assert.Null(_oturum.MevcutKullanici);
    }

    [Fact]
    public async Task Giris_surerken_baska_oturum_acilirsa_o_oturum_ezilmez()
    {
        Ekle("ali");
        var servis = Servis();
        _depo.KullaniciAdiOkununca = () =>
        {
            _oturum.OturumAc(Veli);
            return Task.CompletedTask;
        };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => servis.GirisYapAsync("ali", Parola));

        Assert.Equal(Veli, _oturum.MevcutKullanici);
    }

    [Fact]
    public async Task Cikistan_sonra_baslayan_giris_normal_acilir()
    {
        Ekle("ali");
        var servis = Servis();
        _oturum.OturumAc(Veli);
        servis.CikisYap();

        await servis.GirisYapAsync("ali", Parola);

        Assert.Equal("ali", _oturum.MevcutKullanici?.KullaniciAdi);
    }
}
