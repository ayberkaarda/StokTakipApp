using StokTakip.Data.Tests.Altyapi;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Data.Tests;

/// <summary>
/// İki ayrı kapsam (ayrı DbContext + servisler) ile iyimser eşzamanlılık senaryoları.
///
/// SQLite / SQL Server farkı (dürüst not): TestVeritabani tüm context'leri tek bir bellek içi
/// SQLite bağlantısı üzerinde açar; SQLite tek yazıcılıdır ve Microsoft.Data.Sqlite ReadCommitted'i
/// Serializable'a yükseltir. Bu yüzden iki transaction'ın aynı anda açık olup satır kilitleri için
/// yarıştığı SQL Server davranışı burada üretilemez. Yarış, gerçekte olduğu gibi "ikinci kullanıcı
/// ürünü ilk işlem commit etmeden önce okudu" şeklinde kurulur: ikinci kapsam ürünü önceden okuyup
/// izler (eski Stok + eski RowVersion), ilk kapsam commit eder, ikinci kapsamın UPDATE'i
/// WHERE RowVersion = @eski koşulunu karşılamaz. Korunan şey RowVersion denetimidir, kilitleme değil.
/// </summary>
public class EszamanlilikTestleri
{
    [Fact]
    public async Task Son_stok_icin_iki_kapsam_yarisirsa_ilk_commit_kazanir_ikincisi_EszamanlilikException_alir()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Son Kalem", 1);

        using var kapsamA = new ServisKapsami(db);
        using var kapsamB = new ServisKapsami(db);

        // B, A commit etmeden önce ürünü okur: ekranda Stok = 1 görüyor.
        var urunB = await kapsamB.UrunRepository.GetByIdAsync(urunId);
        Assert.NotNull(urunB);
        Assert.Equal(1, urunB.Stok);
        var beklenenRowVersion = urunB.RowVersion!.ToArray();

        // A son stoğu satın alır ve commit eder.
        await kapsamA.SiparisService.SiparisOlusturAsync(new(urunId, 1));

        // B, gördüğü sürümle siparişi dener.
        kapsamB.UrunRepository.SetBeklenenRowVersion(urunB, beklenenRowVersion);
        var ex = await Assert.ThrowsAnyAsync<DomainException>(
            () => kapsamB.SiparisService.SiparisOlusturAsync(new(urunId, 1)));
        Assert.IsType<EszamanlilikException>(ex);

        var urun = await Okuma.UrunAsync(db, urunId);
        Assert.Equal(0, urun.Stok);
        Assert.Single(await Okuma.TumSiparislerAsync(db));
    }

    [Fact]
    public async Task Son_stok_alindiktan_sonra_guncel_okuyan_kapsam_YetersizStok_alir_ve_stok_negatife_dusmez()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Son Kalem", 1);

        using (var kapsamA = new ServisKapsami(db))
        {
            await kapsamA.SiparisService.SiparisOlusturAsync(new(urunId, 1));
        }

        using (var kapsamB = new ServisKapsami(db))
        {
            await Assert.ThrowsAsync<YetersizStokException>(
                () => kapsamB.SiparisService.SiparisOlusturAsync(new(urunId, 1)));
        }

        Assert.Equal(0, (await Okuma.UrunAsync(db, urunId)).Stok);
        Assert.Single(await Okuma.TumSiparislerAsync(db));
    }

    [Fact]
    public async Task Ayni_siparis_iki_kapsamdan_silinirse_ikincisi_hata_alir_ve_stok_bir_kez_iade_edilir()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 10);
        int siparisId;
        using (var kapsam = new ServisKapsami(db))
        {
            siparisId = (await kapsam.SiparisService.SiparisOlusturAsync(new(urunId, 4))).SiparisId;
        }

        using var kapsamA = new ServisKapsami(db);
        using var kapsamB = new ServisKapsami(db);

        // B, A silmeden önce siparişi (ve ürününü) okumuş: IsDeleted = false, Stok = 6.
        var siparisB = await kapsamB.SiparisRepository.GetByIdAsync(siparisId);
        Assert.False(siparisB!.IsDeleted);

        await kapsamA.SiparisService.SiparisSilAsync(siparisId);

        var ex = await Assert.ThrowsAnyAsync<DomainException>(
            () => kapsamB.SiparisService.SiparisSilAsync(siparisId));
        Assert.True(
            ex is EszamanlilikException or GecersizIslemException,
            $"Beklenmeyen hata türü: {ex.GetType().Name}");

        Assert.Equal(10, (await Okuma.UrunAsync(db, urunId)).Stok);
        Assert.True(Assert.Single(await Okuma.TumSiparislerAsync(db)).IsDeleted);
    }

    [Fact]
    public async Task Ayni_siparis_sirayla_iki_kapsamdan_silinirse_ikincisi_GecersizIslem_alir()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 10);
        int siparisId;
        using (var kapsam = new ServisKapsami(db))
        {
            siparisId = (await kapsam.SiparisService.SiparisOlusturAsync(new(urunId, 4))).SiparisId;
        }

        using (var kapsamA = new ServisKapsami(db))
        {
            await kapsamA.SiparisService.SiparisSilAsync(siparisId);
        }

        using (var kapsamB = new ServisKapsami(db))
        {
            await Assert.ThrowsAsync<GecersizIslemException>(() => kapsamB.SiparisService.SiparisSilAsync(siparisId));
        }

        Assert.Equal(10, (await Okuma.UrunAsync(db, urunId)).Stok);
    }

    [Fact]
    public async Task Eski_RowVersion_ile_stok_guncellemesi_EszamanlilikException_verir_ve_veri_ezilmez()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 10);
        var eskiRowVersion = (await Okuma.UrunAsync(db, urunId)).RowVersion!;

        using (var kapsamA = new ServisKapsami(db))
        {
            await kapsamA.StokService.StokGuncelleAsync(urunId, 25, beklenenRowVersion: eskiRowVersion);
        }

        using (var kapsamB = new ServisKapsami(db))
        {
            await Assert.ThrowsAsync<EszamanlilikException>(
                () => kapsamB.StokService.StokGuncelleAsync(urunId, 3, beklenenRowVersion: eskiRowVersion));
            await Assert.ThrowsAsync<EszamanlilikException>(
                () => kapsamB.StokService.UrunGuncelleAsync(new(urunId, "Yeni Ad", 3), beklenenRowVersion: eskiRowVersion));
        }

        var urun = await Okuma.UrunAsync(db, urunId);
        Assert.Equal(25, urun.Stok);
        Assert.Equal("Kalem", urun.UrunAdi);
    }

    [Fact]
    public async Task Eski_RowVersion_ile_urun_silme_EszamanlilikException_verir_ve_urun_kalir()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 10);
        var eskiRowVersion = (await Okuma.UrunAsync(db, urunId)).RowVersion!;

        using (var kapsamA = new ServisKapsami(db))
        {
            await kapsamA.StokService.StokGuncelleAsync(urunId, 11);
        }

        using (var kapsamB = new ServisKapsami(db))
        {
            await Assert.ThrowsAsync<EszamanlilikException>(
                () => kapsamB.StokService.UrunSilAsync(urunId, beklenenRowVersion: eskiRowVersion));
        }

        Assert.Equal(11, (await Okuma.UrunAsync(db, urunId)).Stok);
    }

    [Fact]
    public async Task Eszamanlilik_hatasindan_sonra_ayni_kapsamdaki_yeni_islem_basarisiz_siparisi_tekrar_kaydetmez()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Son Kalem", 1);

        using var kapsamA = new ServisKapsami(db);
        using var kapsamB = new ServisKapsami(db);
        await kapsamB.UrunRepository.GetByIdAsync(urunId);
        await kapsamA.SiparisService.SiparisOlusturAsync(new(urunId, 1));
        await Assert.ThrowsAsync<EszamanlilikException>(
            () => kapsamB.SiparisService.SiparisOlusturAsync(new(urunId, 1)));

        // Rollback ChangeTracker'ı temizlemeseydi buradaki SaveChanges eski (eklenmiş sipariş +
        // düşülmüş stok) değişiklikleri yeniden gönderir ve yine çakışma hatası verirdi.
        var yeni = await kapsamB.StokService.UrunEkleAsync(new("Defter", 5));

        Assert.True(yeni.UrunId > 0);
        Assert.Equal(0, (await Okuma.UrunAsync(db, urunId)).Stok);
        Assert.Single(await Okuma.TumSiparislerAsync(db));

        // B artık güncel değeri görür.
        Assert.Equal(0, (await kapsamB.UrunRepository.GetByIdAsync(urunId))!.Stok);
    }
}
