using StokTakip.Domain.Entities;

namespace StokTakip.Domain.Repositories;

/// <summary>
/// Dönen varlıklar izlenir; değişiklikler <see cref="IUnitOfWork.SaveChangesAsync"/> ile kalıcı olur.
/// </summary>
public interface IUrunRepository
{
    Task<Urun?> GetByIdAsync(int urunId, CancellationToken cancellationToken = default);

    /// <summary>Tüm ürünler, UrunId'ye göre sıralı.</summary>
    Task<IReadOnlyList<Urun>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Ürüne bağlı (silinmiş olanlar dahil) sipariş kaydı var mı.</summary>
    Task<bool> HasSiparisAsync(int urunId, CancellationToken cancellationToken = default);

    Task AddAsync(Urun urun, CancellationToken cancellationToken = default);

    void Remove(Urun urun);

    void SetBeklenenRowVersion(Urun urun, byte[] beklenenRowVersion);
}
