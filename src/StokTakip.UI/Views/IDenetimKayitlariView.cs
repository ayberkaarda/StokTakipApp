using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StokTakip.Application.Denetim;
using StokTakip.Domain;

namespace StokTakip.Views
{
    /// <summary>Denetim tablosunun bir satırı; zaman yerel saate çevrilmiş, metinler sunum için hazırdır.</summary>
    public sealed record DenetimSatiri(
        string Zaman,
        string KullaniciAdi,
        string Islem,
        string Varlik,
        string EskiDeger,
        string YeniDeger,
        string Fark,
        int? FarkDegeri,
        string Ozet);

    public sealed record IslemSecenegi(string Ad, DenetimIslemi? Islem)
    {
        public override string ToString() => Ad;

        public static IReadOnlyList<IslemSecenegi> Tumu() =>
            new[] { new IslemSecenegi("Tümü", null) }
                .Concat(Enum.GetValues<DenetimIslemi>().Select(i => new IslemSecenegi(DenetimMetni.IslemAdi(i), i)))
                .ToList();
    }

    public interface IDenetimKayitlariView : IMesajView
    {
        event EventHandler Yuklendi;
        event EventHandler Kapandi;
        event EventHandler FiltreDegisti;
        event EventHandler AramaDegisiyor;
        event EventHandler OncekiSayfaIstendi;
        event EventHandler SonrakiSayfaIstendi;
        event EventHandler DisaAktarIstendi;
        event EventHandler DisaAktarimIptalIstendi;

        bool Kapali { get; }

        DateTime? Baslangic { get; }

        DateTime? Bitis { get; }

        string KullaniciAdiArama { get; }

        DenetimIslemi? Islem { get; }

        string VarlikAdiArama { get; }

        int SayfaBoyutu { get; }

        bool Yukleniyor { set; }

        bool DisaAktariliyor { set; }

        void SayfayiGoster(IReadOnlyList<DenetimSatiri> satirlar, int sayfa, int toplamSayfa, int toplamKayit);

        /// <summary>Kullanıcıdan hedef dosyayı ister; hedefle aynı klasördeki geçici dosyayı açar. Vazgeçilirse null.</summary>
        Stream DisaAktarimHedefiAc(string varsayilanDosyaAdi);

        /// <summary>Tamamlandıysa geçici dosyayı hedefin yerine taşır, değilse siler. Form kapalıyken de çağrılabilir.</summary>
        void DisaAktarimHedefiniKapat(Stream hedef, bool tamamlandi);

        void DisaAktarimIlerlemesiGoster(int yazilan, int toplam);
    }
}
