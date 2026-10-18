using StokTakip.Domain;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Kimlik;

public enum Yetki
{
    UrunGoruntule,
    UrunEkleGuncelle,
    StokGuncelle,
    SiparisOlustur,
    GecmisVeRaporGoruntule,
    UrunSil,
    SiparisSil,
    KategoriYonetimi,
    KullaniciYonetimi,
    CsvAktarim,

    /// <summary>Düşük stok eşiğini belirleme/değiştirme (Operator yeni ürünü varsayılan eşikle ekler).</summary>
    EsikDuzenle,
    DenetimGoruntule,
}

public static class YetkiKurallari
{
    public static KullaniciRolu GerekliRol(Yetki yetki) => yetki switch
    {
        Yetki.UrunGoruntule or Yetki.UrunEkleGuncelle or Yetki.StokGuncelle
            or Yetki.SiparisOlustur or Yetki.GecmisVeRaporGoruntule => KullaniciRolu.Operator,
        Yetki.UrunSil or Yetki.SiparisSil or Yetki.KategoriYonetimi
            or Yetki.KullaniciYonetimi or Yetki.CsvAktarim
            or Yetki.EsikDuzenle or Yetki.DenetimGoruntule => KullaniciRolu.Admin,
        _ => throw new ArgumentOutOfRangeException(nameof(yetki)),
    };

    /// <summary>Hassas (Admin) işlemlerde oturumdaki rol yetmez; kullanıcı kaydı veritabanından yeniden doğrulanır.</summary>
    public static bool HassasMi(Yetki yetki) => GerekliRol(yetki) == KullaniciRolu.Admin;
}

public interface IYetkiDenetcisi
{
    /// <exception cref="YetkisizIslemException"/>
    Task GerektirAsync(Yetki yetki, CancellationToken cancellationToken = default);

    /// <summary>Yetki yoksa false döner; oturum yoksa ya da hesap artık geçerli değilse yine reddeder.</summary>
    /// <exception cref="YetkisizIslemException"/>
    async Task<bool> YetkiliMiAsync(Yetki yetki, CancellationToken cancellationToken = default)
    {
        try
        {
            await GerektirAsync(yetki, cancellationToken);
            return true;
        }
        catch (YetkisizIslemException ex) when (ex.Message == YetkisizIslemException.YetkiYokMesaji)
        {
            return false;
        }
    }

    /// <summary>Önbelleği atlayarak güncel kayıtla yeniden doğrular (uzun işlemlerin sonunda).</summary>
    /// <exception cref="YetkisizIslemException"/>
    Task YenidenDogrulaAsync(Yetki yetki, CancellationToken cancellationToken = default) => GerektirAsync(yetki, cancellationToken);

    /// <summary>İşlem bitince çağrılır: oturum işlem sırasında değiştiyse sonucu geçersiz sayar.</summary>
    /// <exception cref="YetkisizIslemException"/>
    void IslemSonuDogrula()
    {
    }
}

/// <summary>Denetim yapmaz; servislerin oturumdan bağımsız kullanıldığı testler ve araçlar içindir.</summary>
public sealed class YetkiDenetimiYok : IYetkiDenetcisi
{
    public static readonly YetkiDenetimiYok Instance = new();

    public Task GerektirAsync(Yetki yetki, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

/// <summary>
/// Üretim denetçisi: oturum yoksa her işlemi reddeder. Her yetkide (okuma ve Operator işlemleri dahil) kullanıcının güncel
/// kaydını okur; kayıt yok, pasif ya da rolü yetmiyorsa oturumu kapatır ve reddeder. Kayıt bir işlemde bir kez okunur.
/// </summary>
public sealed class OturumYetkiDenetcisi : IYetkiDenetcisi
{
    private readonly IOturumBaglami _oturum;
    private readonly IKullaniciRepository _kullanicilar;
    private readonly IslemOturumu _islem;
    private readonly bool _kapsamBoyu;
    private KullaniciYetkiBilgisi? _dogrulanan;

    /// <summary>Her çağrı ayrı bir işlemdir: oturum o çağrının başında sabitlenir ve kayıt yeniden okunur.</summary>
    public OturumYetkiDenetcisi(IOturumBaglami oturum, IKullaniciRepository kullanicilar)
        : this(oturum, kullanicilar, new IslemOturumu(oturum ?? throw new ArgumentNullException(nameof(oturum))), kapsamBoyu: false)
    {
    }

    /// <summary>
    /// Kapsam (DI scope) tek işlemdir: oturum ilk denetimde sabitlenir, kayıt bir kez okunur; oturum sonradan değişirse
    /// her denetim reddedilir. Aynı <paramref name="islem"/> denetim kaydedicisiyle paylaşılmalıdır.
    /// </summary>
    public OturumYetkiDenetcisi(IOturumBaglami oturum, IKullaniciRepository kullanicilar, IslemOturumu islem)
        : this(oturum, kullanicilar, islem, kapsamBoyu: true)
    {
    }

    private OturumYetkiDenetcisi(IOturumBaglami oturum, IKullaniciRepository kullanicilar, IslemOturumu islem, bool kapsamBoyu)
    {
        _oturum = oturum ?? throw new ArgumentNullException(nameof(oturum));
        _kullanicilar = kullanicilar ?? throw new ArgumentNullException(nameof(kullanicilar));
        _islem = islem ?? throw new ArgumentNullException(nameof(islem));
        _kapsamBoyu = kapsamBoyu;
    }

    public Task GerektirAsync(Yetki yetki, CancellationToken cancellationToken = default)
        => DogrulaAsync(YetkiKurallari.GerekliRol(yetki), Anlik(), yenidenOku: false, cancellationToken);

    public Task YenidenDogrulaAsync(Yetki yetki, CancellationToken cancellationToken = default)
        => DogrulaAsync(YetkiKurallari.GerekliRol(yetki), _islem.Sabitle(), yenidenOku: true, cancellationToken);

    public void IslemSonuDogrula() => _islem.NesilDegismediyseDevam();

    public async Task<bool> YetkiliMiAsync(Yetki yetki, CancellationToken cancellationToken = default)
    {
        var gerekliRol = YetkiKurallari.GerekliRol(yetki);
        var anlik = Anlik();
        var kullanici = anlik.Kullanici ?? throw new YetkisizIslemException(YetkisizIslemException.OturumGerekliMesaji);
        if (kullanici.Rol < gerekliRol)
        {
            return false;
        }

        await DogrulaAsync(gerekliRol, anlik, yenidenOku: false, cancellationToken);
        return true;
    }

    private OturumAnligi Anlik()
    {
        if (!_kapsamBoyu)
        {
            _islem.Birak();
            _dogrulanan = null;
        }

        return _islem.Sabitle();
    }

    private async Task DogrulaAsync(KullaniciRolu gerekliRol, OturumAnligi anlik, bool yenidenOku, CancellationToken cancellationToken)
    {
        var kullanici = anlik.Kullanici ?? throw new YetkisizIslemException(YetkisizIslemException.OturumGerekliMesaji);
        _islem.NesilDegismediyseDevam();
        if (kullanici.Rol < gerekliRol)
        {
            throw new YetkisizIslemException();
        }

        if (_dogrulanan is null || yenidenOku)
        {
            _dogrulanan = null;
            var guncel = await _kullanicilar.YetkiBilgisiGetirAsync(kullanici.KullaniciId, cancellationToken);
            _islem.NesilDegismediyseDevam();
            if (guncel is not { Aktif: true })
            {
                _oturum.OturumuKapat(anlik.Nesil);
                throw new YetkisizIslemException();
            }

            _dogrulanan = guncel;
        }

        if (_dogrulanan.Rol < gerekliRol)
        {
            _oturum.OturumuKapat(anlik.Nesil);
            throw new YetkisizIslemException();
        }
    }
}
