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

    /// <remarks>
    /// UPPER her iki tarafa uygulanır ki karşılaştırma, SQL Server'ın varsayılan (büyük/küçük harf duyarsız)
    /// harmanlamasından bağımsız ve SQLite'ta da aynı olsun. Tablo küçük olduğundan indeks kullanılamaması sorun değildir;
    /// eşzamanlı eklemelere karşı son garanti benzersiz indekstir.
    /// </remarks>
    public Task<bool> AdKullaniliyorMuAsync(string ad, int? haricKategoriId = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ad);
        var aranan = ad.Trim().ToUpperInvariant();
        return context.Kategoriler.AnyAsync(
            x => x.Ad.ToUpper() == aranan && (haricKategoriId == null || x.KategoriId != haricKategoriId),
            cancellationToken);
    }

    public async Task AddAsync(Kategori kategori, CancellationToken cancellationToken = default)
        => await context.Kategoriler.AddAsync(kategori, cancellationToken);

    public void Remove(Kategori kategori) => context.Kategoriler.Remove(kategori);
}
