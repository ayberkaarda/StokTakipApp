using StokTakip.Domain.Entities;

namespace StokTakip.Domain.Repositories;

/// <summary>
/// Denetim kayıtları yalnızca eklenir: güncelleme ve silme yoktur. Eklenen kayıt
/// <see cref="IUnitOfWork.SaveChangesAsync"/> ile, aynı kapsamdaki diğer değişikliklerle birlikte yazılır.
/// </summary>
public interface IDenetimRepository
{
    void Add(DenetimKaydi kayit);

    /// <summary>İzlenmeyen kayıtlar; Zaman'a, eşitlikte DenetimId'ye göre yeniden eskiye.</summary>
    Task<SayfaliSonuc<DenetimKaydi>> SayfaGetirAsync(
        DenetimFiltresi filtre, int sayfa, int sayfaBoyutu, CancellationToken cancellationToken = default);

    /// <summary>Dışa aktarım için anahtar tabanlı sayfa: <paramref name="sonDenetimId"/>'den küçük kayıtlar, DenetimId azalan.</summary>
    Task<IReadOnlyList<DenetimKaydi>> CsvSayfasiGetirAsync(
        DenetimFiltresi filtre, long? sonDenetimId, int adet, CancellationToken cancellationToken = default);
}
