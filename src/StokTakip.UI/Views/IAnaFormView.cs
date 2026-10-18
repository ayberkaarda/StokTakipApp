using System;
using System.Collections.Generic;
using StokTakip.Application.Dtos;

namespace StokTakip.Views
{
    public interface IAnaFormView : IMesajView
    {
        event EventHandler Yuklendi;
        event EventHandler UrunEkleIstendi;
        event EventHandler StokGuncelleIstendi;
        event EventHandler UrunSilIstendi;
        event EventHandler SiparisVerIstendi;
        event EventHandler SiparisGecmisiIstendi;
        event EventHandler<UrunDto> UrunSecildi;

        string UrunAdi { get; set; }

        string StokMetni { get; set; }

        string SiparisAdetMetni { get; set; }

        UrunDto SeciliUrun { get; }

        int? SiparisUrunId { get; set; }

        string SiparisUrunAdi { get; }

        void UrunleriGoster(IReadOnlyList<UrunDto> urunler);

        void SiparisGecmisiniAc();
    }
}
