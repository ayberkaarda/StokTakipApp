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
}

public static class YetkiKurallari
{
    public static KullaniciRolu GerekliRol(Yetki yetki) => yetki switch
    {
        Yetki.UrunGoruntule or Yetki.UrunEkleGuncelle or Yetki.StokGuncelle
            or Yetki.SiparisOlustur or Yetki.GecmisVeRaporGoruntule => KullaniciRolu.Operator,
        Yetki.UrunSil or Yetki.SiparisSil or Yetki.KategoriYonetimi
            or Yetki.KullaniciYonetimi or Yetki.CsvAktarim => KullaniciRolu.Admin,
        _ => throw new ArgumentOutOfRangeException(nameof(yetki)),
    };

    /// <summary>Hassas (Admin) işlemlerde oturumdaki rol yetmez; kullanıcı kaydı veritabanından yeniden doğrulanır.</summary>
    public static bool HassasMi(Yetki yetki) => GerekliRol(yetki) == KullaniciRolu.Admin;
}

public interface IYetkiDenetcisi
{
    /// <exception cref="YetkisizIslemException"/>
    Task GerektirAsync(Yetki yetki, CancellationToken cancellationToken = default);
}

/// <summary>Denetim yapmaz; servislerin oturumdan bağımsız kullanıldığı testler ve araçlar içindir.</summary>
public sealed class YetkiDenetimiYok : IYetkiDenetcisi
{
    public static readonly YetkiDenetimiYok Instance = new();

    public Task GerektirAsync(Yetki yetki, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

/// <summary>
/// Üretim denetçisi: oturum yoksa her işlemi reddeder. Hassas işlemlerde kullanıcının güncel kaydını okur;
/// kullanıcı yok, pasif ya da rolü yetmiyorsa oturumu kapatır ve reddeder. Scoped kaydedilir.
/// </summary>
public sealed class OturumYetkiDenetcisi(IOturumBaglami oturum, IKullaniciRepository kullanicilar) : IYetkiDenetcisi
{
    private readonly IOturumBaglami _oturum = oturum ?? throw new ArgumentNullException(nameof(oturum));
    private readonly IKullaniciRepository _kullanicilar = kullanicilar ?? throw new ArgumentNullException(nameof(kullanicilar));

    public Task GerektirAsync(Yetki yetki, CancellationToken cancellationToken = default)
    {
        var gerekliRol = YetkiKurallari.GerekliRol(yetki);
        if (!YetkiKurallari.HassasMi(yetki))
        {
            _oturum.YetkiGerektir(gerekliRol);
            return Task.CompletedTask;
        }

        return GuncelYetkiDogrulama.DogrulaAsync(_oturum, _kullanicilar, gerekliRol, cancellationToken);
    }
}
