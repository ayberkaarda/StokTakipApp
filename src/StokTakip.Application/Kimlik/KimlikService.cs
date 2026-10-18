using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StokTakip.Application.Denetim;
using StokTakip.Application.Services;
using StokTakip.Domain;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Kimlik;

/// <remarks>Parolalar, özetler ve kullanıcı adları günlüğe yazılmaz; yalnızca KullaniciId yazılır.</remarks>
public sealed class KimlikService : IKimlikService
{
    /// <summary>Eşzamanlı ilk kurulumlardan kaybedene verilen mesaj.</summary>
    public const string KurulumTamamlandiMesaji = "Kurulum zaten tamamlandı.";

    private const string KullaniciKayitTuru = "Kullanıcı";
    private const int MaksimumKayitDenemesi = 10;

    private readonly IKullaniciRepository _kullaniciRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IParolaHasher _parolaHasher;
    private readonly IOturumBaglami _oturum;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<KimlikService> _logger;
    private readonly IDenetimKaydedici _denetim;

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
        : this(kullaniciRepository, unitOfWork, parolaHasher, oturum, timeProvider, logger, NullDenetimKaydedici.Instance)
    {
    }

    public KimlikService(
        IKullaniciRepository kullaniciRepository,
        IUnitOfWork unitOfWork,
        IParolaHasher parolaHasher,
        IOturumBaglami oturum,
        TimeProvider timeProvider,
        ILogger<KimlikService> logger,
        IDenetimKaydedici denetim)
    {
        _denetim = denetim ?? throw new ArgumentNullException(nameof(denetim));
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
            var kullanici = new Kullanici(ad, ozet.Hash, ozet.Tuz, ozet.Iterasyon, KullaniciRolu.Admin);
            try
            {
                // Sayım ve ekleme aynı Serializable transaction'da: eşzamanlı ikinci kurulum ya burada bekleyip
                // kullanıcıyı görür ya da kilitlenme/benzersizlik hatası alır; ikisi de aynı mesaja çevrilir.
                await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                if (await _kullaniciRepository.HerhangiBiriVarMiAsync(cancellationToken))
                {
                    throw new GecersizIslemException(KurulumTamamlandiMesaji);
                }

                await _kullaniciRepository.AddAsync(kullanici, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _denetim.KaydetVeYazAsync(
                    new DenetimOlayi(
                        DenetimIslemi.IlkAdminOlusturuldu, DenetimVarlikTurleri.Kullanici,
                        $"İlk yönetici {kullanici.KullaniciAdi} oluşturuldu", kullanici.KullaniciId, kullanici.KullaniciAdi,
                        YeniDeger: $"Rol={KullaniciRolu.Admin}"),
                    cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is not (DomainException or OperationCanceledException))
            {
                if (await KurulumYapilmisMiAsync())
                {
                    throw new GecersizIslemException(KurulumTamamlandiMesaji);
                }

                throw;
            }

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
        var baslangicNesli = _oturum.Anlik.Nesil;
        (int Id, string Ad)? bulunan = null;
        try
        {
            try
            {
                return await GirisDongusuAsync();
            }
            catch (GecersizKimlikBilgisiException)
            {
                await BasarisizGirisKaydetAsync(null, bulunan, cancellationToken);
                throw;
            }
        }
        catch (DomainException ex) when (_logger.IhlaliLogla(ex, nameof(GirisYapAsync)))
        {
            throw;
        }

        async Task<KullaniciDto> GirisDongusuAsync()
        {
            for (var deneme = 1; ; deneme++)
            {
                var kullanici = Kullanici.KullaniciAdiNormallestir(kullaniciAdi, out var ad)
                    ? await _kullaniciRepository.GetByKullaniciAdiAsync(ad, cancellationToken)
                    : null;
                bulunan = kullanici is null ? null : (kullanici.KullaniciId, kullanici.KullaniciAdi);
                if (kullanici is null)
                {
                    _parolaHasher.SahteDogrula(parola);
                    throw new GecersizKimlikBilgisiException();
                }

                var simdi = SimdiUtc;
                var kilitli = kullanici.KilitliMi(simdi);

                // Kilitli hesapta da aynı maliyetli doğrulama yapılır; sonuç kullanılmaz, yanıt türü ve süresi ayırt edilemez.
                var sonuc = _parolaHasher.Dogrula(parola, kullanici.ParolaHash, kullanici.ParolaTuzu, kullanici.Iterasyon);
                if (kilitli)
                {
                    _logger.LogWarning("Kilitli hesaba giriş denemesi reddedildi. KullaniciId={KullaniciId}", kullanici.KullaniciId);
                    throw new GecersizKimlikBilgisiException();
                }

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

                    if (_oturum.Anlik.Nesil != baslangicNesli)
                    {
                        throw GirisIptal(kullanici.KullaniciId);
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

                if (!_oturum.OturumAc(new OturumKullanicisi(kullanici.KullaniciId, kullanici.KullaniciAdi, kullanici.Rol), baslangicNesli))
                {
                    throw GirisIptal(kullanici.KullaniciId);
                }

                // Kaydı yazılamayan giriş geçerli sayılmaz: açılan oturum geri kapatılır.
                try
                {
                    await _denetim.KaydetVeYazAsync(
                        new DenetimOlayi(
                            DenetimIslemi.GirisBasarili, DenetimVarlikTurleri.Oturum, $"{kullanici.KullaniciAdi} giriş yaptı",
                            kullanici.KullaniciId, kullanici.KullaniciAdi)
                        {
                            Aktor = new DenetimAktoru(kullanici.KullaniciId, kullanici.KullaniciAdi),
                        },
                        cancellationToken);
                }
                catch
                {
                    _oturum.OturumuKapat(baslangicNesli + 1);
                    throw;
                }

                _logger.LogInformation("Giriş yapıldı. KullaniciId={KullaniciId}", kullanici.KullaniciId);
                return KullaniciDto.Olustur(kullanici);
            }
        }
    }

    /// <remarks>Yalnızca bulunan hesabın kimliği kaydedilir. Kayıt yazılamazsa asıl hata korunur.</remarks>
    private async Task BasarisizGirisKaydetAsync(string? denenenAd, (int Id, string Ad)? bulunan, CancellationToken cancellationToken)
    {
        var olay = bulunan is { } k
            ? new DenetimOlayi(
                DenetimIslemi.GirisBasarisiz, DenetimVarlikTurleri.Oturum, $"{k.Ad} için başarısız giriş denemesi", k.Id, k.Ad)
            {
                Aktor = new DenetimAktoru(k.Id, k.Ad),
            }
            : new DenetimOlayi(
                DenetimIslemi.GirisBasarisiz, DenetimVarlikTurleri.Oturum,
                "Bilinmeyen kullanıcı adıyla başarısız giriş denemesi")
            {
                Aktor = new DenetimAktoru(null, "(bilinmeyen)"),
            };

        try
        {
            await _denetim.KaydetVeYazAsync(olay, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Başarısız giriş denetim kaydına yazılamadı.");
        }
    }

    public void CikisYap() => _oturum.OturumuKapat();

    public async Task CikisYapAsync(CancellationToken cancellationToken = default)
    {
        OturumAnligi anlik;
        do
        {
            anlik = _oturum.Anlik;
        }
        while (!_oturum.OturumuKapat(anlik.Nesil));

        if (anlik.Kullanici is { } k)
        {
            await _denetim.KaydetVeYazAsync(
                new DenetimOlayi(DenetimIslemi.CikisYapildi, DenetimVarlikTurleri.Oturum, $"{k.KullaniciAdi} çıkış yaptı", k.KullaniciId, k.KullaniciAdi)
                {
                    Aktor = new DenetimAktoru(k.KullaniciId, k.KullaniciAdi),
                },
                cancellationToken);
        }
    }

    public async Task<KullaniciDto> KullaniciOlusturAsync(
        string kullaniciAdi, string parola, KullaniciRolu rol, CancellationToken cancellationToken = default)
    {
        try
        {
            var cagiran = await AdminDogrulaAsync(cancellationToken);
            var ad = Kullanici.KullaniciAdiDogrula(kullaniciAdi);
            RolDogrula(rol);
            ParolaPolitikasi.Dogrula(parola, ad);
            if (await _kullaniciRepository.KullaniciAdiKullaniliyorMuAsync(ad, cancellationToken))
            {
                throw new GecersizGirdiException($"'{ad}' kullanıcı adı zaten kullanılıyor.", nameof(Kullanici.KullaniciAdi));
            }

            var ozet = _parolaHasher.Hashle(parola);
            var kullanici = new Kullanici(ad, ozet.Hash, ozet.Tuz, ozet.Iterasyon, rol);
            await using (var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken: cancellationToken))
            {
                await _kullaniciRepository.AddAsync(kullanici, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _denetim.KaydetVeYazAsync(
                    new DenetimOlayi(
                        DenetimIslemi.KullaniciOlusturuldu, DenetimVarlikTurleri.Kullanici,
                        $"{kullanici.KullaniciAdi} kullanıcısı oluşturuldu (rol: {rol})", kullanici.KullaniciId, kullanici.KullaniciAdi,
                        YeniDeger: $"Rol={rol}"),
                    cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }

            _logger.LogInformation(
                "Kullanıcı oluşturuldu. KullaniciId={KullaniciId}, Rol={Rol}, Olusturan={CagiranId}",
                kullanici.KullaniciId, rol, cagiran.KullaniciId);
            return KullaniciDto.Olustur(kullanici);
        }
        catch (DomainException ex) when (_logger.IhlaliLogla(ex, nameof(KullaniciOlusturAsync)))
        {
            throw;
        }
    }

    public async Task ParolaDegistirAsync(string eskiParola, string yeniParola, CancellationToken cancellationToken = default)
    {
        try
        {
            var oturumdaki = await GuncelYetkiDogrulama.DogrulaAsync(_oturum, _kullaniciRepository, KullaniciRolu.Operator, cancellationToken);
            var kullanici = await KullaniciyiBulAsync(oturumdaki.KullaniciId, cancellationToken);
            ParolaPolitikasi.Dogrula(yeniParola, kullanici.KullaniciAdi);

            var simdi = SimdiUtc;
            if (kullanici.KilitliMi(simdi))
            {
                throw new HesapKilitliException(kullanici.KilitBitis!.Value);
            }

            var sonuc = _parolaHasher.Dogrula(eskiParola, kullanici.ParolaHash, kullanici.ParolaTuzu, kullanici.Iterasyon);
            if (sonuc == ParolaDogrulamaSonucu.Basarisiz)
            {
                kullanici.BasarisizGirisKaydet(simdi);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                throw new GecersizGirdiException("Mevcut parola hatalı.", "EskiParola");
            }

            var ozet = _parolaHasher.Hashle(yeniParola);
            kullanici.ParolayiAyarla(ozet.Hash, ozet.Tuz, ozet.Iterasyon);
            kullanici.BasariliGirisKaydet(simdi);
            _denetim.Kaydet(new DenetimOlayi(
                DenetimIslemi.ParolaDegisti, DenetimVarlikTurleri.Kullanici, $"{kullanici.KullaniciAdi} parolasını değiştirdi",
                kullanici.KullaniciId, kullanici.KullaniciAdi));
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Parola değiştirildi. KullaniciId={KullaniciId}", kullanici.KullaniciId);
        }
        catch (DomainException ex) when (_logger.IhlaliLogla(ex, nameof(ParolaDegistirAsync)))
        {
            throw;
        }
    }

    public async Task<IReadOnlyList<KullaniciDto>> KullaniciListeleAsync(CancellationToken cancellationToken = default)
    {
        await AdminDogrulaAsync(cancellationToken);
        var kullanicilar = await _kullaniciRepository.GetAllAsync(cancellationToken);
        return kullanicilar.Select(KullaniciDto.Olustur).ToList();
    }

    public async Task<KullaniciDto> RolDegistirAsync(int kullaniciId, KullaniciRolu yeniRol, CancellationToken cancellationToken = default)
    {
        try
        {
            RolDogrula(yeniRol);
            await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var cagiran = await AdminDogrulaAsync(cancellationToken);
            var hedef = await KullaniciyiBulAsync(kullaniciId, cancellationToken);
            if (hedef is { Aktif: true, Rol: KullaniciRolu.Admin } && yeniRol != KullaniciRolu.Admin)
            {
                await SonAdminKorumasiAsync(cancellationToken);
            }

            var eskiRol = hedef.Rol;
            hedef.RolAta(yeniRol);
            _denetim.Kaydet(new DenetimOlayi(
                DenetimIslemi.RolDegisti, DenetimVarlikTurleri.Kullanici, $"{hedef.KullaniciAdi} rolü {eskiRol} → {yeniRol}",
                hedef.KullaniciId, hedef.KullaniciAdi, eskiRol.ToString(), yeniRol.ToString()));
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            OturumuEsitle(hedef);
            _logger.LogInformation(
                "Kullanıcı rolü değişti. KullaniciId={KullaniciId}, Rol={Rol}, Degistiren={CagiranId}",
                kullaniciId, yeniRol, cagiran.KullaniciId);
            return KullaniciDto.Olustur(hedef);
        }
        catch (DomainException ex) when (_logger.IhlaliLogla(ex, nameof(RolDegistirAsync)))
        {
            throw;
        }
    }

    public async Task<KullaniciDto> AktiflikDegistirAsync(int kullaniciId, bool aktif, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var cagiran = await AdminDogrulaAsync(cancellationToken);
            var hedef = await KullaniciyiBulAsync(kullaniciId, cancellationToken);
            if (!aktif && hedef is { Aktif: true, Rol: KullaniciRolu.Admin })
            {
                await SonAdminKorumasiAsync(cancellationToken);
            }

            var eski = AktiflikMetni(hedef.Aktif);
            hedef.AktiflikAyarla(aktif);
            _denetim.Kaydet(new DenetimOlayi(
                DenetimIslemi.AktiflikDegisti, DenetimVarlikTurleri.Kullanici, $"{hedef.KullaniciAdi} hesabı {eski} → {AktiflikMetni(aktif)}",
                hedef.KullaniciId, hedef.KullaniciAdi, eski, AktiflikMetni(aktif)));
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            OturumuEsitle(hedef);
            _logger.LogInformation(
                "Kullanıcı aktifliği değişti. KullaniciId={KullaniciId}, Aktif={Aktif}, Degistiren={CagiranId}",
                kullaniciId, aktif, cagiran.KullaniciId);
            return KullaniciDto.Olustur(hedef);
        }
        catch (DomainException ex) when (_logger.IhlaliLogla(ex, nameof(AktiflikDegistirAsync)))
        {
            throw;
        }
    }

    private async Task<bool> KurulumYapilmisMiAsync()
    {
        try
        {
            return await _kullaniciRepository.HerhangiBiriVarMiAsync(CancellationToken.None);
        }
        catch (Exception kontrolHatasi)
        {
            _logger.LogWarning(kontrolHatasi, "İlk kurulum hatası sonrası kullanıcı denetimi yapılamadı.");
            return false;
        }
    }

    private Task<OturumKullanicisi> AdminDogrulaAsync(CancellationToken cancellationToken)
        => GuncelYetkiDogrulama.DogrulaAsync(_oturum, _kullaniciRepository, KullaniciRolu.Admin, cancellationToken);

    private OperationCanceledException GirisIptal(int kullaniciId)
    {
        _logger.LogInformation("Giriş, oturum durumu değiştiği için iptal edildi. KullaniciId={KullaniciId}", kullaniciId);
        return new OperationCanceledException("Giriş sırasında oturum durumu değişti; giriş iptal edildi.");
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
        var anlik = _oturum.Anlik;
        if (anlik.Kullanici?.KullaniciId != kullanici.KullaniciId)
        {
            return;
        }

        if (kullanici.Aktif)
        {
            _oturum.OturumAc(new OturumKullanicisi(kullanici.KullaniciId, kullanici.KullaniciAdi, kullanici.Rol), anlik.Nesil);
        }
        else
        {
            _oturum.OturumuKapat(anlik.Nesil);
        }
    }

    private static string AktiflikMetni(bool aktif) => aktif ? "Aktif" : "Pasif";

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
