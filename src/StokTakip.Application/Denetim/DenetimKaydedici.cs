using StokTakip.Application.Kimlik;
using StokTakip.Domain.Entities;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Denetim;

public sealed class DenetimKaydedici : IDenetimKaydedici
{
    private readonly IDenetimRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOturumBaglami _oturum;

    public DenetimKaydedici(IDenetimRepository repository, IUnitOfWork unitOfWork, IOturumBaglami oturum)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _oturum = oturum ?? throw new ArgumentNullException(nameof(oturum));
    }

    public void Kaydet(DenetimOlayi olay)
    {
        ArgumentNullException.ThrowIfNull(olay);
        var aktor = olay.Aktor ?? Oturumdaki();
        _repository.Add(new DenetimKaydi(
            aktor.KullaniciId, aktor.KullaniciAdi, olay.Islem, olay.VarlikTuru, olay.VarlikId, olay.VarlikAdi,
            olay.EskiDeger, olay.YeniDeger, olay.Fark, olay.Ozet));
    }

    public async Task KaydetVeYazAsync(DenetimOlayi olay, CancellationToken cancellationToken = default)
    {
        Kaydet(olay);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private DenetimAktoru Oturumdaki() =>
        _oturum.MevcutKullanici is { } k
            ? new DenetimAktoru(k.KullaniciId, k.KullaniciAdi)
            : new DenetimAktoru(null, DenetimKaydi.SistemKullaniciAdi);
}
