using System;
using System.Collections.Generic;
using StokTakip.Application.Raporlama;

namespace StokTakip.Views
{
    public enum RaporTuru
    {
        EnCokSatanlar = 0,
        Gunluk = 1,
        Aylik = 2,
    }

    public enum TarihAraligi
    {
        Son7Gun = 0,
        Son30Gun = 1,
        Son3Ay = 2,
        Son12Ay = 3,
        BuYil = 4,
        Ozel = 5,
    }

    public interface IRaporlarView : IMesajView
    {
        event EventHandler Yuklendi;
        event EventHandler Kapandi;
        event EventHandler RaporTuruDegisti;
        event EventHandler AralikSecildi;
        event EventHandler TarihDegisti;
        event EventHandler AdetDegisti;
        event EventHandler YenileIstendi;

        bool Kapali { get; }

        RaporTuru SeciliRapor { get; }

        TarihAraligi Aralik { get; set; }

        DateTime Baslangic { get; set; }

        DateTime Bitis { get; set; }

        int Adet { get; }

        bool Yukleniyor { set; }

        /// <param name="bosMesaji">null değilse sonuç yerine bu mesaj gösterilir.</param>
        void EnCokSatanlariGoster(IReadOnlyList<UrunSatisOzetiDto> sonuc, string ozet, string bosMesaji);

        void DonemOzetiniGoster(RaporTuru tur, IReadOnlyList<DonemOzetiDto> sonuc, string ozet, string bosMesaji);
    }
}
