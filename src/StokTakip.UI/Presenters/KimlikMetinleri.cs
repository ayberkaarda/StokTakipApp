using System;
using System.Linq;
using StokTakip.Application.Kimlik;
using StokTakip.Domain;
using StokTakip.Views;

namespace StokTakip.Views
{
    public enum FormAlani
    {
        KullaniciAdi,
        Parola,
        ParolaTekrar,
        EskiParola,
    }

    /// <summary>Arayüzde gösterilecek/gizlenecek yönetici işlemleri; asıl denetim servistedir.</summary>
    public sealed record ArayuzYetkileri(
        bool UrunSil, bool KategoriYonetimi, bool EsikDuzenle, bool KullaniciYonetimi, bool DenetimGoruntule)
    {
        public static readonly ArayuzYetkileri Yok = new ArayuzYetkileri(false, false, false, false, false);
    }

    public readonly record struct ParolaKuralDurumu(bool Bos, bool Uzunluk, bool HarfVeRakam, bool KullaniciAdiYok)
    {
        public bool Gecerli => !Bos && Uzunluk && HarfVeRakam && KullaniciAdiYok;
    }
}

namespace StokTakip.Presenters
{
    public static class KimlikMetinleri
    {
        public const string OturumSonlandi =
            "Oturumunuz sonlandırıldı. Hesabınız pasifleştirilmiş ya da yetkileriniz değiştirilmiş olabilir. Lütfen yeniden giriş yapın.";

        public const string CikisYapildi = "Oturum kapatıldı.";

        public const string ParolalarEslesmiyor = "Parolalar eşleşmiyor.";

        public static string RolAdi(KullaniciRolu rol) => rol == KullaniciRolu.Admin ? "Yönetici" : "Operatör";

        public static string OturumBilgisi(OturumKullanicisi kullanici) =>
            kullanici == null ? string.Empty : kullanici.KullaniciAdi + " · " + RolAdi(kullanici.Rol);

        /// <summary><see cref="ParolaPolitikasi"/> kurallarının anlık gösterimi; doğrulamanın kendisi serviste yapılır.</summary>
        public static ParolaKuralDurumu ParolaKurallari(string parola, string kullaniciAdi)
        {
            parola ??= string.Empty;
            string ad = kullaniciAdi?.Trim();
            return new ParolaKuralDurumu(
                parola.Length == 0,
                parola.Length >= ParolaPolitikasi.MinimumUzunluk && parola.Length <= ParolaPolitikasi.MaksimumUzunluk,
                parola.Any(char.IsLetter) && parola.Any(char.IsDigit),
                string.IsNullOrEmpty(ad) || !parola.Contains(ad, StringComparison.OrdinalIgnoreCase));
        }
    }
}
