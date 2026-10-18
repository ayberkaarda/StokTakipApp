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
        var siparisSayisi = await context.Siparisler.CountAsync(cancellationToken);
        var toplamAdet = await context.Siparisler.SumAsync(x => (long?)x.Adet, cancellationToken) ?? 0L;
        return (siparisSayisi, toplamAdet);
    }

    public async Task AddAsync(Siparis siparis, CancellationToken cancellationToken = default)
        => await context.Siparisler.AddAsync(siparis, cancellationToken);
}
