using Microsoft.EntityFrameworkCore;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Repositories;

namespace StokTakip.Data.Repositories;

/// <remarks>Kullanıcı adları küçük harfle saklandığından aramalar düz eşitlikle yapılır ve benzersiz indeksi kullanır.</remarks>
public sealed class KullaniciRepository(StokTakipDbContext context) : IKullaniciRepository
{
    public Task<Kullanici?> GetByIdAsync(int kullaniciId, CancellationToken cancellationToken = default)
        => context.Kullanicilar.AsTracking()
            .SingleOrDefaultAsync(x => x.KullaniciId == kullaniciId, cancellationToken);

    public Task<Kullanici?> GetByKullaniciAdiAsync(string kullaniciAdi, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(kullaniciAdi);
        return context.Kullanicilar.AsTracking()
            .SingleOrDefaultAsync(x => x.KullaniciAdi == kullaniciAdi, cancellationToken);
    }

    public async Task<IReadOnlyList<Kullanici>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Kullanicilar.AsTracking()
            .OrderBy(x => x.KullaniciAdi)
            .ToListAsync(cancellationToken);

    public Task<bool> HerhangiBiriVarMiAsync(CancellationToken cancellationToken = default)
        => context.Kullanicilar.AnyAsync(cancellationToken);

    public Task<bool> KullaniciAdiKullaniliyorMuAsync(string kullaniciAdi, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(kullaniciAdi);
        return context.Kullanicilar.AnyAsync(x => x.KullaniciAdi == kullaniciAdi, cancellationToken);
    }

    public Task<int> AktifAdminSayisiAsync(CancellationToken cancellationToken = default)
        => context.Kullanicilar.CountAsync(x => x.Aktif && x.Rol == KullaniciRolu.Admin, cancellationToken);

    public async Task AddAsync(Kullanici kullanici, CancellationToken cancellationToken = default)
        => await context.Kullanicilar.AddAsync(kullanici, cancellationToken);
}
