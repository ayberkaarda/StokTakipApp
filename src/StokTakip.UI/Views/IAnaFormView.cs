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
        event EventHandler DuzenleIstendi;
        event EventHandler DuzenlemeKaydetIstendi;
        event EventHandler DuzenlemeVazgecIstendi;
        event EventHandler SiparisVerIstendi;
        event EventHandler SiparisGecmisiIstendi;
        event EventHandler RaporlarIstendi;
        event EventHandler KategorilerIstendi;
        event EventHandler FiltreDegisti;
        event EventHandler DusukStokFiltresiIstendi;
        event EventHandler<UrunDto> UrunSecildi;
        event EventHandler Kapandi;

        string UrunAdi { get; set; }

        string StokMetni { get; set; }

        string EsikMetni { get; set; }

        /// <summary>Ekleme/düzenleme alanındaki kategori seçimi.</summary>
        KategoriSecenegi UrunKategorisi { get; set; }

        string SiparisAdetMetni { get; set; }

        UrunDto SeciliUrun { get; }

        int? SiparisUrunId { get; set; }

        string SiparisUrunAdi { get; }

        KategoriSecenegi KategoriFiltresi { get; set; }

        StokFiltresi StokFiltresi { get; set; }

        /// <summary>null ise görünüm normal moddadır; değilse bu ürün düzenleniyordur.</summary>
        UrunDto DuzenlenenUrun { set; }

        /// <param name="tumUrunler">Sipariş açılır listesi için tüm ürünler.</param>
        /// <param name="listelenen">Filtrelerden geçen, ızgarada gösterilecek ürünler.</param>
        void UrunleriGoster(IReadOnlyList<UrunDto> tumUrunler, IReadOnlyList<UrunDto> listelenen);

        void KategorileriGoster(IReadOnlyList<KategoriSecenegi> filtreSecenekleri, IReadOnlyList<KategoriSecenegi> urunSecenekleri);

        void DusukStokUyarisiGoster(int adet);

        void SiparisGecmisiniAc();

        void RaporlariAc();

        void KategorileriYonet();
    }
}
