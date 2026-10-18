using StokTakip.Domain;
using StokTakip.Domain.Exceptions;
using StokTakip.Domain.Repositories;

namespace StokTakip.Application.Kimlik;

internal static class GuncelYetkiDogrulama
{
    /// <inheritdoc cref="AnlikDogrulaAsync"/>
    public static async Task<OturumKullanicisi> DogrulaAsync(
        IOturumBaglami oturum, IKullaniciRepository kullanicilar, KullaniciRolu gerekliRol, CancellationToken cancellationToken)
        => (await AnlikDogrulaAsync(oturum, kullanicilar, gerekliRol, cancellationToken)).Kullanici!;

    /// <summary>
    /// Oturumdaki kullanıcıyı ve rolünü önce bellekte, sonra veritabanındaki güncel kayıtla doğrular.
    /// Kayıt yok, pasif ya da rolü yetmiyorsa (başka örnekte değiştirilmiş olabilir) oturumu kapatır.
    /// Sorgu sürerken oturum değiştiyse reddeder; dönen anlık işlemin aktörü ve neslidir.
    /// </summary>
    /// <exception cref="YetkisizIslemException"/>
    public static async Task<OturumAnligi> AnlikDogrulaAsync(
        IOturumBaglami oturum, IKullaniciRepository kullanicilar, KullaniciRolu gerekliRol, CancellationToken cancellationToken)
    {
        var anlik = oturum.Anlik;
        var kullanici = anlik.Kullanici ?? throw new YetkisizIslemException(YetkisizIslemException.OturumGerekliMesaji);
        if (kullanici.Rol < gerekliRol)
        {
            throw new YetkisizIslemException();
        }

        var guncel = await kullanicilar.YetkiBilgisiGetirAsync(kullanici.KullaniciId, cancellationToken);
        NesilDegismediyseDevam(oturum, anlik);
        if (guncel is not { Aktif: true } || guncel.Rol < gerekliRol)
        {
            oturum.OturumuKapat(anlik.Nesil);
            throw new YetkisizIslemException();
        }

        return anlik;
    }

    /// <exception cref="YetkisizIslemException">Oturum <paramref name="anlik"/> alındıktan sonra kapandı ya da değiştiyse.</exception>
    public static void NesilDegismediyseDevam(IOturumBaglami oturum, OturumAnligi anlik)
    {
        if (oturum.Anlik.Nesil != anlik.Nesil)
        {
            throw new YetkisizIslemException(YetkisizIslemException.OturumDegistiMesaji);
        }
    }
}
