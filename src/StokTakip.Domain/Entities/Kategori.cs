using StokTakip.Domain.Exceptions;

namespace StokTakip.Domain.Entities;

/// <summary>
/// Ürün kategorisi. Adı benzersizdir (büyük/küçük harf duyarsız denetim servis katmanında,
/// kesin garanti veritabanındaki benzersiz indekstedir). Kategori silinince ürünleri kategorisiz kalır.
/// </summary>
public class Kategori
{
    public const int AdMaksimumUzunluk = 100;

    private Kategori()
    {
        Ad = string.Empty;
    }

    public Kategori(string ad)
    {
        Ad = AdiDogrula(ad);
    }

    public int KategoriId { get; private set; }

    public string Ad { get; private set; }

    public void YenidenAdlandir(string yeniAd)
    {
        Ad = AdiDogrula(yeniAd);
    }

    /// <summary>Adı doğrular ve baştaki/sondaki boşluklardan arındırılmış halini döner.</summary>
    /// <exception cref="GecersizGirdiException">Ad boşsa veya çok uzunsa.</exception>
    public static string AdiDogrula(string? ad)
    {
        if (string.IsNullOrWhiteSpace(ad))
        {
            throw new GecersizGirdiException("Kategori adı boş olamaz.", nameof(Ad));
        }

        var kirpilmis = ad.Trim();
        if (kirpilmis.Length > AdMaksimumUzunluk)
        {
            throw new GecersizGirdiException($"Kategori adı en fazla {AdMaksimumUzunluk} karakter olabilir.", nameof(Ad));
        }

        return kirpilmis;
    }
}
