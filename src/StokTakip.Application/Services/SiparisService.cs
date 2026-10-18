using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StokTakip.Application.Denetim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Mapping;
using StokTakip.Domain;
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
    private readonly ILogger<SiparisService> _logger;
    private readonly IDenetimKaydedici _denetim;

    public SiparisService(
        ISiparisRepository siparisRepository,
        IUrunRepository urunRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
        : this(siparisRepository, urunRepository, unitOfWork, timeProvider, NullLogger<SiparisService>.Instance)
    {
    }

    public SiparisService(
        ISiparisRepository siparisRepository,
        IUrunRepository urunRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ILogger<SiparisService> logger)
        : this(siparisRepository, urunRepository, unitOfWork, timeProvider, logger, NullDenetimKaydedici.Instance)
    {
    }

    public SiparisService(
        ISiparisRepository siparisRepository,
        IUrunRepository urunRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ILogger<SiparisService> logger,
        IDenetimKaydedici denetim)
    {
        _denetim = denetim ?? throw new ArgumentNullException(nameof(denetim));
        _siparisRepository = siparisRepository ?? throw new ArgumentNullException(nameof(siparisRepository));
        _urunRepository = urunRepository ?? throw new ArgumentNullException(nameof(urunRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SiparisDto> SiparisOlusturAsync(SiparisOlusturIstegi istek, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(istek);

        try
        {
            if (istek.Adet <= 0)
            {
                throw new GecersizGirdiException("Adet sıfırdan büyük olmalıdır.", nameof(istek.Adet));
            }

            if (istek.UrunId <= 0)
            {
                throw new KayitBulunamadiException(UrunKayitTuru, istek.UrunId);
            }

            var sonuc = await TransactionIcindeAsync(async () =>
            {
                var urun = await _urunRepository.GetByIdAsync(istek.UrunId, cancellationToken)
                    ?? throw new KayitBulunamadiException(UrunKayitTuru, istek.UrunId);

                var eskiStok = urun.Stok;
                var siparis = Siparis.Olustur(urun, istek.Adet, _timeProvider.GetLocalNow().DateTime);
                await _siparisRepository.AddAsync(siparis, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                var no = DenetimMetni.Sayi(siparis.SiparisId);
                await _denetim.KaydetVeYazAsync(
                    new DenetimOlayi(
                        DenetimIslemi.SiparisOlusturuldu, DenetimVarlikTurleri.Siparis,
                        $"Sipariş #{no} oluşturuldu: {urun.UrunAdi} × {DenetimMetni.Sayi(siparis.Adet)}, kalan stok {DenetimMetni.Sayi(urun.Stok)} ({DenetimMetni.Degisim(eskiStok, urun.Stok)})",
                        siparis.SiparisId, urun.UrunAdi, DenetimMetni.Sayi(eskiStok), DenetimMetni.Sayi(urun.Stok), -siparis.Adet),
                    cancellationToken);
                return siparis.ToDto();
            }, cancellationToken);

            _logger.LogInformation(
                "Sipariş oluşturuldu. SiparisId={SiparisId}, UrunId={UrunId}, Adet={Adet}",
                sonuc.SiparisId, istek.UrunId, istek.Adet);
            return sonuc;
        }
        catch (DomainException ex) when (_logger.IhlaliLogla(ex, nameof(SiparisOlusturAsync)))
        {
            throw;
        }
    }

    public async Task<IReadOnlyList<SiparisDto>> SiparisGecmisiniGetirAsync(CancellationToken cancellationToken = default)
    {
        var siparisler = await _siparisRepository.GetGecmisAsync(cancellationToken);
        return siparisler.Select(s => s.ToDto()).ToList();
    }

    public async Task<SayfaliSonucDto<SiparisDto>> SiparisGecmisiSayfaGetirAsync(
        SiparisGecmisiSorgusu sorgu, CancellationToken cancellationToken = default)
    {
        var sonuc = await _siparisRepository.GetGecmisSayfaAsync(
            FiltreOlustur(sorgu), Math.Max(1, sorgu.Sayfa), Math.Clamp(sorgu.SayfaBoyutu, 1, 200), cancellationToken);
        return sonuc.ToDto(s => s.ToDto());
    }

    public async Task<IReadOnlyList<SiparisDto>> SiparisCsvSayfasiGetirAsync(
        SiparisGecmisiSorgusu sorgu, int? sonSiparisId, int adet, CancellationToken cancellationToken = default)
    {
        var siparisler = await _siparisRepository.CsvSayfasiGetirAsync(
            FiltreOlustur(sorgu), sonSiparisId, Math.Clamp(adet, 1, 1000), cancellationToken);
        return siparisler.Select(s => s.ToDto()).ToList();
    }

    private static SiparisFiltresi FiltreOlustur(SiparisGecmisiSorgusu sorgu)
    {
        ArgumentNullException.ThrowIfNull(sorgu);
        if (sorgu.Baslangic.HasValue && sorgu.Bitis.HasValue && sorgu.Baslangic.Value.Date > sorgu.Bitis.Value.Date)
            throw new GecersizGirdiException("Başlangıç tarihi bitiş tarihinden sonra olamaz.", nameof(sorgu.Baslangic));

        var urunAdi = sorgu.UrunAdi?.Trim();
        if (urunAdi?.Length > 100)
            urunAdi = urunAdi[..100];

        return new SiparisFiltresi(sorgu.Baslangic, sorgu.Bitis, urunAdi, sorgu.UrunId);
    }

    public async Task<SiparisOzetiDto> SiparisOzetiGetirAsync(CancellationToken cancellationToken = default)
    {
        var ozet = await _siparisRepository.GetOzetAsync(cancellationToken);
        // UI uyumluluğu için int toplamı taşma yerine üst sınıra doyurulur.
        return new SiparisOzetiDto(ozet.SiparisSayisi, (int)Math.Min(ozet.ToplamAdet, int.MaxValue));
    }

    public async Task SiparisSilAsync(int siparisId, CancellationToken cancellationToken = default)
    {
        try
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

                var eskiStok = urun.Stok;
                siparis.Sil(urun);
                _denetim.Kaydet(new DenetimOlayi(
                    DenetimIslemi.SiparisSilindi, DenetimVarlikTurleri.Siparis,
                    $"Sipariş #{DenetimMetni.Sayi(siparis.SiparisId)} silindi: {urun.UrunAdi} (ürün #{DenetimMetni.Sayi(urun.UrunId)}) stoğuna {DenetimMetni.Sayi(siparis.Adet)} iade edildi ({DenetimMetni.Degisim(eskiStok, urun.Stok)})",
                    siparis.SiparisId, urun.UrunAdi, DenetimMetni.Sayi(eskiStok), DenetimMetni.Sayi(urun.Stok), siparis.Adet));
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }, cancellationToken);

            _logger.LogInformation("Sipariş silindi, stok iade edildi. SiparisId={SiparisId}", siparisId);
        }
        catch (DomainException ex) when (_logger.IhlaliLogla(ex, nameof(SiparisSilAsync)))
        {
            throw;
        }
    }

    private Task TransactionIcindeAsync(Func<Task> islem, CancellationToken cancellationToken) =>
        TransactionIcindeAsync(async () =>
        {
            await islem();
            return true;
        }, cancellationToken);

    private async Task<T> TransactionIcindeAsync<T>(Func<Task<T>> islem, CancellationToken cancellationToken)
    {
        var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken: cancellationToken);
        T sonuc;
        try
        {
            sonuc = await islem();
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            // Asıl hata korunur: geri alma ya da kapatma hatası onu örtmemeli.
            // Commit edilmemiş transaction kapatılırken de geri alınır.
            try
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }
            catch (Exception rollbackHatasi)
            {
                _logger.LogWarning(
                    IslemGunlugu.Ayrinti(rollbackHatasi), "Transaction geri alınamadı. Hata={Hata}", IslemGunlugu.HataOzeti(rollbackHatasi));
            }

            try
            {
                await transaction.DisposeAsync();
            }
            catch (Exception kapatmaHatasi)
            {
                _logger.LogWarning(
                    IslemGunlugu.Ayrinti(kapatmaHatasi), "Transaction kapatılamadı. Hata={Hata}", IslemGunlugu.HataOzeti(kapatmaHatasi));
            }

            throw;
        }

        await transaction.DisposeAsync();
        return sonuc;
    }
}
