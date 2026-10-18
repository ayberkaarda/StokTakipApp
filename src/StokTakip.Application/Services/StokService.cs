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
    private readonly IKategoriRepository? _kategoriRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StokService> _logger;

    /// <summary>Kategori deposu olmadan; kategori atayan istekler <see cref="InvalidOperationException"/> verir.</summary>
    public StokService(IUrunRepository urunRepository, IUnitOfWork unitOfWork)
        : this(urunRepository, unitOfWork, NullLogger<StokService>.Instance)
    {
    }

    /// <summary>Kategori deposu olmadan; kategori atayan istekler <see cref="InvalidOperationException"/> verir.</summary>
    public StokService(IUrunRepository urunRepository, IUnitOfWork unitOfWork, ILogger<StokService> logger)
        : this(urunRepository, null, unitOfWork, logger)
    {
    }

    public StokService(IUrunRepository urunRepository, IKategoriRepository kategoriRepository, IUnitOfWork unitOfWork)
        : this(urunRepository, kategoriRepository ?? throw new ArgumentNullException(nameof(kategoriRepository)), unitOfWork, NullLogger<StokService>.Instance)
    {
    }

    public StokService(
        IUrunRepository urunRepository,
        IKategoriRepository? kategoriRepository,
        IUnitOfWork unitOfWork,
        ILogger<StokService> logger)
    {
        _urunRepository = urunRepository ?? throw new ArgumentNullException(nameof(urunRepository));
        _kategoriRepository = kategoriRepository;
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
            // Ad, stok ve eşik kurucuda doğrulanır; kategori ancak girdiler geçerliyse aranır.
            var urun = new Urun(istek.UrunAdi, istek.Stok, istek.DusukStokEsigi);
            if (istek.KategoriId is { } kategoriId)
            {
                urun.KategoriyeAta(await KategoriyiBulAsync(kategoriId, cancellationToken));
            }

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

    public async Task<UrunDto> UrunGuncelleAsync(UrunGuncelleIstegi istek, CancellationToken cancellationToken = default, byte[]? beklenenRowVersion = null)
    {
        ArgumentNullException.ThrowIfNull(istek);

        try
        {
            // Tüm alanlar varlığa dokunmadan önce doğrulanır ki izlenen varlık yarım güncellenmesin.
            AdDogrula(istek.UrunAdi);
            StokDogrula(istek.Stok);
            if (istek.DusukStokEsigi is { } esik)
            {
                Urun.EsikDogrula(esik);
            }

            var urun = await UrunuBulAsync(istek.UrunId, cancellationToken);
            var kategori = istek.KategoriGuncellensin && istek.KategoriId is { } kategoriId
                ? await KategoriyiBulAsync(kategoriId, cancellationToken)
                : null;

            if (beklenenRowVersion is not null)
            {
                _urunRepository.SetBeklenenRowVersion(urun, beklenenRowVersion);
            }

            urun.AdiDegistir(istek.UrunAdi);
            urun.StokAyarla(istek.Stok);
            if (istek.DusukStokEsigi is { } yeniEsik)
            {
                urun.DusukStokEsiginiAyarla(yeniEsik);
            }

            if (istek.KategoriGuncellensin)
            {
                urun.KategoriyeAta(kategori);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Ürün güncellendi. UrunId={UrunId}", urun.UrunId);
            return urun.ToDto();
        }
        catch (DomainException ex) when (_logger.IhlaliLogla(ex, nameof(UrunGuncelleAsync)))
        {
            throw;
        }
    }

    public async Task<UrunDto> StokGuncelleAsync(int urunId, int yeniStok, CancellationToken cancellationToken = default, byte[]? beklenenRowVersion = null)
    {
        try
        {
            StokDogrula(yeniStok);

            var urun = await UrunuBulAsync(urunId, cancellationToken);
            if (beklenenRowVersion is not null)
            {
                _urunRepository.SetBeklenenRowVersion(urun, beklenenRowVersion);
            }

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

    public async Task UrunSilAsync(int urunId, CancellationToken cancellationToken = default, byte[]? beklenenRowVersion = null)
    {
        try
        {
            var urun = await UrunuBulAsync(urunId, cancellationToken);

            if (await _urunRepository.HasSiparisAsync(urunId, cancellationToken))
            {
                throw new GecersizIslemException($"'{urun.UrunAdi}' ürününe ait sipariş kaydı bulunduğu için ürün silinemez.");
            }

            if (beklenenRowVersion is not null)
            {
                _urunRepository.SetBeklenenRowVersion(urun, beklenenRowVersion);
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

    /// <exception cref="GecersizGirdiException">Kategori yoksa; seçim alanı hatası olarak raporlanır.</exception>
    private async Task<Kategori> KategoriyiBulAsync(int kategoriId, CancellationToken cancellationToken)
    {
        if (_kategoriRepository is null)
        {
            throw new InvalidOperationException("Kategori ataması için StokService'e IKategoriRepository verilmelidir.");
        }

        var kategori = kategoriId > 0 ? await _kategoriRepository.GetByIdAsync(kategoriId, cancellationToken) : null;
        return kategori ?? throw new GecersizGirdiException("Seçilen kategori bulunamadı.", nameof(Urun.KategoriId));
    }

    private static void AdDogrula(string? urunAdi)
    {
        if (string.IsNullOrWhiteSpace(urunAdi))
        {
            throw new GecersizGirdiException("Ürün adı boş olamaz.", nameof(Urun.UrunAdi));
        }

        if (urunAdi.Length > Urun.UrunAdiMaksimumUzunluk)
        {
            throw new GecersizGirdiException($"Ürün adı en fazla {Urun.UrunAdiMaksimumUzunluk} karakter olabilir.", nameof(Urun.UrunAdi));
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
