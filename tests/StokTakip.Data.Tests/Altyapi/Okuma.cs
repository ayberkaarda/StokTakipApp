using Microsoft.EntityFrameworkCore;
using StokTakip.Domain.Entities;

namespace StokTakip.Data.Tests.Altyapi;

/// <summary>Doğrulamalar her zaman yeni bir context ile, izlenen varlıklardan bağımsız olarak veritabanından okunur.</summary>
public static class Okuma
{
    public static async Task<Urun> UrunAsync(TestVeritabani veritabani, int urunId)
    {
        await using var context = veritabani.ContextOlustur();
        return await context.Urunler.AsNoTracking().SingleAsync(x => x.UrunId == urunId);
    }

    public static async Task<List<Siparis>> TumSiparislerAsync(TestVeritabani veritabani)
    {
        await using var context = veritabani.ContextOlustur();
        return await context.Siparisler.IgnoreQueryFilters().AsNoTracking()
            .OrderBy(x => x.SiparisId).ToListAsync();
    }

    public static async Task<int> UrunEkleAsync(TestVeritabani veritabani, string ad, int stok)
    {
        using var kapsam = new ServisKapsami(veritabani);
        var dto = await kapsam.StokService.UrunEkleAsync(new(ad, stok));
        return dto.UrunId;
    }
}
