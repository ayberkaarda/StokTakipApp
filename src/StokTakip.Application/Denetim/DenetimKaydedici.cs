using StokTakip.Application.Kimlik;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Denetim;

public sealed class DenetimKaydedici : IDenetimKaydedici
{
    private readonly IDenetimRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOturumBaglami _oturum;
    private readonly IslemOturumu _islem;

    public DenetimKaydedici(IDenetimRepository repository, IUnitOfWork unitOfWork, IOturumBaglami oturum)
        : this(repository, unitOfWork, oturum, new IslemOturumu(oturum ?? throw new ArgumentNullException(nameof(oturum))))
    {
    }

    /// <param name="islem">Yetki denetçisiyle paylaşılan işlem anlığı: kayıt işlemi başlatan aktörle yazılır.</param>
    public DenetimKaydedici(IDenetimRepository repository, IUnitOfWork unitOfWork, IOturumBaglami oturum, IslemOturumu islem)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _oturum = oturum ?? throw new ArgumentNullException(nameof(oturum));
        _islem = islem ?? throw new ArgumentNullException(nameof(islem));
    }

    /// <exception cref="Domain.Exceptions.YetkisizIslemException">
    /// İşlem başında sabitlenen oturum değiştiyse; kayıt eklenmez, işlem kaydetmeden önce iptal olur.
    /// </exception>
    public void Kaydet(DenetimOlayi olay)
    {
        ArgumentNullException.ThrowIfNull(olay);
        var aktor = olay.Aktor ?? IslemAktoru(olay.IslemSonrasi);
        _repository.Add(new DenetimKaydi(
            aktor.KullaniciId, aktor.KullaniciAdi, olay.Islem, olay.VarlikTuru, olay.VarlikId, olay.VarlikAdi,
            olay.EskiDeger, olay.YeniDeger, olay.Fark, olay.Ozet));
    }

    public async Task KaydetVeYazAsync(DenetimOlayi olay, CancellationToken cancellationToken = default)
    {
        Kaydet(olay);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private DenetimAktoru IslemAktoru(bool islemSonrasi)
    {
        if (_islem.Sabitlenen is not { } anlik)
        {
            return Aktor(_oturum.MevcutKullanici);
        }

        if (!islemSonrasi)
        {
            _islem.NesilDegismediyseDevam();
        }

        return Aktor(anlik.Kullanici);
    }

    private static DenetimAktoru Aktor(OturumKullanicisi? k) =>
        k is null ? new DenetimAktoru(null, DenetimKaydi.SistemKullaniciAdi) : new DenetimAktoru(k.KullaniciId, k.KullaniciAdi);
}
