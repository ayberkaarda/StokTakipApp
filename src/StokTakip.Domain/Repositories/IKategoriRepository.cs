using StokTakip.Domain.Entities;

namespace StokTakip.Domain.Repositories;

/// <summary>
/// Dönen varlıklar izlenir; değişiklikler <see cref="IUnitOfWork.SaveChangesAsync"/> ile kalıcı olur.
/// </summary>
public interface IKategoriRepository
{
    Task<Kategori?> GetByIdAsync(int kategoriId, CancellationToken cancellationToken = default);

    /// <summary>Tüm kategoriler, ada göre sıralı.</summary>
    Task<IReadOnlyList<Kategori>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verilen ad (büyük/küçük harf duyarsız) başka bir kategoride kullanılıyor mu.
    /// <paramref name="haricKategoriId"/> verilirse o kategori yok sayılır (yeniden adlandırma için).
    /// </summary>
    Task<bool> AdKullaniliyorMuAsync(string ad, int? haricKategoriId = null, CancellationToken cancellationToken = default);

    Task AddAsync(Kategori kategori, CancellationToken cancellationToken = default);

    /// <summary>Kategoriyi siler; bağlı ürünlerin kategorisi veritabanınca boşaltılır (ON DELETE SET NULL).</summary>
    void Remove(Kategori kategori);
}
