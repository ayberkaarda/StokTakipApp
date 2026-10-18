using System;
using System.IO;
using StokTakip.Application.Dtos;

namespace StokTakip.Views
{
    public interface ISiparisGecmisiView : IMesajView
    {
        event EventHandler Yuklendi;
        event EventHandler SiparisSilIstendi;
        event EventHandler Kapandi;
        event EventHandler FiltreDegisti;
        event EventHandler AramaDegisiyor;
        event EventHandler OncekiSayfaIstendi;
        event EventHandler SonrakiSayfaIstendi;
        event EventHandler DisaAktarIstendi;
        event EventHandler DisaAktarimIptalIstendi;

        bool Kapali { get; }

        bool DisaAktariliyor { set; }

        /// <summary>Kullanıcıdan hedef dosyayı ister ve yazmak için açar; vazgeçilirse null.</summary>
        Stream DisaAktarimHedefiAc(string varsayilanDosyaAdi);

        /// <summary>Akışı kapatır; aktarım tamamlanmadıysa yarım kalan dosyayı siler. Form kapalıyken de çağrılabilir.</summary>
        void DisaAktarimHedefiniKapat(Stream hedef, bool tamamlandi);

        void DisaAktarimIlerlemesiGoster(int yazilan, int toplam);

        int? SeciliSiparisId { get; }

        string AramaMetni { get; }

        DateTime? Baslangic { get; }

        DateTime? Bitis { get; }

        int SayfaBoyutu { get; }

        bool Yukleniyor { set; }

        void SayfayiGoster(SayfaliSonucDto<SiparisDto> sonuc);

        void OzetiGoster(SiparisOzetiDto ozet);
    }
}
