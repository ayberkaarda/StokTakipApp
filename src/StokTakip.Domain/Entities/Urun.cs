using StokTakip.Domain.Exceptions;

namespace StokTakip.Domain.Entities;

public class Urun
{
    public const int UrunAdiMaksimumUzunluk = 200;

    /// <summary>Eşik belirtilmeden oluşturulan ürünlerin (ve migration öncesi mevcut satırların) düşük stok eşiği.</summary>
    public const int VarsayilanDusukStokEsigi = 10;

    private Urun()
    {
        UrunAdi = string.Empty;
    }

    public Urun(string urunAdi, int stok)
        : this(urunAdi, stok, VarsayilanDusukStokEsigi)
    {
    }

    public Urun(string urunAdi, int stok, int dusukStokEsigi)
    {
        UrunAdi = AdiDogrula(urunAdi);
        Stok = StokDogrula(stok);
        DusukStokEsigi = EsikDogrula(dusukStokEsigi);
    }

    public int UrunId { get; private set; }

    public string UrunAdi { get; private set; }

    /// <summary>Her zaman sıfır veya pozitiftir.</summary>
    public int Stok { get; private set; }

    /// <summary>Faz 3'te iyimser eşzamanlılık denetimi için kullanılacak (SQL Server rowversion).</summary>
    public byte[]? RowVersion { get; private set; }

    /// <summary>Ürüne özel düşük stok eşiği; her zaman sıfır veya pozitiftir.</summary>
    public int DusukStokEsigi { get; private set; } = VarsayilanDusukStokEsigi;

    /// <summary>Kategorisiz ürünlerde null. Kategori silinince veritabanı bu alanı boşaltır (ON DELETE SET NULL).</summary>
    public int? KategoriId { get; private set; }

    public Kategori? Kategori { get; private set; }

    /// <summary>Stok ve <see cref="DusukStokEsigi"/> değerine göre hesaplanır; kural için bkz. <see cref="StokDurumunuHesapla"/>.</summary>
    public StokDurumu StokDurumu => StokDurumunuHesapla(Stok, DusukStokEsigi);

    /// <summary>
    /// Stok durumu kuralı (E = eşik, S = stok):
    /// <list type="bullet">
    /// <item><description>Kritik: S = 0 ya da S ≤ E / 2 (kesirli yarım: 2·S ≤ E; E = 5 için S ≤ 2).</description></item>
    /// <item><description>Düşük: Kritik değil ve S ≤ E.</description></item>
    /// <item><description>Yeterli: S &gt; E.</description></item>
    /// </list>
    /// E = 0 iken stok 0 Kritik, pozitif stok Yeterli'dir. Örnek E = 10: 0–5 Kritik, 6–10 Düşük, 11+ Yeterli.
    /// </summary>
    public static StokDurumu StokDurumunuHesapla(int stok, int dusukStokEsigi)
    {
        if (stok <= 0 || 2L * stok <= dusukStokEsigi)
        {
            return StokDurumu.Kritik;
        }

        return stok <= dusukStokEsigi ? StokDurumu.Dusuk : StokDurumu.Yeterli;
    }

    public void AdiDegistir(string yeniAd)
    {
        UrunAdi = AdiDogrula(yeniAd);
    }

    /// <summary>Stoğu verilen değere ayarlar (sayım/düzeltme).</summary>
    public void StokAyarla(int yeniStok)
    {
        Stok = StokDogrula(yeniStok);
    }

    /// <exception cref="GecersizGirdiException">Eşik negatifse; bu durumda ürün değişmez.</exception>
    public void DusukStokEsiginiAyarla(int yeniEsik)
    {
        DusukStokEsigi = EsikDogrula(yeniEsik);
    }

    /// <summary>Ürünü kategoriye atar; null verilirse ürün kategorisiz kalır.</summary>
    public void KategoriyeAta(Kategori? kategori)
    {
        Kategori = kategori;
        KategoriId = kategori?.KategoriId;
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

    /// <exception cref="GecersizGirdiException">Eşik negatifse.</exception>
    public static int EsikDogrula(int esik)
    {
        if (esik < 0)
        {
            throw new GecersizGirdiException("Düşük stok eşiği negatif olamaz.", nameof(DusukStokEsigi));
        }

        return esik;
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

        if (urunAdi.Length > UrunAdiMaksimumUzunluk)
        {
            throw new GecersizGirdiException($"Ürün adı en fazla {UrunAdiMaksimumUzunluk} karakter olabilir.", nameof(UrunAdi));
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
