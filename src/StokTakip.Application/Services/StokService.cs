using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StokTakip.Application.Dtos;
using StokTakip.Application.Mapping;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Services;

public sealed class StokService : IStokService
{
    private const string UrunKayitTuru = "Ürün";

    private readonly IUrunRepository _urunRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StokService> _logger;

    public StokService(IUrunRepository urunRepository, IUnitOfWork unitOfWork)
        : this(urunRepository, unitOfWork, NullLogger<StokService>.Instance)
    {
    }

    public StokService(IUrunRepository urunRepository, IUnitOfWork unitOfWork, ILogger<StokService> logger)
    {
        _urunRepository = urunRepository ?? throw new ArgumentNullException(nameof(urunRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<UrunDto>> UrunleriListeleAsync(CancellationToken cancellationToken = default)
    {
        var urunler = await _urunRepository.GetAllAsync(cancellationToken);
        return urunler.Select(u => u.ToDto()).ToList();
    }

    public async Task<UrunDto?> UrunGetirAsync(int urunId, CancellationToken cancellationToken = default)
    {
        if (urunId <= 0)
        {
            return null;
        }

        var urun = await _urunRepository.GetByIdAsync(urunId, cancellationToken);
        return urun?.ToDto();
    }

    public async Task<UrunDto> UrunEkleAsync(UrunEkleIstegi istek, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(istek);

        try
        {
            var urun = new Urun(istek.UrunAdi, istek.Stok);
            await _urunRepository.AddAsync(urun, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Ürün eklendi. UrunId={UrunId}", urun.UrunId);
            return urun.ToDto();
        }
        catch (DomainException ex) when (_logger.IhlaliLogla(ex, nameof(UrunEkleAsync)))
        {
            throw;
        }
    }

    public async Task<UrunDto> UrunGuncelleAsync(UrunGuncelleIstegi istek, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(istek);

        try
        {
            // Ad ve stok birlikte doğrulanır ki izlenen varlık yarım güncellenmesin.
            AdDogrula(istek.UrunAdi);
            StokDogrula(istek.Stok);

            var urun = await UrunuBulAsync(istek.UrunId, cancellationToken);
            urun.AdiDegistir(istek.UrunAdi);
            urun.StokAyarla(istek.Stok);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Ürün güncellendi. UrunId={UrunId}", urun.UrunId);
            return urun.ToDto();
        }
        catch (DomainException ex) when (_logger.IhlaliLogla(ex, nameof(UrunGuncelleAsync)))
        {
            throw;
        }
    }

    public async Task<UrunDto> StokGuncelleAsync(int urunId, int yeniStok, CancellationToken cancellationToken = default)
    {
        try
        {
            StokDogrula(yeniStok);

            var urun = await UrunuBulAsync(urunId, cancellationToken);
            urun.StokAyarla(yeniStok);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Stok güncellendi. UrunId={UrunId}, YeniStok={YeniStok}", urunId, yeniStok);
            return urun.ToDto();
        }
        catch (DomainException ex) when (_logger.IhlaliLogla(ex, nameof(StokGuncelleAsync)))
        {
            throw;
        }
    }

    public async Task UrunSilAsync(int urunId, CancellationToken cancellationToken = default)
    {
        try
        {
            var urun = await UrunuBulAsync(urunId, cancellationToken);

            if (await _urunRepository.HasSiparisAsync(urunId, cancellationToken))
            {
                throw new GecersizIslemException($"'{urun.UrunAdi}' ürününe ait sipariş kaydı bulunduğu için ürün silinemez.");
            }

            _urunRepository.Remove(urun);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Ürün silindi. UrunId={UrunId}", urunId);
        }
        catch (DomainException ex) when (_logger.IhlaliLogla(ex, nameof(UrunSilAsync)))
        {
            throw;
        }
    }

    private async Task<Urun> UrunuBulAsync(int urunId, CancellationToken cancellationToken)
    {
        var urun = urunId > 0 ? await _urunRepository.GetByIdAsync(urunId, cancellationToken) : null;
        return urun ?? throw new KayitBulunamadiException(UrunKayitTuru, urunId);
    }

    private static void AdDogrula(string? urunAdi)
    {
        if (string.IsNullOrWhiteSpace(urunAdi))
        {
            throw new GecersizGirdiException("Ürün adı boş olamaz.", nameof(Urun.UrunAdi));
        }
    }

    private static void StokDogrula(int stok)
    {
        if (stok < 0)
        {
            throw new GecersizGirdiException("Stok miktarı negatif olamaz.", nameof(Urun.Stok));
        }
    }
}
