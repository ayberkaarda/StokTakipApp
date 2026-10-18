using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.Kimlik;

/// <summary>
/// Bir işlemin (DI kapsamının) oturum anlığı: ilk yetki denetiminde sabitlenir. Yetki doğrulama ve denetim kaydı
/// aynı anlığı kullanır; işlem sürerken çıkış/yeni giriş olursa işlem eski kullanıcıyla sürdürülmez. Scoped kaydedilir.
/// </summary>
public sealed class IslemOturumu
{
    private readonly IOturumBaglami _oturum;
    private OturumAnligi? _sabitlenen;

    public IslemOturumu(IOturumBaglami oturum)
    {
        _oturum = oturum ?? throw new ArgumentNullException(nameof(oturum));
    }

    public OturumAnligi? Sabitlenen => _sabitlenen;

    public bool NesilDegisti => _sabitlenen is { } anlik && _oturum.Anlik.Nesil != anlik.Nesil;

    public OturumAnligi Sabitle() => _sabitlenen ??= _oturum.Anlik;

    /// <exception cref="YetkisizIslemException">Oturum sabitlendikten sonra kapandı ya da değiştiyse.</exception>
    public void NesilDegismediyseDevam()
    {
        if (NesilDegisti)
        {
            throw new YetkisizIslemException(YetkisizIslemException.OturumDegistiMesaji);
        }
    }

    internal void Birak() => _sabitlenen = null;
}
