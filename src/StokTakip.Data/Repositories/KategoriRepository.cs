using Microsoft.EntityFrameworkCore;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Repositories;

namespace StokTakip.Data.Repositories;

public sealed class KategoriRepository(StokTakipDbContext context) : IKategoriRepository
{
    public Task<Kategori?> GetByIdAsync(int kategoriId, CancellationToken cancellationToken = default)
        => context.Kategoriler.AsTracking()
            .SingleOrDefaultAsync(x => x.KategoriId == kategoriId, cancellationToken);

    public async Task<IReadOnlyList<Kategori>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Kategoriler.AsTracking()
            .OrderBy(x => x.Ad).ThenBy(x => x.KategoriId)
            .ToListAsync(cancellationToken);

    public Task<bool> AdKullaniliyorMuAsync(string ad, int? haricKategoriId = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ad);
        var aranan = ad.Trim();
        var sorgu = context.Kategoriler.Where(x => haricKategoriId == null || x.KategoriId != haricKategoriId);
        // SQL Server kendi kollasyonunu kullanır; SQLite'ın mevcut harf duyarsız kontrolü korunur.
        if (context.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            aranan = aranan.ToUpperInvariant();
            return sorgu.AnyAsync(x => x.Ad.ToUpper() == aranan, cancellationToken);
        }

        return sorgu.AnyAsync(x => x.Ad == aranan, cancellationToken);
    }

    public async Task AddAsync(Kategori kategori, CancellationToken cancellationToken = default)
        => await context.Kategoriler.AddAsync(kategori, cancellationToken);

    public void Remove(Kategori kategori) => context.Kategoriler.Remove(kategori);
}
