using System;
using System.Collections.Generic;
using StokTakip.Application.Dtos;

namespace StokTakip.Views
{
    public interface IKategorilerView : IMesajView
    {
        event EventHandler Yuklendi;
        event EventHandler Kapandi;
        event EventHandler EkleIstendi;
        event EventHandler YenidenAdlandirIstendi;
        event EventHandler SilIstendi;
        event EventHandler<KategoriDto> KategoriSecildi;

        bool Kapali { get; }

        string KategoriAdi { get; set; }

        KategoriDto SeciliKategori { get; }

        void KategorileriGoster(IReadOnlyList<KategoriDto> kategoriler, int? seciliKategoriId);
    }
}
