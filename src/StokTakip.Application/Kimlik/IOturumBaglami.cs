using StokTakip.Domain;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.Kimlik;

public sealed record OturumKullanicisi(int KullaniciId, string KullaniciAdi, KullaniciRolu Rol);

/// <summary>Masaüstü uygulamasında tek oturum: singleton olarak kaydedilir.</summary>
public interface IOturumBaglami
{
    OturumKullanicisi? MevcutKullanici { get; }

    bool OturumAcikMi { get; }

    void OturumAc(OturumKullanicisi kullanici);

    void OturumuKapat();

    /// <exception cref="YetkisizIslemException">Oturum yoksa ya da rol yetmiyorsa.</exception>
    void YetkiGerektir(KullaniciRolu gerekliRol);
}

public sealed class OturumBaglami : IOturumBaglami
{
    private OturumKullanicisi? _mevcut;

    public OturumKullanicisi? MevcutKullanici => Volatile.Read(ref _mevcut);

    public bool OturumAcikMi => MevcutKullanici is not null;

    public void OturumAc(OturumKullanicisi kullanici)
    {
        ArgumentNullException.ThrowIfNull(kullanici);
        Volatile.Write(ref _mevcut, kullanici);
    }

    public void OturumuKapat() => Volatile.Write(ref _mevcut, null);

    public void YetkiGerektir(KullaniciRolu gerekliRol)
    {
        var kullanici = MevcutKullanici ?? throw new YetkisizIslemException(YetkisizIslemException.OturumGerekliMesaji);
        if (kullanici.Rol < gerekliRol)
        {
            throw new YetkisizIslemException();
        }
    }
}
