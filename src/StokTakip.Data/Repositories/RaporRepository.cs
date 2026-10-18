using Microsoft.EntityFrameworkCore;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Raporlar;
using StokTakip.Domain.Repositories;

namespace StokTakip.Data.Repositories;

public sealed class RaporRepository(StokTakipDbContext context) : IRaporRepository
{
    public async Task<IReadOnlyList<UrunSatisOzeti>> EnCokSatanlariGetirAsync(
        DateTime? baslangic, DateTime? bitis, int adet, CancellationToken cancellationToken = default)
    {
        var sonuc = await Filtrele(baslangic, bitis)
            .GroupBy(x => new { x.UrunId, UrunAdi = x.Urun == null ? string.Empty : x.Urun.UrunAdi })
            .Select(g => new
            {
                g.Key.UrunId, g.Key.UrunAdi,
                ToplamAdet = g.Sum(x => (long)x.Adet), SiparisSayisi = g.Count()
            })
            .OrderByDescending(x => x.ToplamAdet).ThenBy(x => x.UrunAdi).ThenBy(x => x.UrunId)
            .Take(adet).ToListAsync(cancellationToken);
        return sonuc.Select(x => new UrunSatisOzeti(x.UrunId, x.UrunAdi, x.ToplamAdet, x.SiparisSayisi)).ToList();
    }

    public async Task<IReadOnlyList<DonemOzeti>> GunlukOzetAsync(
        DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default)
    {
        var sonuc = await Filtrele(baslangic, bitis)
            .GroupBy(x => x.Tarih.Date)
            .Select(g => new { Donem = g.Key, SiparisSayisi = g.Count(), ToplamAdet = g.Sum(x => (long)x.Adet) })
            .OrderBy(x => x.Donem).ToListAsync(cancellationToken);
        return sonuc.Select(x => new DonemOzeti(x.Donem, x.SiparisSayisi, x.ToplamAdet)).ToList();
    }

    public async Task<IReadOnlyList<DonemOzeti>> AylikOzetAsync(
        DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default)
    {
        var sonuc = await Filtrele(baslangic, bitis)
            .GroupBy(x => new { x.Tarih.Year, x.Tarih.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, SiparisSayisi = g.Count(), ToplamAdet = g.Sum(x => (long)x.Adet) })
            .OrderBy(x => x.Year).ThenBy(x => x.Month).ToListAsync(cancellationToken);
        return sonuc.Select(x => new DonemOzeti(new DateTime(x.Year, x.Month, 1), x.SiparisSayisi, x.ToplamAdet)).ToList();
    }

    private IQueryable<Siparis> Filtrele(DateTime? baslangic, DateTime? bitis)
    {
        var sorgu = context.Siparisler.AsNoTracking();
        if (baslangic is { } ilk)
        {
            var ilkGun = ilk.Date;
            sorgu = sorgu.Where(x => x.Tarih >= ilkGun);
        }
        if (bitis is { } son && son.Date < DateTime.MaxValue.Date)
        {
            var sonHaric = son.Date.AddDays(1);
            sorgu = sorgu.Where(x => x.Tarih < sonHaric);
        }
        return sorgu;
    }
}
