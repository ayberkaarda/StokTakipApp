using Microsoft.Extensions.DependencyInjection;
using StokTakip.Application;
using StokTakip.Application.DisaAktarim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Kimlik;
using StokTakip.Application.Raporlama;
using StokTakip.Application.Services;
using StokTakip.Data.Repositories;
using StokTakip.Data.Tests.Altyapi;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Data.Tests;

public class YetkiUctanUcaTestleri
{
    private static ServiceProvider UretimServisleri()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddData("Server=(localdb)\\yok;Database=Yok;Integrated Security=true");
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    [Fact]
    public async Task Uretim_DI_dekoratorleri_kaydeder_ve_oturum_yokken_islemleri_veritabanina_gitmeden_reddeder()
    {
        await using var sp = UretimServisleri();
        await using var scope = sp.CreateAsyncScope();
        var stok = scope.ServiceProvider.GetRequiredService<IStokService>();
        var siparis = scope.ServiceProvider.GetRequiredService<ISiparisService>();
        var kategori = scope.ServiceProvider.GetRequiredService<IKategoriService>();

        Assert.IsType<YetkiDenetimliStokService>(stok);
        Assert.IsType<YetkiDenetimliSiparisService>(siparis);
        Assert.IsType<YetkiDenetimliKategoriService>(kategori);
        Assert.IsType<YetkiDenetimliRaporService>(scope.ServiceProvider.GetRequiredService<IRaporService>());
        Assert.IsType<YetkiDenetimliSiparisDisaAktarimService>(scope.ServiceProvider.GetRequiredService<ISiparisDisaAktarimService>());
        Assert.IsType<KimlikService>(scope.ServiceProvider.GetRequiredService<IKimlikService>());
        Assert.IsType<ParolaHasher>(sp.GetRequiredService<IParolaHasher>());

        await Assert.ThrowsAsync<YetkisizIslemException>(() => stok.UrunSilAsync(1));
        await Assert.ThrowsAsync<YetkisizIslemException>(() => stok.UrunleriListeleAsync());
        await Assert.ThrowsAsync<YetkisizIslemException>(() => siparis.SiparisSilAsync(1));
        await Assert.ThrowsAsync<YetkisizIslemException>(() => kategori.KategoriEkleAsync("Gıda"));
    }

    [Fact]
    public async Task Uretim_DI_oturum_baglami_tekil_operator_oturumu_admin_islemlerini_reddeder()
    {
        await using var sp = UretimServisleri();
        var oturum = sp.GetRequiredService<IOturumBaglami>();
        oturum.OturumAc(new OturumKullanicisi(7, "ali", KullaniciRolu.Operator));

        await using var scope = sp.CreateAsyncScope();
        Assert.Same(oturum, scope.ServiceProvider.GetRequiredService<IOturumBaglami>());
        var hata = await Assert.ThrowsAsync<YetkisizIslemException>(
            () => scope.ServiceProvider.GetRequiredService<IStokService>().UrunSilAsync(1));
        Assert.Equal("Bu işlem için yetkiniz yok.", hata.Message);
        await Assert.ThrowsAsync<YetkisizIslemException>(
            () => scope.ServiceProvider.GetRequiredService<ISiparisDisaAktarimService>().SiparisleriCsvAktarAsync(new(), Stream.Null));
    }

    [Fact]
    public async Task Operator_urun_ve_siparis_olusturur_silemez_Admin_siler()
    {
        using var db = new TestVeritabani();
        var oturum = new OturumBaglami();
        await using var kullaniciContext = db.ContextOlustur();
        kullaniciContext.Kullanicilar.Add(new Kullanici("yonetici", [1], [1], 1, KullaniciRolu.Admin));
        await kullaniciContext.SaveChangesAsync();
        kullaniciContext.Kullanicilar.Add(new Kullanici("ali", [1], [1], 1, KullaniciRolu.Operator));
        await kullaniciContext.SaveChangesAsync();
        var denetci = new OturumYetkiDenetcisi(oturum, new KullaniciRepository(kullaniciContext));

        using var kapsam = new KategoriliServisKapsami(db);
        using var siparisKapsami = new ServisKapsami(db);
        var stok = new YetkiDenetimliStokService(kapsam.StokService, denetci);
        var kategori = new YetkiDenetimliKategoriService(kapsam.KategoriService, denetci);
        var siparis = new YetkiDenetimliSiparisService(siparisKapsami.SiparisService, denetci);

        await Assert.ThrowsAsync<YetkisizIslemException>(() => stok.UrunEkleAsync(new UrunEkleIstegi("Kalem", 10)));

        oturum.OturumAc(new OturumKullanicisi(2, "ali", KullaniciRolu.Operator));
        var urun = await stok.UrunEkleAsync(new UrunEkleIstegi("Kalem", 10));
        await stok.StokGuncelleAsync(urun.UrunId, 20);
        var s = await siparis.SiparisOlusturAsync(new SiparisOlusturIstegi(urun.UrunId, 3));
        Assert.Single(await siparis.SiparisGecmisiniGetirAsync());

        await Assert.ThrowsAsync<YetkisizIslemException>(() => siparis.SiparisSilAsync(s.SiparisId));
        await Assert.ThrowsAsync<YetkisizIslemException>(() => stok.UrunSilAsync(urun.UrunId));
        await Assert.ThrowsAsync<YetkisizIslemException>(() => kategori.KategoriEkleAsync("Kırtasiye"));
        Assert.Single(await siparis.SiparisGecmisiniGetirAsync());
        Assert.Empty(await kategori.KategorileriListeleAsync());

        oturum.OturumAc(new OturumKullanicisi(1, "yonetici", KullaniciRolu.Admin));
        await siparis.SiparisSilAsync(s.SiparisId);
        await kategori.KategoriEkleAsync("Kırtasiye");
        Assert.Empty(await siparis.SiparisGecmisiniGetirAsync());
        Assert.Single(await kategori.KategorileriListeleAsync());
    }

    [Fact]
    public async Task Dekoratorsuz_dogrudan_kurulan_servisler_oturumsuz_calismaya_devam_eder()
    {
        using var db = new TestVeritabani();
        using var kapsam = new ServisKapsami(db);

        var urun = await kapsam.StokService.UrunEkleAsync(new UrunEkleIstegi("Kalem", 1));
        await kapsam.StokService.UrunSilAsync(urun.UrunId);

        Assert.Empty(await kapsam.StokService.UrunleriListeleAsync());
    }
}
