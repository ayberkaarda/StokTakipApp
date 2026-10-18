using System;
using System.Collections.Generic;
using StokTakip.Application.Dtos;

namespace StokTakip.Views
{
    public interface ISiparisGecmisiView : IMesajView
    {
        event EventHandler Yuklendi;
        event EventHandler SiparisSilIstendi;
        event EventHandler Kapandi;

        bool Kapali { get; }

        int? SeciliSiparisId { get; }

        void SiparisleriGoster(IReadOnlyList<SiparisDto> siparisler);

        void OzetiGoster(SiparisOzetiDto ozet);
    }
}
