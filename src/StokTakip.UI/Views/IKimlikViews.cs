using System;
using System.Collections.Generic;
using StokTakip.Domain;

namespace StokTakip.Views
{
    public interface IGirisView
    {
        event EventHandler GirisIstendi;
        event EventHandler Kapandi;

        bool Kapali { get; }

        string KullaniciAdi { get; set; }

        string Parola { get; set; }

        bool Mesgul { set; }

        void BilgiGoster(string mesaj);

        void HataGoster(string mesaj, string ipucu);

        void ParolayiTemizleVeOdaklan();

        /// <summary>Art arda denemeleri yavaşlatmak için giriş düğmesini kısa süre devre dışı bırakır.</summary>
        void GirisiBeklet(int milisaniye);

        void GirisTamamlandi();
    }

    public interface IIlkKurulumView
    {
        event EventHandler KurulumIstendi;
        event EventHandler ParolaDegisti;
        event EventHandler Kapandi;

        bool Kapali { get; }

        string KullaniciAdi { get; }

        string Parola { get; }

        string ParolaTekrar { get; }

        bool Mesgul { set; }

        void HataGoster(string mesaj, FormAlani? alan);

        void ParolaKurallariniGoster(ParolaKuralDurumu durum);

        /// <param name="kullaniciAdi">Oluşturulan yönetici; kurulum başka yerde tamamlandıysa null.</param>
        void KurulumTamamlandi(string kullaniciAdi, string bilgi);
    }

    public interface IParolaDegistirView
    {
        event EventHandler DegistirIstendi;
        event EventHandler ParolaDegisti;
        event EventHandler Kapandi;

        bool Kapali { get; }

        string EskiParola { get; }

        string YeniParola { get; }

        string YeniParolaTekrar { get; }

        bool Mesgul { set; }

        void HataGoster(string mesaj, FormAlani? alan);

        void ParolaKurallariniGoster(ParolaKuralDurumu durum);

        void Tamamlandi(string bilgi);
    }

    /// <summary>Kullanıcılar tablosunun bir satırı; tarih/durum metinleri sunum için hazırdır.</summary>
    public sealed record KullaniciSatiri(
        int KullaniciId,
        string KullaniciAdi,
        KullaniciRolu Rol,
        bool Aktif,
        string RolAdi,
        string Durum,
        string SonGiris,
        string Kilit,
        bool Kendisi);

    public interface IKullanicilarView : IMesajView
    {
        event EventHandler Yuklendi;
        event EventHandler Kapandi;
        event EventHandler EkleIstendi;
        event EventHandler RolDegistirIstendi;
        event EventHandler AktiflikDegistirIstendi;
        event EventHandler ParolaDegisti;

        bool Kapali { get; }

        string YeniKullaniciAdi { get; set; }

        string YeniParola { get; set; }

        string YeniParolaTekrar { get; set; }

        KullaniciRolu YeniRol { get; set; }

        KullaniciSatiri SeciliKullanici { get; }

        void KullanicilariGoster(IReadOnlyList<KullaniciSatiri> kullanicilar, int? seciliKullaniciId);

        void ParolaKurallariniGoster(ParolaKuralDurumu durum);

        void AlanaOdaklan(FormAlani alan);
    }
}
