using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StokTakip.Application.Dtos;
using StokTakip.Application.Mapping;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Services;

public sealed class KategoriService : IKategoriService
{
    private const string KategoriKayitTuru = "Kategori";

    private readonly IKategoriRepository _kategoriRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<KategoriService> _logger;

    public KategoriService(IKategoriRepository kategoriRepository, IUnitOfWork unitOfWork)
        : this(kategoriRepository, unitOfWork, NullLogger<KategoriService>.Instance)
    {
    }

    public KategoriService(IKategoriRepository kategoriRepository, IUnitOfWork unitOfWork, ILogger<KategoriService> logger)
    {
        _kategoriRepository = kategoriRepository ?? throw new ArgumentNullException(nameof(kategoriRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<KategoriDto>> KategorileriListeleAsync(CancellationToken cancellationToken = default)
    {
        var kategoriler = await _kategoriRepository.GetAllAsync(cancellationToken);
        return kategoriler.Select(k => k.ToDto()).ToList();
    }

    public async Task<KategoriDto> KategoriEkleAsync(string ad, CancellationToken cancellationToken = default)
    {
        try
        {
            var kirpilmis = Kategori.AdiDogrula(ad);
            await AdBenzersizOlmaliAsync(kirpilmis, null, cancellationToken);

            var kategori = new Kategori(kirpilmis);
            await _kategoriRepository.AddAsync(kategori, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Kategori eklendi. KategoriId={KategoriId}", kategori.KategoriId);
            return kategori.ToDto();
        }
        catch (DomainException ex) when (_logger.IhlaliLogla(ex, nameof(KategoriEkleAsync)))
        {
            throw;
        }
    }

    public async Task<KategoriDto> KategoriYenidenAdlandirAsync(int kategoriId, string yeniAd, CancellationToken cancellationToken = default)
    {
        try
        {
            var kirpilmis = Kategori.AdiDogrula(yeniAd);
            var kategori = await KategoriyiBulAsync(kategoriId, cancellationToken);
            await AdBenzersizOlmaliAsync(kirpilmis, kategoriId, cancellationToken);

            kategori.YenidenAdlandir(kirpilmis);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Kategori yeniden adlandırıldı. KategoriId={KategoriId}", kategoriId);
            return kategori.ToDto();
        }
        catch (DomainException ex) when (_logger.IhlaliLogla(ex, nameof(KategoriYenidenAdlandirAsync)))
        {
            throw;
        }
    }

    public async Task KategoriSilAsync(int kategoriId, CancellationToken cancellationToken = default)
    {
        try
        {
            var kategori = await KategoriyiBulAsync(kategoriId, cancellationToken);
            _kategoriRepository.Remove(kategori);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Kategori silindi. KategoriId={KategoriId}", kategoriId);
        }
        catch (DomainException ex) when (_logger.IhlaliLogla(ex, nameof(KategoriSilAsync)))
        {
            throw;
        }
    }

    private async Task<Kategori> KategoriyiBulAsync(int kategoriId, CancellationToken cancellationToken)
    {
        var kategori = kategoriId > 0 ? await _kategoriRepository.GetByIdAsync(kategoriId, cancellationToken) : null;
        return kategori ?? throw new KayitBulunamadiException(KategoriKayitTuru, kategoriId);
    }

    private async Task AdBenzersizOlmaliAsync(string ad, int? haricKategoriId, CancellationToken cancellationToken)
    {
        if (await _kategoriRepository.AdKullaniliyorMuAsync(ad, haricKategoriId, cancellationToken))
        {
            throw new GecersizGirdiException($"'{ad}' adında bir kategori zaten var.", nameof(Kategori.Ad));
        }
    }
}
