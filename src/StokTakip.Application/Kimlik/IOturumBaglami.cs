using StokTakip.Domain;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.Kimlik;

public sealed record OturumKullanicisi(int KullaniciId, string KullaniciAdi, KullaniciRolu Rol);

/// <param name="Nesil">Oturum her açıldığında, güncellendiğinde ya da kapandığında artar.</param>
public readonly record struct OturumAnligi(OturumKullanicisi? Kullanici, long Nesil);

/// <summary>Masaüstü uygulamasında tek oturum: singleton olarak kaydedilir. Tüm işlemler atomiktir.</summary>
public interface IOturumBaglami
{
    OturumKullanicisi? MevcutKullanici { get; }

    bool OturumAcikMi { get; }

    /// <summary>Kullanıcı ve nesil, aynı anda okunmuş tutarlı bir çift olarak.</summary>
    OturumAnligi Anlik { get; }

    void OturumAc(OturumKullanicisi kullanici);

    /// <summary>Nesil hâlâ <paramref name="beklenenNesil"/> ise oturumu açar; arada açma/kapama olduysa hiçbir şey yapmaz.</summary>
    /// <returns>Oturum açıldıysa true.</returns>
    bool OturumAc(OturumKullanicisi kullanici, long beklenenNesil);

    void OturumuKapat();

    /// <summary>Nesil hâlâ <paramref name="beklenenNesil"/> ise oturumu kapatır; arada başka oturum açıldıysa ona dokunmaz.</summary>
    bool OturumuKapat(long beklenenNesil);

    /// <exception cref="YetkisizIslemException">Oturum yoksa ya da rol yetmiyorsa.</exception>
    void YetkiGerektir(KullaniciRolu gerekliRol);
}

public sealed class OturumBaglami : IOturumBaglami
{
    private readonly Lock _kilit = new();
    private OturumAnligi _anlik;

    public OturumKullanicisi? MevcutKullanici => Anlik.Kullanici;

    public bool OturumAcikMi => MevcutKullanici is not null;

    public OturumAnligi Anlik
    {
        get
        {
            lock (_kilit)
            {
                return _anlik;
            }
        }
    }

    public void OturumAc(OturumKullanicisi kullanici)
    {
        ArgumentNullException.ThrowIfNull(kullanici);
        lock (_kilit)
        {
            _anlik = new(kullanici, _anlik.Nesil + 1);
        }
    }

    public bool OturumAc(OturumKullanicisi kullanici, long beklenenNesil)
    {
        ArgumentNullException.ThrowIfNull(kullanici);
        lock (_kilit)
        {
            if (_anlik.Nesil != beklenenNesil)
            {
                return false;
            }

            _anlik = new(kullanici, _anlik.Nesil + 1);
            return true;
        }
    }

    public void OturumuKapat()
    {
        lock (_kilit)
        {
            _anlik = new(null, _anlik.Nesil + 1);
        }
    }

    public bool OturumuKapat(long beklenenNesil)
    {
        lock (_kilit)
        {
            if (_anlik.Nesil != beklenenNesil)
            {
                return false;
            }

            _anlik = new(null, _anlik.Nesil + 1);
            return true;
        }
    }

    public void YetkiGerektir(KullaniciRolu gerekliRol)
    {
        var kullanici = MevcutKullanici ?? throw new YetkisizIslemException(YetkisizIslemException.OturumGerekliMesaji);
        if (kullanici.Rol < gerekliRol)
        {
            throw new YetkisizIslemException();
        }
    }
}
