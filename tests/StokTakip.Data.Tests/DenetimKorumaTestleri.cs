using Microsoft.EntityFrameworkCore;
using StokTakip.Application.Denetim;
using StokTakip.Application.Dtos;
using StokTakip.Data.Tests.Altyapi;
using StokTakip.Domain;
using StokTakip.Domain.Entities;

namespace StokTakip.Data.Tests;

/// <summary>Denetim kayıtları yalnızca eklenir; zaman veritabanından gelir.</summary>
public class DenetimKorumaTestleri
{
    private readonly DenetimTestVeritabani _db = new();

    private async Task<long> KayitEkleAsync(string ozet = "Deneme")
    {
        using var kapsam = new DenetimKapsami(_db);
        await kapsam.Kaydedici.KaydetVeYazAsync(new DenetimOlayi(DenetimIslemi.UrunEklendi, "Ürün", ozet, 1, "Kalem"));
        return (await _db.KayitlarAsync()).Last().DenetimId;
    }

    private static void Ayarla(DenetimKaydi kayit, string ozellik, object? deger) =>
        typeof(DenetimKaydi).GetProperty(ozellik)!.SetValue(kayit, deger);

    [Fact]
    public async Task Degistirilen_kayit_SaveChanges_ile_reddedilir_ve_veritabani_degismez()
    {
        var id = await KayitEkleAsync();

        await using (var context = _db.ContextOlustur())
        {
            var kayit = await context.DenetimKayitlari.SingleAsync(x => x.DenetimId == id);
            Ayarla(kayit, nameof(DenetimKaydi.Ozet), "Değiştirildi");
            Ayarla(kayit, nameof(DenetimKaydi.Fark), 999);

            var hata = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
            Assert.Equal(DenetimKaydiKorumasi.Mesaj, hata.Message);
            Assert.Throws<InvalidOperationException>(() => context.SaveChanges());
        }

        var guncel = Assert.Single(await _db.KayitlarAsync());
        Assert.Equal("Deneme", guncel.Ozet);
        Assert.Null(guncel.Fark);
    }

    [Fact]
    public async Task Silinen_kayit_reddedilir_UnitOfWork_uzerinden_de()
    {
        var id = await KayitEkleAsync();

        await using (var context = _db.ContextOlustur())
        {
            context.DenetimKayitlari.Remove(await context.DenetimKayitlari.SingleAsync(x => x.DenetimId == id));
            await Assert.ThrowsAsync<InvalidOperationException>(() => new UnitOfWork(context).SaveChangesAsync());
        }

        await using (var context = _db.ContextOlustur())
        {
            context.Entry(await context.DenetimKayitlari.SingleAsync()).State = EntityState.Modified;
            await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
        }

        Assert.Single(await _db.KayitlarAsync());
    }

    [Fact]
    public async Task Koruma_eklemeyi_ve_ayni_kapsamdaki_diger_degisiklikleri_engellemez()
    {
        await KayitEkleAsync("Birinci");
        using var kapsam = new DenetimKapsami(_db);
        var urun = await kapsam.StokService.UrunEkleAsync(new UrunEkleIstegi("Kalem", 1));
        await kapsam.StokService.StokGuncelleAsync(urun.UrunId, 4);

        Assert.Equal(3, (await _db.KayitlarAsync()).Count);
    }

    [Fact]
    public async Task Zaman_istemci_saatinden_bagimsiz_veritabani_utc_saatidir()
    {
        var istemciSaati = new SabitZamanSaglayici(new DateTime(2001, 2, 3, 4, 5, 6));
        var once = DateTime.UtcNow.AddSeconds(-5);
        using (var kapsam = new DenetimKapsami(_db, zaman: istemciSaati))
        {
            var urun = await kapsam.StokService.UrunEkleAsync(new UrunEkleIstegi("Kalem", 10));
            await kapsam.SiparisService.SiparisOlusturAsync(new SiparisOlusturIstegi(urun.UrunId, 1));
            istemciSaati.Simdi = new DateTime(2099, 1, 1);
            await kapsam.StokService.StokGuncelleAsync(urun.UrunId, 3);
        }

        var sonra = DateTime.UtcNow.AddSeconds(5);
        var kayitlar = await _db.KayitlarAsync();
        Assert.Equal(3, kayitlar.Count);
        Assert.All(kayitlar, k => Assert.InRange(k.Zaman, once, sonra));
        await using var context = _db.ContextOlustur();
        Assert.Equal(2001, (await context.Siparisler.SingleAsync()).Tarih.Year);
    }

    [Fact]
    public async Task Uygulamanin_verdigi_zaman_degeri_veritabanina_gonderilmez()
    {
        await using (var context = _db.ContextOlustur())
        {
            var kayit = new DenetimKaydi(null, null, DenetimIslemi.CikisYapildi, "Oturum", null, null, null, null, null, "Çıkış");
            Ayarla(kayit, nameof(DenetimKaydi.Zaman), new DateTime(2001, 1, 1));
            context.DenetimKayitlari.Add(kayit);
            await context.SaveChangesAsync();
            Assert.NotEqual(2001, kayit.Zaman.Year);
        }

        Assert.InRange((await _db.KayitlarAsync()).Single().Zaman, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    [Fact]
    public async Task Kaydet_SaveChanges_cagirmaz_KaydetVeYaz_hemen_yazar()
    {
        using var kapsam = new DenetimKapsami(_db);
        kapsam.Kaydedici.Kaydet(new DenetimOlayi(DenetimIslemi.ParolaDegisti, "Kullanıcı", "Bekleyen"));
        Assert.Empty(await _db.KayitlarAsync());

        await kapsam.Kaydedici.KaydetVeYazAsync(new DenetimOlayi(DenetimIslemi.ParolaDegisti, "Kullanıcı", "Yazılan"));
        Assert.Equal(new[] { "Bekleyen", "Yazılan" }, (await _db.KayitlarAsync()).Select(x => x.Ozet));
    }
}
