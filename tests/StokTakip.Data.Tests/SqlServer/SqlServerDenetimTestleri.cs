using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StokTakip.Application.Denetim;
using StokTakip.Application.DisaAktarim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Kimlik;
using StokTakip.Data.Tests.Altyapi;
using StokTakip.Domain;
using StokTakip.Domain.Entities;

namespace StokTakip.Data.Tests.SqlServer;

/// <summary>Denetim kaydının zamanı, yalnızca ekleme koruması ve anahtar tabanlı CSV sayfalaması gerçek SQL Server'da.</summary>
public sealed class SqlServerDenetimTestleri(SqlServerVeritabani db) : IClassFixture<SqlServerVeritabani>, IAsyncLifetime
{
    public Task InitializeAsync() => db.Etkin ? db.SifirlaAsync() : Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;

    private static void Ayarla(DenetimKaydi kayit, string ozellik, object? deger) =>
        typeof(DenetimKaydi).GetProperty(ozellik)!.SetValue(kayit, deger);

    private async Task<long> KayitEkleAsync(string ozet = "Deneme")
    {
        using var kapsam = new SqlServisKapsami(db);
        await kapsam.Kaydedici.KaydetVeYazAsync(new DenetimOlayi(DenetimIslemi.UrunEklendi, DenetimVarlikTurleri.Urun, ozet, 1, "Kalem"));
        return (await db.DenetimKayitlariAsync()).Last().DenetimId;
    }

    /// <summary>Tek ifadeyle eklenen kayıtların Zaman'ı aynıdır (SYSUTCDATETIME ifade başına bir kez hesaplanır).</summary>
    private Task DenetimKayitlariEkleAsync(int adet) => db.KomutAsync($"""
        WITH n AS (SELECT TOP ({adet}) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS i FROM sys.all_objects a CROSS JOIN sys.all_objects b)
        INSERT INTO DenetimKayitlari (KullaniciAdi, Islem, VarlikTuru, VarlikAdi, Ozet)
        SELECT N'ali', CASE WHEN i % 2 = 0 THEN 4 ELSE 1 END, N'Ürün', N'Kalem', N'kayıt ' + CAST(i AS nvarchar(10)) FROM n ORDER BY i;
        """);

    private static List<string> CsvSatirlari(MemoryStream akis)
    {
        var metin = Encoding.UTF8.GetString(akis.ToArray());
        Assert.StartsWith("﻿", metin);
        return metin[1..].Split("\r\n", StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    [SqlServerFact]
    public async Task Zaman_veritabani_SYSUTCDATETIME_degeridir_istemci_saatinden_ve_yerel_saatten_bagimsizdir()
    {
        var istemci = new SabitZamanSaglayici(new DateTime(2001, 2, 3, 4, 5, 6));
        var once = await db.SkalerAsync<DateTime>("SELECT SYSUTCDATETIME()");
        using (var kapsam = new SqlServisKapsami(db, istemci))
        {
            var urun = await kapsam.StokService.UrunEkleAsync(new UrunEkleIstegi("Kalem", 10));
            await kapsam.SiparisService.SiparisOlusturAsync(new SiparisOlusturIstegi(urun.UrunId, 1));
        }

        var sonra = await db.SkalerAsync<DateTime>("SELECT SYSUTCDATETIME()");
        var sunucuYerel = await db.SkalerAsync<DateTime>("SELECT SYSDATETIME()");
        var kayitlar = await db.DenetimKayitlariAsync();

        Assert.Equal(2, kayitlar.Count);
        Assert.All(kayitlar, k => Assert.InRange(k.Zaman, once.AddMilliseconds(-1), sonra.AddMilliseconds(1)));
        if ((sunucuYerel - sonra).Duration() > TimeSpan.FromMinutes(1))
        {
            Assert.All(kayitlar, k => Assert.True((sunucuYerel - k.Zaman).Duration() > TimeSpan.FromMinutes(1), "Zaman yerel saatle yazılmış."));
        }

        Assert.Equal(2001, (await db.TumSiparislerAsync()).Single().Tarih.Year);
        using var okuma = new SqlServisKapsami(db);
        var dto = (await okuma.DenetimService.DenetimKayitlariniGetirAsync(new DenetimSorgusu())).Ogeler;
        Assert.All(dto, x => Assert.Equal(DateTimeKind.Utc, x.Zaman.Kind));
    }

    [SqlServerFact]
    public async Task Uygulamanin_verdigi_zaman_gonderilmez_sunucu_degeri_geri_okunur()
    {
        await using (var context = db.ContextOlustur())
        {
            var kayit = new DenetimKaydi(null, null, DenetimIslemi.CikisYapildi, "Oturum", null, null, null, null, null, "Çıkış");
            Ayarla(kayit, nameof(DenetimKaydi.Zaman), new DateTime(2001, 1, 1));
            context.DenetimKayitlari.Add(kayit);
            await context.SaveChangesAsync();
            Assert.NotEqual(2001, kayit.Zaman.Year);
        }

        var simdi = await db.SkalerAsync<DateTime>("SELECT SYSUTCDATETIME()");
        Assert.InRange(Assert.Single(await db.DenetimKayitlariAsync()).Zaman, simdi.AddMinutes(-1), simdi);
    }

    [SqlServerFact]
    public async Task Degistirilen_veya_silinen_kayit_SaveChanges_ile_reddedilir_veritabani_degismez()
    {
        var id = await KayitEkleAsync();

        await using (var context = db.ContextOlustur())
        {
            var kayit = await context.DenetimKayitlari.AsTracking().SingleAsync(x => x.DenetimId == id);
            Ayarla(kayit, nameof(DenetimKaydi.Ozet), "Değiştirildi");
            var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => new UnitOfWork(context).SaveChangesAsync());
            Assert.Equal(DenetimKaydiKorumasi.Mesaj, hata.Message);
        }

        await using (var context = db.ContextOlustur())
        {
            context.DenetimKayitlari.Remove(await context.DenetimKayitlari.AsTracking().SingleAsync(x => x.DenetimId == id));
            await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        }

        Assert.Equal("Deneme", Assert.Single(await db.DenetimKayitlariAsync()).Ozet);
    }

    /// <summary>
    /// Bilinen sınır (belgeleme): koruma SaveChanges üzerindedir; ExecuteUpdate/ExecuteDelete ve ham SQL veritabanında
    /// engellenmez (tetikleyici ya da yetki kısıtı yok). Bu test sınırın hâlâ var olduğunu kaydeder; kapatılırsa güncellenmeli.
    /// </summary>
    [SqlServerFact]
    public async Task Bilinen_sinir_ExecuteUpdate_ve_ExecuteDelete_denetim_kaydini_degistirip_silebilir()
    {
        var id = await KayitEkleAsync();
        await using var context = db.ContextOlustur();

        var guncellenen = await context.DenetimKayitlari.Where(x => x.DenetimId == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Ozet, "sonradan değiştirildi"));
        Assert.Equal(1, guncellenen);
        Assert.Equal("sonradan değiştirildi", Assert.Single(await db.DenetimKayitlariAsync()).Ozet);

        var silinen = await context.DenetimKayitlari.ExecuteDeleteAsync();
        Assert.Equal(1, silinen);
        Assert.Empty(await db.DenetimKayitlariAsync());
    }

    [SqlServerFact]
    public async Task Ayni_zamanli_kayitlarda_sayfalama_zaman_sonra_id_azalan_tekrar_ve_atlama_yok()
    {
        await DenetimKayitlariEkleAsync(401);
        Assert.Equal(1, Convert.ToInt32((await db.SatirlarAsync("SELECT COUNT(DISTINCT Zaman) FROM DenetimKayitlari"))[0][0]));

        using var kapsam = new SqlServisKapsami(db);
        var idler = new List<long>();
        for (var sayfa = 1; sayfa <= 3; sayfa++)
        {
            var sonuc = await kapsam.DenetimService.DenetimKayitlariniGetirAsync(new DenetimSorgusu(Sayfa: sayfa, SayfaBoyutu: 200));
            Assert.Equal(401, sonuc.ToplamKayit);
            idler.AddRange(sonuc.Ogeler.Select(x => x.DenetimId));
        }

        Assert.Equal(401, idler.Distinct().Count());
        Assert.Equal(idler.OrderByDescending(x => x), idler);
    }

    [SqlServerFact]
    public async Task Denetim_csv_uretim_bilesimiyle_anahtar_tabanli_sayfalar_tum_kayitlari_bir_kez_yazar()
    {
        var admin = await db.KullaniciEkleAsync("yonetici", KullaniciRolu.Admin);
        await DenetimKayitlariEkleAsync(401);
        await using var sp = db.UygulamaOlustur();
        sp.GetRequiredService<IOturumBaglami>().OturumAc(new OturumKullanicisi(admin.KullaniciId, admin.KullaniciAdi, admin.Rol));

        List<string> tumu, filtreli;
        await using (var scope = sp.CreateAsyncScope())
        {
            var servis = scope.ServiceProvider.GetRequiredService<IDenetimService>();
            await servis.YetkiKontrolEtAsync();
            var akis = new MemoryStream();
            var sayi = await servis.DenetimKayitlariniCsvAktarAsync(new DenetimSorgusu(Sayfa: 3, SayfaBoyutu: 5), akis);
            Assert.Equal(401, sayi);
            await servis.AktarimTamamlandiAsync(sayi);
            tumu = CsvSatirlari(akis);
        }

        await using (var scope = sp.CreateAsyncScope())
        {
            var akis = new MemoryStream();
            var sayi = await scope.ServiceProvider.GetRequiredService<IDenetimService>()
                .DenetimKayitlariniCsvAktarAsync(new DenetimSorgusu(Islem: DenetimIslemi.StokGuncellendi), akis);
            Assert.Equal(200, sayi);
            filtreli = CsvSatirlari(akis);
        }

        Assert.Equal(DenetimService.CsvBaslik, tumu[0]);
        var ozetler = tumu.Skip(1).Select(x => x.Split(';')[^1]).ToList();
        Assert.Equal(Enumerable.Range(1, 401).Reverse().Select(i => $"kayıt {i}"), ozetler);
        Assert.Equal(Enumerable.Range(1, 200).Reverse().Select(i => $"kayıt {i * 2}"), filtreli.Skip(1).Select(x => x.Split(';')[^1]));

        var aktarim = Assert.Single(await db.DenetimKayitlariAsync(), x => x.Islem == DenetimIslemi.CsvAktarildi);
        Assert.Equal(("yonetici", "Satır=401"), (aktarim.KullaniciAdi, aktarim.YeniDeger));
    }

    [SqlServerFact]
    public async Task Siparis_csv_uretim_bilesimiyle_anahtar_tabanli_sayfalar_silinmisleri_atlar()
    {
        var admin = await db.KullaniciEkleAsync("yonetici", KullaniciRolu.Admin);
        var urunId = await db.UrunEkleAsync("Kalem; mavi", 10);
        await db.KomutAsync($"""
            WITH n AS (SELECT TOP (401) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS i FROM sys.all_objects a CROSS JOIN sys.all_objects b)
            INSERT INTO Siparisler (UrunID, Adet, Tarih, IsDeleted)
            SELECT {urunId}, i, DATEADD(MINUTE, i, '2026-05-01T08:00:00'), CASE WHEN i % 100 = 0 THEN 1 ELSE 0 END FROM n ORDER BY i;
            """);
        await using var sp = db.UygulamaOlustur();
        sp.GetRequiredService<IOturumBaglami>().OturumAc(new OturumKullanicisi(admin.KullaniciId, admin.KullaniciAdi, admin.Rol));

        await using var scope = sp.CreateAsyncScope();
        var servis = scope.ServiceProvider.GetRequiredService<ISiparisDisaAktarimService>();
        await servis.YetkiKontrolEtAsync();
        var akis = new MemoryStream();
        var sayi = await servis.SiparisleriCsvAktarAsync(new SiparisGecmisiSorgusu(), akis);
        await servis.AktarimTamamlandiAsync(sayi);

        Assert.Equal(397, sayi);
        var satirlar = CsvSatirlari(akis);
        Assert.Equal("Sipariş No;Ürün No;Ürün Adı;Adet;Tarih", satirlar[0]);
        var adetler = satirlar.Skip(1).Select(x => int.Parse(x.Split(';')[^2])).ToList();
        Assert.Equal(Enumerable.Range(1, 401).Where(i => i % 100 != 0).Reverse(), adetler);
        Assert.All(satirlar.Skip(1), x => Assert.Contains(";\"Kalem; mavi\";", x));
    }
}
