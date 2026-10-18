using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StokTakip.Application;
using StokTakip.Application.Denetim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Kimlik;
using StokTakip.Application.Services;
using StokTakip.Data.Tests.Altyapi;
using StokTakip.Domain;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Data.Tests;

public class DenetimYetkiTestleri
{
    private readonly DenetimTestVeritabani _db = new();

    [Fact]
    public async Task Operator_denetim_kayitlarini_sorgulayamaz_ve_csv_alamaz_Admin_alir()
    {
        var admin = await _db.KullaniciEkleAsync("yonetici", KullaniciRolu.Admin);
        var operatorKullanici = await _db.KullaniciEkleAsync("ali", KullaniciRolu.Operator);
        using var kapsam = new DenetimKapsami(_db);
        var servis = new YetkiDenetimliDenetimService(kapsam.DenetimService, kapsam.Denetci);

        var oturumsuz = await Assert.ThrowsAsync<YetkisizIslemException>(() => servis.DenetimKayitlariniGetirAsync(new DenetimSorgusu()));
        Assert.Equal(YetkisizIslemException.OturumGerekliMesaji, oturumsuz.Message);

        kapsam.OturumAc(operatorKullanici);
        await Assert.ThrowsAsync<YetkisizIslemException>(() => servis.DenetimKayitlariniGetirAsync(new DenetimSorgusu()));
        await Assert.ThrowsAsync<YetkisizIslemException>(() => servis.YetkiKontrolEtAsync());
        var akis = new MemoryStream();
        await Assert.ThrowsAsync<YetkisizIslemException>(() => servis.DenetimKayitlariniCsvAktarAsync(new DenetimSorgusu(), akis));
        Assert.Equal(0, akis.Length);
        Assert.Empty(await _db.KayitlarAsync());

        kapsam.OturumAc(admin);
        await servis.YetkiKontrolEtAsync();
        await servis.DenetimKayitlariniCsvAktarAsync(new DenetimSorgusu(), akis);
        var sonuc = await servis.DenetimKayitlariniGetirAsync(new DenetimSorgusu());
        Assert.Equal(DenetimIslemi.CsvAktarildi, Assert.Single(sonuc.Ogeler).Islem);
    }

    [Fact]
    public async Task Admin_yetkisi_her_cagrida_veritabanindan_dogrulanir()
    {
        var admin = await _db.KullaniciEkleAsync("yonetici", KullaniciRolu.Admin);
        using var kapsam = new DenetimKapsami(_db);
        var servis = new YetkiDenetimliDenetimService(kapsam.DenetimService, kapsam.Denetci);
        kapsam.OturumAc(admin);
        await servis.DenetimKayitlariniGetirAsync(new DenetimSorgusu());

        await using (var context = _db.ContextOlustur())
        {
            await context.Database.ExecuteSqlRawAsync("UPDATE Kullanicilar SET Rol = 0");
        }

        await Assert.ThrowsAsync<YetkisizIslemException>(() => servis.DenetimKayitlariniGetirAsync(new DenetimSorgusu()));
        Assert.False(kapsam.Oturum.OturumAcikMi);
    }

    [Fact]
    public async Task Operator_esik_degistiremez_yeni_urunde_varsayilan_kullanilir_Admin_degistirir()
    {
        var admin = await _db.KullaniciEkleAsync("yonetici", KullaniciRolu.Admin);
        var operatorKullanici = await _db.KullaniciEkleAsync("ali", KullaniciRolu.Operator);
        using var kapsam = new DenetimKapsami(_db);
        var stok = new YetkiDenetimliStokService(kapsam.StokService, kapsam.Denetci);

        kapsam.OturumAc(operatorKullanici);
        var urun = await stok.UrunEkleAsync(new UrunEkleIstegi("Kalem", 5, DusukStokEsigi: 0));
        Assert.Equal(10, urun.DusukStokEsigi);
        Assert.Equal(10, (await _db.UrunAsync(urun.UrunId)).DusukStokEsigi);

        var hata = await Assert.ThrowsAsync<YetkisizIslemException>(
            () => stok.UrunGuncelleAsync(new UrunGuncelleIstegi(urun.UrunId, "Kalem X", 1, DusukStokEsigi: 0)));
        Assert.Equal(YetkiDenetimliStokService.EsikYetkisiMesaji, hata.Message);
        Assert.True(kapsam.Oturum.OturumAcikMi);
        var degismemis = await _db.UrunAsync(urun.UrunId);
        Assert.Equal(("Kalem", 5, 10), (degismemis.UrunAdi, degismemis.Stok, degismemis.DusukStokEsigi));

        await stok.UrunGuncelleAsync(new UrunGuncelleIstegi(urun.UrunId, "Kalem X", 4, DusukStokEsigi: 10));
        Assert.Equal(("Kalem X", 4), ((await _db.UrunAsync(urun.UrunId)).UrunAdi, (await _db.UrunAsync(urun.UrunId)).Stok));

        kapsam.OturumAc(admin);
        await stok.UrunGuncelleAsync(new UrunGuncelleIstegi(urun.UrunId, "Kalem X", 4, DusukStokEsigi: 0));
        var admininUrunu = await stok.UrunEkleAsync(new UrunEkleIstegi("Defter", 5, DusukStokEsigi: 2));
        Assert.Equal(0, (await _db.UrunAsync(urun.UrunId)).DusukStokEsigi);
        Assert.Equal(2, admininUrunu.DusukStokEsigi);

        var esik = Assert.Single(await _db.KayitlarAsync(), x => x.Islem == DenetimIslemi.EsikDegisti);
        Assert.Equal(("yonetici", "10", "0"), (esik.KullaniciAdi, esik.EskiDeger, esik.YeniDeger));
    }

    [Fact]
    public async Task Uretim_DI_gercek_denetim_kaydedicisini_ve_admin_dekoratorunu_baglar()
    {
        using var baglanti = new SqliteConnection("DataSource=:memory:");
        baglanti.Open();
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddData("Server=(localdb)\\yok;Database=Yok;Integrated Security=true");
        services.RemoveAll<IDbContextOptionsConfiguration<StokTakipDbContext>>();
        services.AddDbContext<StokTakipDbContext>(o => o
            .UseSqlite(baglanti)
            .ReplaceService<IModelCustomizer, KimlikSqliteModelCustomizer>()
            .AddInterceptors(new RowVersionInterceptor()));
        await using var sp = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        await using (var scope = sp.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<StokTakipDbContext>().Database.EnsureCreatedAsync();
            Assert.IsType<DenetimKaydedici>(scope.ServiceProvider.GetRequiredService<IDenetimKaydedici>());
            Assert.IsType<YetkiDenetimliDenetimService>(scope.ServiceProvider.GetRequiredService<IDenetimService>());
            await scope.ServiceProvider.GetRequiredService<IKimlikService>().IlkAdminOlusturAsync("yonetici", "Ilk-Parola-001");
        }

        await using (var scope = sp.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<IKimlikService>().GirisYapAsync("yonetici", "Ilk-Parola-001");
        }

        await using (var scope = sp.CreateAsyncScope())
        {
            var stok = scope.ServiceProvider.GetRequiredService<IStokService>();
            var urun = await stok.UrunEkleAsync(new UrunEkleIstegi("Kalem", 9));
            await stok.StokGuncelleAsync(urun.UrunId, 4);
            await scope.ServiceProvider.GetRequiredService<IKategoriService>().KategoriEkleAsync("Ofis");
            await scope.ServiceProvider.GetRequiredService<ISiparisService>().SiparisOlusturAsync(new SiparisOlusturIstegi(urun.UrunId, 1));
        }

        await using (var scope = sp.CreateAsyncScope())
        {
            var kayitlar = await scope.ServiceProvider.GetRequiredService<IDenetimService>().DenetimKayitlariniGetirAsync(new DenetimSorgusu());
            Assert.Equal(
                new[]
                {
                    DenetimIslemi.SiparisOlusturuldu, DenetimIslemi.KategoriEklendi, DenetimIslemi.StokGuncellendi,
                    DenetimIslemi.UrunEklendi, DenetimIslemi.GirisBasarili, DenetimIslemi.IlkAdminOlusturuldu,
                },
                kayitlar.Ogeler.Select(x => x.Islem));
            Assert.All(kayitlar.Ogeler.Take(5), x => Assert.Equal("yonetici", x.KullaniciAdi));
            await scope.ServiceProvider.GetRequiredService<IKimlikService>().CikisYapAsync();
            await Assert.ThrowsAsync<YetkisizIslemException>(
                () => scope.ServiceProvider.GetRequiredService<IDenetimService>().DenetimKayitlariniGetirAsync(new DenetimSorgusu()));
        }
    }
}
