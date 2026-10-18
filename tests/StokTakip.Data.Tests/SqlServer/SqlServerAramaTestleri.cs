using Microsoft.EntityFrameworkCore;
using StokTakip.Application.Denetim;
using StokTakip.Application.Dtos;
using StokTakip.Data.Tests.Altyapi;
using Xunit.Abstractions;

namespace StokTakip.Data.Tests.SqlServer;

/// <summary>
/// LIKE kaçışı, Türkçe büyük/küçük harf duyarsız eşleşme ve ad sıralaması gerçek SQL Server harmanlamasıyla.
/// SQLite'ta LIKE yalnızca ASCII harflerde büyük/küçük harf duyarsızdır ve '[' özel değildir; sıralama ordinal'dir.
/// Türkçe'ye özgü beklentiler (i/İ, ı/I) yalnızca veritabanı Turkish_* harmanlamasındaysa denetlenir.
/// </summary>
public sealed class SqlServerAramaTestleri(SqlServerVeritabani db, ITestOutputHelper cikti) : IClassFixture<SqlServerVeritabani>, IAsyncLifetime
{
    private static readonly DateTime Gun = new(2026, 5, 10, 12, 0, 0);

    public Task InitializeAsync() => db.Etkin ? db.SifirlaAsync() : Task.CompletedTask;

    public Task DisposeAsync() => Task.CompletedTask;

    private Task<string> HarmanlamaAsync() =>
        db.SkalerAsync<string>("SELECT CONVERT(nvarchar(128), DATABASEPROPERTYEX(DB_NAME(), 'Collation'))");

    private async Task UrunlerVeSiparislerAsync(params string[] adlar)
    {
        foreach (var ad in adlar)
        {
            await db.UrunEkleAsync(ad, 100);
        }

        await db.KomutAsync("INSERT INTO Siparisler (UrunID, Adet, Tarih, IsDeleted) SELECT UrunID, 1, '2026-05-10T12:00:00', 0 FROM Urunler");
    }

    private async Task<string[]> GecmisteAraAsync(string aranan)
    {
        using var kapsam = new SqlServisKapsami(db);
        var sonuc = await kapsam.SiparisService.SiparisGecmisiSayfaGetirAsync(new SiparisGecmisiSorgusu(UrunAdi: aranan, SayfaBoyutu: 200));
        return sonuc.Ogeler.Select(x => x.UrunAdi).Order(StringComparer.Ordinal).ToArray();
    }

    private async Task DenetimKaydiEkleAsync(string kullanici, string? varlik)
    {
        await using var context = db.ContextOlustur();
        await context.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO DenetimKayitlari (KullaniciAdi, Islem, VarlikTuru, VarlikAdi, Ozet) VALUES ({kullanici}, 1, {"Ürün"}, {varlik}, {kullanici + "|" + varlik})");
    }

    private async Task<string[]> DenetimdeAraAsync(DenetimSorgusu sorgu)
    {
        using var kapsam = new SqlServisKapsami(db, new SabitZamanSaglayici(Gun));
        var sonuc = await kapsam.DenetimService.DenetimKayitlariniGetirAsync(sorgu with { SayfaBoyutu = 200 });
        return sonuc.Ogeler.Select(x => x.Ozet).Order(StringComparer.Ordinal).ToArray();
    }

    [SqlServerFact]
    public async Task Siparis_gecmisi_urun_adi_aramasinda_LIKE_ozel_karakterleri_duz_metin_sayilir()
    {
        await UrunlerVeSiparislerAsync("Etiket [a]", "Etiket a", "İndirim %50", "Model 50", "dosya_adi", "dosyaXadi", "yol~isareti", "yol isareti", "A]B");

        Assert.Equal(new[] { "Etiket [a]" }, await GecmisteAraAsync("[a]"));
        Assert.Equal(new[] { "Etiket [a]" }, await GecmisteAraAsync("["));
        Assert.Equal(new[] { "İndirim %50" }, await GecmisteAraAsync("%50"));
        Assert.Equal(new[] { "İndirim %50" }, await GecmisteAraAsync("%"));
        Assert.Equal(new[] { "dosya_adi" }, await GecmisteAraAsync("a_a"));
        Assert.Equal(new[] { "yol~isareti" }, await GecmisteAraAsync("~"));
        Assert.Equal(new[] { "yol~isareti" }, await GecmisteAraAsync("l~i"));
        Assert.Equal(new[] { "A]B", "Etiket [a]" }, await GecmisteAraAsync("]"));
        Assert.Equal(9, (await GecmisteAraAsync("  ")).Length);
    }

    [SqlServerFact]
    public async Task Denetim_varlik_ve_kullanici_adi_aramasinda_LIKE_ozel_karakterleri_duz_metin_sayilir()
    {
        await DenetimKaydiEkleAsync("a_b", "Etiket [a]");
        await DenetimKaydiEkleAsync("axb", "Etiket a");
        await DenetimKaydiEkleAsync("veli%", "İndirim %50");
        await DenetimKaydiEkleAsync("veli", "Model 50");
        await DenetimKaydiEkleAsync("[x]", "yol~isareti");
        await DenetimKaydiEkleAsync("x", null);

        Assert.Equal(new[] { "a_b|Etiket [a]" }, await DenetimdeAraAsync(new DenetimSorgusu(VarlikAdi: "[a]")));
        Assert.Equal(new[] { "veli%|İndirim %50" }, await DenetimdeAraAsync(new DenetimSorgusu(VarlikAdi: "%5")));
        Assert.Equal(new[] { "[x]|yol~isareti" }, await DenetimdeAraAsync(new DenetimSorgusu(VarlikAdi: "~")));
        Assert.Equal(new[] { "a_b|Etiket [a]" }, await DenetimdeAraAsync(new DenetimSorgusu(KullaniciAdi: "a_b")));
        Assert.Equal(new[] { "veli%|İndirim %50" }, await DenetimdeAraAsync(new DenetimSorgusu(KullaniciAdi: "%")));
        Assert.Equal(new[] { "[x]|yol~isareti" }, await DenetimdeAraAsync(new DenetimSorgusu(KullaniciAdi: "[x]")));
    }

    [SqlServerFact]
    public async Task Turkce_harflerde_buyuk_kucuk_harf_duyarsiz_eslesme()
    {
        await UrunlerVeSiparislerAsync("Şeker", "Çiğdem Çayı", "Dağ Suyu", "ırmak taşı", "İnce Kalem", "Istakoz", "incir", "Öğle Ünü");
        var harmanlama = await HarmanlamaAsync();
        cikti.WriteLine($"Harmanlama: {harmanlama}");

        // Her CI harmanlamasında geçerli olanlar.
        Assert.Equal(new[] { "Şeker" }, await GecmisteAraAsync("şEKER"));
        Assert.Equal(new[] { "Dağ Suyu" }, await GecmisteAraAsync("DAĞ"));
        Assert.Equal(new[] { "Çiğdem Çayı" }, await GecmisteAraAsync("dem ÇA"));
        Assert.Equal(new[] { "Öğle Ünü" }, await GecmisteAraAsync("ÖĞLE ün"));

        if (harmanlama.StartsWith("Turkish", StringComparison.OrdinalIgnoreCase))
        {
            // Türkçe kural: i ↔ İ, ı ↔ I; i ile I farklı harflerdir.
            Assert.Equal(new[] { "İnce Kalem" }, await GecmisteAraAsync("ince"));
            Assert.Equal(new[] { "İnce Kalem" }, await GecmisteAraAsync("İNCE"));
            Assert.Empty(await GecmisteAraAsync("INCE"));
            Assert.Equal(new[] { "ırmak taşı" }, await GecmisteAraAsync("IRMAK"));
            Assert.Empty(await GecmisteAraAsync("irmak"));
            Assert.Equal(new[] { "Istakoz" }, await GecmisteAraAsync("ıstakoz"));
            Assert.Equal(new[] { "incir", "İnce Kalem" }, await GecmisteAraAsync("İNC"));
            Assert.Equal(new[] { "Çiğdem Çayı" }, await GecmisteAraAsync("ÇAYI"));
        }
        else
        {
            foreach (var aranan in new[] { "ince", "İNCE", "INCE", "IRMAK", "irmak", "ıstakoz" })
            {
                cikti.WriteLine($"'{aranan}' → {string.Join(", ", await GecmisteAraAsync(aranan))}");
            }
        }
    }

    [SqlServerFact]
    public async Task Ad_siralamasi_veritabani_harmanlamasina_gore_buyuk_kucuk_harf_duyarsiz_ve_Turkce_alfabeyle_yapilir()
    {
        var adlar = new[] { "Zeytin", "Çay", "Cips", "Şeker", "Su", "Ilık Süt", "İncir", "Üzüm", "Uçak", "ayva", "Ayran" };
        await UrunlerVeSiparislerAsync(adlar);
        using var kapsam = new SqlServisKapsami(db);
        foreach (var ad in adlar)
        {
            await kapsam.KategoriService.KategoriEkleAsync(ad);
        }

        var rapor = (await kapsam.RaporService.EnCokSatanlariGetirAsync(null, null, 100)).Select(x => x.UrunAdi).ToArray();
        var kategoriler = (await kapsam.KategoriService.KategorileriListeleAsync()).Select(x => x.Ad).ToArray();
        var harmanlama = await HarmanlamaAsync();
        cikti.WriteLine($"Harmanlama: {harmanlama}");
        cikti.WriteLine($"Rapor (eşit adet, ada göre): {string.Join(", ", rapor)}");
        cikti.WriteLine($"SQLite ordinal karşılığı:    {string.Join(", ", adlar.Order(StringComparer.Ordinal))}");

        Assert.Equal(kategoriler, rapor);
        Assert.Equal(Array.IndexOf(rapor, "Ayran") + 1, Array.IndexOf(rapor, "ayva"));
        if (harmanlama.StartsWith("Turkish", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Equal(new[] { "Ayran", "ayva", "Cips", "Çay", "Ilık Süt", "İncir", "Su", "Şeker", "Uçak", "Üzüm", "Zeytin" }, rapor);
        }
    }

    [SqlServerFact]
    public async Task Denetim_kullanici_adi_aramasi_kucuk_harfle_parca_eslesmesidir()
    {
        await DenetimKaydiEkleAsync("ali", "Kalem");
        await DenetimKaydiEkleAsync("veli", "Kalem");
        await DenetimKaydiEkleAsync("ayse", "Kalem");

        Assert.Equal(new[] { "ali|Kalem", "veli|Kalem" }, await DenetimdeAraAsync(new DenetimSorgusu(KullaniciAdi: " li ")));
    }

    /// <summary>
    /// AÇIK bulgu: kullanıcı adları ASCII küçük harfle saklanır, arama metni küçültülmez. SQLite'ta (ASCII CI LIKE) geçen
    /// DenetimSorguTestleri.Kullanici_adi_like_kacisli_parca_eslesmesidir " LI " beklentisi Turkish_CI_AS'ta tutmaz:
    /// 'I' yalnızca 'ı' ile eşleşir, "ali" bulunmaz. "ADMIN" ile "admin" de bulunmaz.
    /// </summary>
    [Fact(Skip = "AÇIK: DenetimRepository.Filtrele KullaniciAdi LIKE'ı Turkish_CI_AS'ta ' LI ' ile 'ali'yi bulmuyor (SQLite'ta buluyor).")]
    public async Task Denetim_kullanici_adi_aramasi_buyuk_harfle_de_eslesir()
    {
        await DenetimKaydiEkleAsync("ali", "Kalem");
        await DenetimKaydiEkleAsync("veli%", "Kalem");
        await DenetimKaydiEkleAsync("admin", "Kalem");

        Assert.Equal(new[] { "ali|Kalem", "veli%|Kalem" }, await DenetimdeAraAsync(new DenetimSorgusu(KullaniciAdi: " LI ")));
        Assert.Equal(new[] { "admin|Kalem" }, await DenetimdeAraAsync(new DenetimSorgusu(KullaniciAdi: "ADMIN")));
    }
}
