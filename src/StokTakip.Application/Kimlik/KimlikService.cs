using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StokTakip.Application.Services;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Kimlik;

/// <remarks>Parolalar, özetler ve kullanıcı adları günlüğe yazılmaz; yalnızca KullaniciId yazılır.</remarks>
public sealed class KimlikService : IKimlikService
{
    private const string KullaniciKayitTuru = "Kullanıcı";
    private const int MaksimumKayitDenemesi = 10;

    private readonly IKullaniciRepository _kullaniciRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IParolaHasher _parolaHasher;
    private readonly IOturumBaglami _oturum;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<KimlikService> _logger;

    public KimlikService(
        IKullaniciRepository kullaniciRepository,
        IUnitOfWork unitOfWork,
        IParolaHasher parolaHasher,
        IOturumBaglami oturum,
        TimeProvider timeProvider)
        : this(kullaniciRepository, unitOfWork, parolaHasher, oturum, timeProvider, NullLogger<KimlikService>.Instance)
    {
    }

    public KimlikService(
        IKullaniciRepository kullaniciRepository,
        IUnitOfWork unitOfWork,
        IParolaHasher parolaHasher,
        IOturumBaglami oturum,
        TimeProvider timeProvider,
        ILogger<KimlikService> logger)
    {
        _kullaniciRepository = kullaniciRepository ?? throw new ArgumentNullException(nameof(kullaniciRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _parolaHasher = parolaHasher ?? throw new ArgumentNullException(nameof(parolaHasher));
        _oturum = oturum ?? throw new ArgumentNullException(nameof(oturum));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private DateTime SimdiUtc => _timeProvider.GetUtcNow().UtcDateTime;

    public async Task<bool> IlkKurulumGerekliMiAsync(CancellationToken cancellationToken = default)
        => !await _kullaniciRepository.HerhangiBiriVarMiAsync(cancellationToken);

    public async Task<KullaniciDto> IlkAdminOlusturAsync(string kullaniciAdi, string parola, CancellationToken cancellationToken = default)
    {
        try
        {
            var ad = Kullanici.KullaniciAdiDogrula(kullaniciAdi);
            ParolaPolitikasi.Dogrula(parola, ad);
            if (await _kullaniciRepository.HerhangiBiriVarMiAsync(cancellationToken))
            {
                throw IlkKurulumTamamlandi();
            }

            var ozet = _parolaHasher.Hashle(parola);

            await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            if (await _kullaniciRepository.HerhangiBiriVarMiAsync(cancellationToken))
            {
                throw IlkKurulumTamamlandi();
            }

            var kullanici = new Kullanici(ad, ozet.Hash, ozet.Tuz, ozet.Iterasyon, KullaniciRolu.Admin);
            await _kullaniciRepository.AddAsync(kullanici, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("İlk yönetici oluşturuldu. KullaniciId={KullaniciId}", kullanici.KullaniciId);
            return KullaniciDto.Olustur(kullanici);
        }
        catch (DomainException ex) when (_logger.IhlaliLogla(ex, nameof(IlkAdminOlusturAsync)))
        {
            throw;
        }
    }

    public async Task<KullaniciDto> GirisYapAsync(string kullaniciAdi, string parola, CancellationToken cancellationToken = default)
    {
        try
        {
            for (var deneme = 1; ; deneme++)
            {
                var kullanici = Kullanici.KullaniciAdiNormallestir(kullaniciAdi, out var ad)
                    ? await _kullaniciRepository.GetByKullaniciAdiAsync(ad, cancellationToken)
                    : null;
                if (kullanici is null)
                {
                    _parolaHasher.SahteDogrula(parola);
                    throw new GecersizKimlikBilgisiException();
                }

                var simdi = SimdiUtc;
                if (kullanici.KilitliMi(simdi))
                {
                    throw new HesapKilitliException(kullanici.KilitBitis!.Value);
                }

                var sonuc = _parolaHasher.Dogrula(parola, kullanici.ParolaHash, kullanici.ParolaTuzu, kullanici.Iterasyon);
                if (!kullanici.Aktif)
                {
                    throw new GecersizKimlikBilgisiException();
                }

                try
                {
                    if (sonuc == ParolaDogrulamaSonucu.Basarisiz)
                    {
                        var kilitlendi = kullanici.BasarisizGirisKaydet(simdi);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                        if (kilitlendi)
                        {
                            _logger.LogWarning("Hesap kilitlendi. KullaniciId={KullaniciId}", kullanici.KullaniciId);
                        }

                        throw new GecersizKimlikBilgisiException();
                    }

                    if (sonuc == ParolaDogrulamaSonucu.BasariliYenidenHashGerekli)
                    {
                        var yeni = _parolaHasher.Hashle(parola);
                        kullanici.ParolayiAyarla(yeni.Hash, yeni.Tuz, yeni.Iterasyon);
                    }

                    kullanici.BasariliGirisKaydet(simdi);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                catch (EszamanlilikException) when (deneme < MaksimumKayitDenemesi)
                {
                    continue;
                }

                _oturum.OturumAc(new OturumKullanicisi(kullanici.KullaniciId, kullanici.KullaniciAdi, kullanici.Rol));
                _logger.LogInformation("Giriş yapıldı. KullaniciId={KullaniciId}", kullanici.KullaniciId);
                return KullaniciDto.Olustur(kullanici);
            }
        }
        catch (DomainException ex) when (_logger.IhlaliLogla(ex, nameof(GirisYapAsync)))
        {
            throw;
        }
    }

    public void CikisYap() => _oturum.OturumuKapat();

    public async Task<KullaniciDto> KullaniciOlusturAsync(
        int cagiranKullaniciId, string kullaniciAdi, string parola, KullaniciRolu rol, CancellationToken cancellationToken = default)
    {
        try
        {
            await AdminDogrulaAsync(cagiranKullaniciId, cancellationToken);
            var ad = Kullanici.KullaniciAdiDogrula(kullaniciAdi);
            RolDogrula(rol);
            ParolaPolitikasi.Dogrula(parola, ad);
            if (await _kullaniciRepository.KullaniciAdiKullaniliyorMuAsync(ad, cancellationToken))
            {
                throw new GecersizGirdiException($"'{ad}' kullanıcı adı zaten kullanılıyor.", nameof(Kullanici.KullaniciAdi));
            }

            var ozet = _parolaHasher.Hashle(parola);
            var kullanici = new Kullanici(ad, ozet.Hash, ozet.Tuz, ozet.Iterasyon, rol);
            await _kullaniciRepository.AddAsync(kullanici, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Kullanıcı oluşturuldu. KullaniciId={KullaniciId}, Rol={Rol}, Olusturan={CagiranId}",
                kullanici.KullaniciId, rol, cagiranKullaniciId);
            return KullaniciDto.Olustur(kullanici);
        }
        catch (DomainException ex) when (_logger.IhlaliLogla(ex, nameof(KullaniciOlusturAsync)))
        {
            throw;
        }
    }

    public async Task ParolaDegistirAsync(int kullaniciId, string eskiParola, string yeniParola, CancellationToken cancellationToken = default)
    {
        try
        {
            var kullanici = await KullaniciyiBulAsync(kullaniciId, cancellationToken);
            ParolaPolitikasi.Dogrula(yeniParola, kullanici.KullaniciAdi);

            var simdi = SimdiUtc;
            if (kullanici.KilitliMi(simdi))
            {
                throw new HesapKilitliException(kullanici.KilitBitis!.Value);
            }

            var sonuc = _parolaHasher.Dogrula(eskiParola, kullanici.ParolaHash, kullanici.ParolaTuzu, kullanici.Iterasyon);
            if (!kullanici.Aktif)
            {
                throw new GecersizKimlikBilgisiException();
            }

            if (sonuc == ParolaDogrulamaSonucu.Basarisiz)
            {
                kullanici.BasarisizGirisKaydet(simdi);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                throw new GecersizGirdiException("Mevcut parola hatalı.", "EskiParola");
            }

            var ozet = _parolaHasher.Hashle(yeniParola);
            kullanici.ParolayiAyarla(ozet.Hash, ozet.Tuz, ozet.Iterasyon);
            kullanici.BasariliGirisKaydet(simdi);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Parola değiştirildi. KullaniciId={KullaniciId}", kullaniciId);
        }
        catch (DomainException ex) when (_logger.IhlaliLogla(ex, nameof(ParolaDegistirAsync)))
        {
            throw;
        }
    }

    public async Task<IReadOnlyList<KullaniciDto>> KullaniciListeleAsync(int cagiranKullaniciId, CancellationToken cancellationToken = default)
    {
        await AdminDogrulaAsync(cagiranKullaniciId, cancellationToken);
        var kullanicilar = await _kullaniciRepository.GetAllAsync(cancellationToken);
        return kullanicilar.Select(KullaniciDto.Olustur).ToList();
    }

    public async Task<KullaniciDto> RolDegistirAsync(
        int cagiranKullaniciId, int kullaniciId, KullaniciRolu yeniRol, CancellationToken cancellationToken = default)
    {
        try
        {
            RolDogrula(yeniRol);
            await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            await AdminDogrulaAsync(cagiranKullaniciId, cancellationToken);
            var hedef = await KullaniciyiBulAsync(kullaniciId, cancellationToken);
            if (hedef is { Aktif: true, Rol: KullaniciRolu.Admin } && yeniRol != KullaniciRolu.Admin)
            {
                await SonAdminKorumasiAsync(cancellationToken);
            }

            hedef.RolAta(yeniRol);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            OturumuEsitle(hedef);
            _logger.LogInformation(
                "Kullanıcı rolü değişti. KullaniciId={KullaniciId}, Rol={Rol}, Degistiren={CagiranId}",
                kullaniciId, yeniRol, cagiranKullaniciId);
            return KullaniciDto.Olustur(hedef);
        }
        catch (DomainException ex) when (_logger.IhlaliLogla(ex, nameof(RolDegistirAsync)))
        {
            throw;
        }
    }

    public async Task<KullaniciDto> AktiflikDegistirAsync(
        int cagiranKullaniciId, int kullaniciId, bool aktif, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            await AdminDogrulaAsync(cagiranKullaniciId, cancellationToken);
            var hedef = await KullaniciyiBulAsync(kullaniciId, cancellationToken);
            if (!aktif && hedef is { Aktif: true, Rol: KullaniciRolu.Admin })
            {
                await SonAdminKorumasiAsync(cancellationToken);
            }

            hedef.AktiflikAyarla(aktif);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            OturumuEsitle(hedef);
            _logger.LogInformation(
                "Kullanıcı aktifliği değişti. KullaniciId={KullaniciId}, Aktif={Aktif}, Degistiren={CagiranId}",
                kullaniciId, aktif, cagiranKullaniciId);
            return KullaniciDto.Olustur(hedef);
        }
        catch (DomainException ex) when (_logger.IhlaliLogla(ex, nameof(AktiflikDegistirAsync)))
        {
            throw;
        }
    }

    private async Task AdminDogrulaAsync(int cagiranKullaniciId, CancellationToken cancellationToken)
    {
        var cagiran = cagiranKullaniciId > 0
            ? await _kullaniciRepository.GetByIdAsync(cagiranKullaniciId, cancellationToken)
            : null;
        if (cagiran is not { Aktif: true, Rol: KullaniciRolu.Admin })
        {
            throw new YetkisizIslemException();
        }
    }

    private async Task SonAdminKorumasiAsync(CancellationToken cancellationToken)
    {
        if (await _kullaniciRepository.AktifAdminSayisiAsync(cancellationToken) <= 1)
        {
            throw new GecersizIslemException("Sistemde en az bir aktif yönetici (Admin) kalmalıdır.");
        }
    }

    private async Task<Kullanici> KullaniciyiBulAsync(int kullaniciId, CancellationToken cancellationToken)
    {
        var kullanici = kullaniciId > 0 ? await _kullaniciRepository.GetByIdAsync(kullaniciId, cancellationToken) : null;
        return kullanici ?? throw new KayitBulunamadiException(KullaniciKayitTuru, kullaniciId);
    }

    private void OturumuEsitle(Kullanici kullanici)
    {
        if (_oturum.MevcutKullanici?.KullaniciId != kullanici.KullaniciId)
        {
            return;
        }

        if (kullanici.Aktif)
        {
            _oturum.OturumAc(new OturumKullanicisi(kullanici.KullaniciId, kullanici.KullaniciAdi, kullanici.Rol));
        }
        else
        {
            _oturum.OturumuKapat();
        }
    }

    private static void RolDogrula(KullaniciRolu rol)
    {
        if (!Enum.IsDefined(rol))
        {
            throw new GecersizGirdiException("Geçersiz kullanıcı rolü.", nameof(Kullanici.Rol));
        }
    }

    private static GecersizIslemException IlkKurulumTamamlandi() =>
        new("İlk kurulum zaten tamamlanmış; yeni kullanıcıları bir yönetici oluşturabilir.");
}
