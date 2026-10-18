using System;
using StokTakip.Application.Dtos;

namespace StokTakip.Views
{
    public interface ISiparisGecmisiView : IMesajView
    {
        event EventHandler Yuklendi;
        event EventHandler SiparisSilIstendi;
        event EventHandler Kapandi;
        event EventHandler FiltreDegisti;
        event EventHandler OncekiSayfaIstendi;
        event EventHandler SonrakiSayfaIstendi;

        bool Kapali { get; }

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
