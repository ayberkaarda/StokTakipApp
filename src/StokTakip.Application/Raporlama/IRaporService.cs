namespace StokTakip.Application.Raporlama;

public interface IRaporService
{
    Task<IReadOnlyList<UrunSatisOzetiDto>> EnCokSatanlariGetirAsync(
        DateTime? baslangic, DateTime? bitis, int adet, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DonemOzetiDto>> GunlukOzetAsync(
        DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DonemOzetiDto>> AylikOzetAsync(
        DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default);
}
