using StokTakip.Domain.Entities;

namespace StokTakip.Domain.Repositories;

/// <summary>
/// Dönen varlıklar izlenir; değişiklikler <see cref="IUnitOfWork.SaveChangesAsync"/> ile kalıcı olur.
/// Kullanıcı adları normalleştirilmiş (küçük harf) olarak verilmelidir.
/// </summary>
public interface IKullaniciRepository
{
    Task<Kullanici?> GetByIdAsync(int kullaniciId, CancellationToken cancellationToken = default);

    Task<Kullanici?> GetByKullaniciAdiAsync(string kullaniciAdi, CancellationToken cancellationToken = default);

    /// <summary>Tüm kullanıcılar, kullanıcı adına göre sıralı.</summary>
    Task<IReadOnlyList<Kullanici>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<bool> HerhangiBiriVarMiAsync(CancellationToken cancellationToken = default);

    Task<bool> KullaniciAdiKullaniliyorMuAsync(string kullaniciAdi, CancellationToken cancellationToken = default);

    Task<int> AktifAdminSayisiAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Kullanici kullanici, CancellationToken cancellationToken = default);
}
