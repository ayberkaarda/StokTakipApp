using StokTakip.Data.Tests.Altyapi;
using StokTakip.Domain.Entities;

namespace StokTakip.Data.Tests;

/// <summary>UnitOfWork transaction davranışı: geri alma sonrası ChangeTracker temizliği.</summary>
public class UnitOfWorkTestleri
{
    [Fact]
    public async Task Commit_edilmeden_kapatilan_transaction_sonrasi_ayni_kapsamdaki_kayit_eski_degisikligi_yazmaz()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 10);

        using var kapsam = new ServisKapsami(db);
        await using (await kapsam.UnitOfWork.BeginTransactionAsync())
        {
            // Kaydedilmemiş değişiklik: stok düşüldü, sipariş eklendi; commit yok.
            var urun = (await kapsam.UrunRepository.GetByIdAsync(urunId))!;
            await kapsam.SiparisRepository.AddAsync(Siparis.Olustur(urun, 4, DateTime.Now));
        }

        await kapsam.StokService.UrunEkleAsync(new("Defter", 1));

        Assert.Equal(10, (await Okuma.UrunAsync(db, urunId)).Stok);
        Assert.Empty(await Okuma.TumSiparislerAsync(db));
    }

    [Fact]
    public async Task RollbackAsync_sonrasi_ayni_kapsamda_okunan_urun_veritabanindaki_degeri_gosterir()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 10);

        using var kapsam = new ServisKapsami(db);
        await using (var tx = await kapsam.UnitOfWork.BeginTransactionAsync())
        {
            var urun = (await kapsam.UrunRepository.GetByIdAsync(urunId))!;
            urun.StokAyarla(99);
            await kapsam.UnitOfWork.SaveChangesAsync();
            await tx.RollbackAsync();
        }

        Assert.Equal(10, (await kapsam.UrunRepository.GetByIdAsync(urunId))!.Stok);
        Assert.Equal(10, (await Okuma.UrunAsync(db, urunId)).Stok);
    }

    [Fact]
    public async Task Commit_edilen_transaction_degisiklikleri_kalici_olur_ve_izlenen_varliklar_korunur()
    {
        using var db = new TestVeritabani();
        var urunId = await Okuma.UrunEkleAsync(db, "Kalem", 10);

        using var kapsam = new ServisKapsami(db);
        Urun urun;
        await using (var tx = await kapsam.UnitOfWork.BeginTransactionAsync())
        {
            urun = (await kapsam.UrunRepository.GetByIdAsync(urunId))!;
            urun.StokAyarla(3);
            await kapsam.UnitOfWork.SaveChangesAsync();
            await tx.CommitAsync();
        }

        Assert.Same(urun, await kapsam.UrunRepository.GetByIdAsync(urunId));
        Assert.Equal(3, (await Okuma.UrunAsync(db, urunId)).Stok);
    }
}
