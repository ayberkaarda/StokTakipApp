using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using StokTakip.Data.Tests.Altyapi;

namespace StokTakip.Data.Tests;

/// <summary>
/// Veritabanı kısıtları, domain doğrulamasını atlayan ham SQL ile denenir.
/// Not: şema EnsureCreated ile modelden üretilir, migration çalışmaz; SQLite nvarchar(200) uzunluğunu
/// zorlamaz, bu yüzden 200 karakter sınırı yalnızca domain/servis katmanında test edilir.
/// </summary>
public class SemaKisitlariTestleri
{
    [Fact]
    public async Task Stok_negatif_yapilamaz_CHECK_kisiti_calisir()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 10);

        await using var context = db.ContextOlustur();
        var ex = await Assert.ThrowsAsync<SqliteException>(
            () => context.Database.ExecuteSqlAsync($"UPDATE Urunler SET Stok = -1 WHERE UrunID = {urunId}"));

        Assert.Contains("CK_Urunler_Stok", ex.Message);
        Assert.Equal(10, (await Okuma.UrunAsync(db, urunId)).Stok);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task Siparis_adedi_sifir_veya_negatif_olamaz_CHECK_kisiti_calisir(int adet)
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 10);

        await using var context = db.ContextOlustur();
        var ex = await Assert.ThrowsAsync<SqliteException>(() => context.Database.ExecuteSqlAsync(
            $"INSERT INTO Siparisler (UrunID, Adet, Tarih, IsDeleted) VALUES ({urunId}, {adet}, '2026-01-01 00:00:00', 0)"));

        Assert.Contains("CK_Siparisler_Adet", ex.Message);
        Assert.Empty(await Okuma.TumSiparislerAsync(db));
    }

    [Fact]
    public async Task Siparisi_olan_urun_dogrudan_context_ile_silinmek_istenirse_yabanci_anahtar_engeller()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 10);
        using (var kapsam = new ServisKapsami(db))
        {
            await kapsam.SiparisService.SiparisOlusturAsync(new(urunId, 1));
        }

        await using var context = db.ContextOlustur();
        var ex = await Assert.ThrowsAsync<SqliteException>(
            () => context.Database.ExecuteSqlAsync($"DELETE FROM Urunler WHERE UrunID = {urunId}"));

        Assert.Contains("FOREIGN KEY", ex.Message);
    }

    [Fact]
    public async Task Tarih_verilmeden_eklenen_siparis_veritabani_varsayilan_tarihini_alir()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 10);

        await using (var context = db.ContextOlustur())
        {
            await context.Database.ExecuteSqlAsync(
                $"INSERT INTO Siparisler (UrunID, Adet, IsDeleted) VALUES ({urunId}, 1, 0)");
        }

        var siparis = Assert.Single(await Okuma.TumSiparislerAsync(db));
        Assert.True(siparis.Tarih > DateTime.Now.AddMinutes(-5));
    }
}
