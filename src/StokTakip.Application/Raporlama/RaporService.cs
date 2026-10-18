using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Raporlar;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Raporlama;

public sealed class RaporService : IRaporService
{
    private readonly IRaporRepository _repository;

    public RaporService(IRaporRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<IReadOnlyList<UrunSatisOzetiDto>> EnCokSatanlariGetirAsync(
        DateTime? baslangic, DateTime? bitis, int adet, CancellationToken cancellationToken = default)
    {
        if (adet is < 1 or > 100)
            throw new GecersizGirdiException("Adet 1 ile 100 arasında olmalıdır.", nameof(adet));
        AralikDogrula(baslangic, bitis);
        var sonuc = await _repository.EnCokSatanlariGetirAsync(baslangic?.Date, bitis?.Date, adet, cancellationToken);
        return sonuc.Select(x => new UrunSatisOzetiDto(x.UrunId, x.UrunAdi, x.ToplamAdet, x.SiparisSayisi)).ToList();
    }

    public async Task<IReadOnlyList<DonemOzetiDto>> GunlukOzetAsync(
        DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default)
    {
        AralikDogrula(baslangic, bitis);
        var sonuc = await _repository.GunlukOzetAsync(baslangic.Date, bitis.Date, cancellationToken);
        return Doldur(sonuc, baslangic.Date, bitis.Date, false, cancellationToken);
    }

    public async Task<IReadOnlyList<DonemOzetiDto>> AylikOzetAsync(
        DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default)
    {
        AralikDogrula(baslangic, bitis);
        var sonuc = await _repository.AylikOzetAsync(baslangic.Date, bitis.Date, cancellationToken);
        return Doldur(sonuc, new DateTime(baslangic.Year, baslangic.Month, 1),
            new DateTime(bitis.Year, bitis.Month, 1), true, cancellationToken);
    }

    private static void AralikDogrula(DateTime? baslangic, DateTime? bitis)
    {
        if (baslangic is not { } ilk || bitis is not { } son)
            return;
        if (ilk.Date > son.Date)
            throw new GecersizGirdiException("Başlangıç tarihi bitiş tarihinden sonra olamaz.", nameof(baslangic));
        if (ilk.Year <= 9994 && son.Date > ilk.Date.AddYears(5))
            throw new GecersizGirdiException("Tarih aralığı en çok 5 yıl olabilir.", nameof(bitis));
    }

    private static IReadOnlyList<DonemOzetiDto> Doldur(
        IReadOnlyList<DonemOzeti> ozetler, DateTime ilk, DateTime son, bool aylik, CancellationToken cancellationToken)
    {
        var donemler = ozetler.ToDictionary(x => x.Donem);
        var sonuc = new List<DonemOzetiDto>();
        for (var donem = ilk; ; donem = aylik ? donem.AddMonths(1) : donem.AddDays(1))
        {
            cancellationToken.ThrowIfCancellationRequested();
            sonuc.Add(donemler.TryGetValue(donem, out var ozet)
                ? new DonemOzetiDto(donem, ozet.SiparisSayisi, ozet.ToplamAdet)
                : new DonemOzetiDto(donem, 0, 0L));
            if (donem == son)
                break;
        }
        return sonuc;
    }
}
