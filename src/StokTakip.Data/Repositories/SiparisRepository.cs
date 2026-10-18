using Microsoft.EntityFrameworkCore;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Repositories;

namespace StokTakip.Data.Repositories;

public sealed class SiparisRepository(StokTakipDbContext context) : ISiparisRepository
{
    public Task<Siparis?> GetByIdAsync(int siparisId, CancellationToken cancellationToken = default)
        => context.Siparisler.IgnoreQueryFilters().AsTracking().Include(x => x.Urun)
            .SingleOrDefaultAsync(x => x.SiparisId == siparisId, cancellationToken);

    public async Task<IReadOnlyList<Siparis>> GetGecmisAsync(CancellationToken cancellationToken = default)
        => await context.Siparisler.AsTracking().Include(x => x.Urun)
            .OrderByDescending(x => x.Tarih).ThenByDescending(x => x.SiparisId)
            .ToListAsync(cancellationToken);

    public async Task<(int SiparisSayisi, long ToplamAdet)> GetOzetAsync(CancellationToken cancellationToken = default)
    {
        var ozet = await context.Siparisler
            .GroupBy(_ => 1)
            .Select(g => new { Sayi = g.Count(), Toplam = g.Sum(x => (long)x.Adet) })
            .SingleOrDefaultAsync(cancellationToken);
        return ozet is null ? (0, 0L) : (ozet.Sayi, ozet.Toplam);
    }

    public async Task AddAsync(Siparis siparis, CancellationToken cancellationToken = default)
        => await context.Siparisler.AddAsync(siparis, cancellationToken);
}
