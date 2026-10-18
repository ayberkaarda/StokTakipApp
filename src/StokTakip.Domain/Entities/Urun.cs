using StokTakip.Domain.Exceptions;

namespace StokTakip.Domain.Entities;

public class Urun
{
    private Urun()
    {
        UrunAdi = string.Empty;
    }

    public Urun(string urunAdi, int stok)
    {
        UrunAdi = AdiDogrula(urunAdi);
        Stok = StokDogrula(stok);
    }

    public int UrunId { get; private set; }

    public string UrunAdi { get; private set; }

    /// <summary>Her zaman sıfır veya pozitiftir.</summary>
    public int Stok { get; private set; }

    /// <summary>Faz 3'te iyimser eşzamanlılık denetimi için kullanılacak (SQL Server rowversion).</summary>
    public byte[]? RowVersion { get; private set; }

    public void AdiDegistir(string yeniAd)
    {
        UrunAdi = AdiDogrula(yeniAd);
    }

    /// <summary>Stoğu verilen değere ayarlar (sayım/düzeltme).</summary>
    public void StokAyarla(int yeniStok)
    {
        Stok = StokDogrula(yeniStok);
    }

    public void StokEkle(int adet)
    {
        AdetDogrula(adet);
        Stok = checked(Stok + adet);
    }

    /// <exception cref="YetersizStokException">Adet mevcut stoktan büyükse.</exception>
    public void StoktanDus(int adet)
    {
        AdetDogrula(adet);
        if (adet > Stok)
        {
            throw new YetersizStokException(UrunId, UrunAdi, Stok, adet);
        }

        Stok -= adet;
    }

    private static void AdetDogrula(int adet)
    {
        if (adet <= 0)
        {
            throw new GecersizGirdiException("Adet sıfırdan büyük olmalıdır.", "Adet");
        }
    }

    private static string AdiDogrula(string? urunAdi)
    {
        if (string.IsNullOrWhiteSpace(urunAdi))
        {
            throw new GecersizGirdiException("Ürün adı boş olamaz.", nameof(UrunAdi));
        }

        return urunAdi.Trim();
    }

    private static int StokDogrula(int stok)
    {
        if (stok < 0)
        {
            throw new GecersizGirdiException("Stok miktarı negatif olamaz.", nameof(Stok));
        }

        return stok;
    }
}
