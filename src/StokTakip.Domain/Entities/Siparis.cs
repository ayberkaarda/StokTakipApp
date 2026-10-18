using StokTakip.Domain.Exceptions;

namespace StokTakip.Domain.Entities;

public class Siparis
{
    private Siparis()
    {
    }

    public int SiparisId { get; private set; }

    public int UrunId { get; private set; }

    public int Adet { get; private set; }

    public DateTime Tarih { get; private set; }

    public bool IsDeleted { get; private set; }

    public Urun? Urun { get; private set; }

    /// <summary>
    /// Yeni sipariş oluşturur ve adedi ürünün stoğundan düşer.
    /// </summary>
    /// <exception cref="GecersizGirdiException">Adet sıfır veya negatifse.</exception>
    /// <exception cref="YetersizStokException">Stok yetersizse; bu durumda ürün değişmez.</exception>
    public static Siparis Olustur(Urun urun, int adet, DateTime tarih)
    {
        ArgumentNullException.ThrowIfNull(urun);
        urun.StoktanDus(adet);

        return new Siparis
        {
            UrunId = urun.UrunId,
            Urun = urun,
            Adet = adet,
            Tarih = tarih,
        };
    }

    /// <summary>
    /// Siparişi yumuşak siler (IsDeleted) ve adedi ürünün stoğuna iade eder.
    /// </summary>
    /// <exception cref="GecersizIslemException">Sipariş zaten silinmişse veya ürün eşleşmiyorsa.</exception>
    public void Sil(Urun urun)
    {
        ArgumentNullException.ThrowIfNull(urun);

        if (IsDeleted)
        {
            throw new GecersizIslemException("Bu sipariş zaten silinmiş.");
        }

        if (urun.UrunId != UrunId)
        {
            throw new GecersizIslemException("Sipariş ile stok iadesi yapılacak ürün eşleşmiyor.");
        }

        urun.StokEkle(Adet);
        IsDeleted = true;
    }
}
