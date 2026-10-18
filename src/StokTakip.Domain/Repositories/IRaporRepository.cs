using StokTakip.Domain.Raporlar;

namespace StokTakip.Domain.Repositories;

public interface IRaporRepository
{
    Task<IReadOnlyList<UrunSatisOzeti>> EnCokSatanlariGetirAsync(
        DateTime? baslangic, DateTime? bitis, int adet, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DonemOzeti>> GunlukOzetAsync(
        DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DonemOzeti>> AylikOzetAsync(
        DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default);
}
