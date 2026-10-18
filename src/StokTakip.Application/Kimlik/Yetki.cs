using StokTakip.Domain;
using StokTakip.Domain.Exceptions;

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
}

public interface IYetkiDenetcisi
{
    /// <exception cref="YetkisizIslemException"/>
    void Gerektir(Yetki yetki);
}

/// <summary>Denetim yapmaz; servislerin oturumdan bağımsız kullanıldığı testler ve araçlar içindir.</summary>
public sealed class YetkiDenetimiYok : IYetkiDenetcisi
{
    public static readonly YetkiDenetimiYok Instance = new();

    public void Gerektir(Yetki yetki)
    {
    }
}

/// <summary>Üretim denetçisi: oturum yoksa her işlemi reddeder.</summary>
public sealed class OturumYetkiDenetcisi(IOturumBaglami oturum) : IYetkiDenetcisi
{
    private readonly IOturumBaglami _oturum = oturum ?? throw new ArgumentNullException(nameof(oturum));

    public void Gerektir(Yetki yetki) => _oturum.YetkiGerektir(YetkiKurallari.GerekliRol(yetki));
}
