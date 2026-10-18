using Microsoft.EntityFrameworkCore;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Repositories;

namespace StokTakip.Data.Repositories;

public sealed class UrunRepository(StokTakipDbContext context) : IUrunRepository
{
    public Task<Urun?> GetByIdAsync(int urunId, CancellationToken cancellationToken = default)
        => context.Urunler.AsTracking()
            .SingleOrDefaultAsync(x => x.UrunId == urunId, cancellationToken);

    public async Task<IReadOnlyList<Urun>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Urunler.AsTracking().OrderBy(x => x.UrunId).ToListAsync(cancellationToken);

    public Task<bool> HasSiparisAsync(int urunId, CancellationToken cancellationToken = default)
        => context.Siparisler.IgnoreQueryFilters()
            .AnyAsync(x => x.UrunId == urunId, cancellationToken);

    public async Task AddAsync(Urun urun, CancellationToken cancellationToken = default)
        => await context.Urunler.AddAsync(urun, cancellationToken);

    public void Remove(Urun urun) => context.Urunler.Remove(urun);

    public void SetBeklenenRowVersion(Urun urun, byte[] beklenenRowVersion)
    {
        ArgumentNullException.ThrowIfNull(urun);
        ArgumentNullException.ThrowIfNull(beklenenRowVersion);
        context.Entry(urun).Property(x => x.RowVersion).OriginalValue = beklenenRowVersion.ToArray();
        context.Entry(urun).Property(x => x.Stok).IsModified = true;
    }
}
