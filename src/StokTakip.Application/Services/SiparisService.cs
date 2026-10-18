using StokTakip.Application.Dtos;
using StokTakip.Application.Mapping;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Services;

public sealed class SiparisService : ISiparisService
{
    private const string UrunKayitTuru = "Ürün";
    private const string SiparisKayitTuru = "Sipariş";

    private readonly ISiparisRepository _siparisRepository;
    private readonly IUrunRepository _urunRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public SiparisService(
        ISiparisRepository siparisRepository,
        IUrunRepository urunRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _siparisRepository = siparisRepository ?? throw new ArgumentNullException(nameof(siparisRepository));
        _urunRepository = urunRepository ?? throw new ArgumentNullException(nameof(urunRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<SiparisDto> SiparisOlusturAsync(SiparisOlusturIstegi istek, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(istek);

        if (istek.Adet <= 0)
        {
            throw new GecersizGirdiException("Adet sıfırdan büyük olmalıdır.", nameof(istek.Adet));
        }

        if (istek.UrunId <= 0)
        {
            throw new KayitBulunamadiException(UrunKayitTuru, istek.UrunId);
        }

        return await TransactionIcindeAsync(async () =>
        {
            var urun = await _urunRepository.GetByIdAsync(istek.UrunId, cancellationToken)
                ?? throw new KayitBulunamadiException(UrunKayitTuru, istek.UrunId);

            var siparis = Siparis.Olustur(urun, istek.Adet, _timeProvider.GetLocalNow().DateTime);
            await _siparisRepository.AddAsync(siparis, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return siparis.ToDto();
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<SiparisDto>> SiparisGecmisiniGetirAsync(CancellationToken cancellationToken = default)
    {
        var siparisler = await _siparisRepository.GetGecmisAsync(cancellationToken);
        return siparisler.Select(s => s.ToDto()).ToList();
    }

    public async Task<SiparisOzetiDto> SiparisOzetiGetirAsync(CancellationToken cancellationToken = default)
    {
        var siparisler = await _siparisRepository.GetGecmisAsync(cancellationToken);
        return new SiparisOzetiDto(siparisler.Count, siparisler.Sum(s => s.Adet));
    }

    public async Task SiparisSilAsync(int siparisId, CancellationToken cancellationToken = default)
    {
        if (siparisId <= 0)
        {
            throw new KayitBulunamadiException(SiparisKayitTuru, siparisId);
        }

        await TransactionIcindeAsync(async () =>
        {
            var siparis = await _siparisRepository.GetByIdAsync(siparisId, cancellationToken)
                ?? throw new KayitBulunamadiException(SiparisKayitTuru, siparisId);

            var urun = siparis.Urun
                ?? await _urunRepository.GetByIdAsync(siparis.UrunId, cancellationToken)
                ?? throw new KayitBulunamadiException(UrunKayitTuru, siparis.UrunId);

            siparis.Sil(urun);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }, cancellationToken);
    }

    private Task TransactionIcindeAsync(Func<Task> islem, CancellationToken cancellationToken) =>
        TransactionIcindeAsync(async () =>
        {
            await islem();
            return true;
        }, cancellationToken);

    private async Task<T> TransactionIcindeAsync<T>(Func<Task<T>> islem, CancellationToken cancellationToken)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken: cancellationToken);
        try
        {
            var sonuc = await islem();
            await transaction.CommitAsync(cancellationToken);
            return sonuc;
        }
        catch
        {
            try
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }
            catch
            {
                // Asıl hata korunur; commit edilmemiş transaction dispose sırasında da geri alınır.
            }

            throw;
        }
    }
}
