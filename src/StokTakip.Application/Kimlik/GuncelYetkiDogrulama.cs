using StokTakip.Domain;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Kimlik;

internal static class GuncelYetkiDogrulama
{
    /// <summary>
    /// Oturumdaki kullanıcıyı ve rolünü önce bellekte, sonra veritabanındaki güncel kayıtla doğrular.
    /// Kayıt yok, pasif ya da rolü yetmiyorsa (başka örnekte değiştirilmiş olabilir) oturumu kapatır.
    /// </summary>
    /// <exception cref="YetkisizIslemException"/>
    public static async Task<OturumKullanicisi> DogrulaAsync(
        IOturumBaglami oturum, IKullaniciRepository kullanicilar, KullaniciRolu gerekliRol, CancellationToken cancellationToken)
    {
        var anlik = oturum.Anlik;
        var kullanici = anlik.Kullanici ?? throw new YetkisizIslemException(YetkisizIslemException.OturumGerekliMesaji);
        if (kullanici.Rol < gerekliRol)
        {
            throw new YetkisizIslemException();
        }

        var guncel = await kullanicilar.YetkiBilgisiGetirAsync(kullanici.KullaniciId, cancellationToken);
        if (guncel is not { Aktif: true } || guncel.Rol < gerekliRol)
        {
            oturum.OturumuKapat(anlik.Nesil);
            throw new YetkisizIslemException();
        }

        return kullanici;
    }
}
