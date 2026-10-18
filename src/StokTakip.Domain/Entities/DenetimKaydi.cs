using StokTakip.Domain.Exceptions;

namespace StokTakip.Domain.Entities;

/// <summary>
/// Yalnızca eklenen denetim kaydı. <see cref="Zaman"/> uygulamadan verilmez; veritabanı sunucusunun UTC saatiyle dolar.
/// Metin alanları sınırı aşarsa kesilir: kayıt yazılamadığı için işlemin düşmesi istenmez.
/// </summary>
public class DenetimKaydi
{
    public const int KullaniciAdiMaksimumUzunluk = 50;
    public const int VarlikTuruMaksimumUzunluk = 30;
    public const int VarlikAdiMaksimumUzunluk = 200;
    public const int DegerMaksimumUzunluk = 400;
    public const int OzetMaksimumUzunluk = 400;
    public const string SistemKullaniciAdi = "sistem";

    private DenetimKaydi()
    {
        KullaniciAdi = string.Empty;
        VarlikTuru = string.Empty;
        Ozet = string.Empty;
    }

    public DenetimKaydi(
        int? kullaniciId,
        string? kullaniciAdi,
        DenetimIslemi islem,
        string varlikTuru,
        int? varlikId,
        string? varlikAdi,
        string? eskiDeger,
        string? yeniDeger,
        int? fark,
        string ozet)
    {
        if (!Enum.IsDefined(islem))
        {
            throw new GecersizGirdiException("Geçersiz denetim işlemi.", nameof(Islem));
        }

        if (string.IsNullOrWhiteSpace(varlikTuru))
        {
            throw new GecersizGirdiException("Varlık türü boş olamaz.", nameof(VarlikTuru));
        }

        if (string.IsNullOrWhiteSpace(ozet))
        {
            throw new GecersizGirdiException("Özet boş olamaz.", nameof(Ozet));
        }

        KullaniciId = kullaniciId;
        KullaniciAdi = Kes(string.IsNullOrWhiteSpace(kullaniciAdi) ? SistemKullaniciAdi : kullaniciAdi.Trim(), KullaniciAdiMaksimumUzunluk)!;
        Islem = islem;
        VarlikTuru = Kes(varlikTuru.Trim(), VarlikTuruMaksimumUzunluk)!;
        VarlikId = varlikId;
        VarlikAdi = Kes(varlikAdi, VarlikAdiMaksimumUzunluk);
        EskiDeger = Kes(eskiDeger, DegerMaksimumUzunluk);
        YeniDeger = Kes(yeniDeger, DegerMaksimumUzunluk);
        Fark = fark;
        Ozet = Kes(ozet.Trim(), OzetMaksimumUzunluk)!;
    }

    public long DenetimId { get; private set; }

    /// <summary>UTC; veritabanı varsayılanından gelir.</summary>
    public DateTime Zaman { get; private set; }

    /// <summary>Oturum yokken (ör. bulunamayan kullanıcıyla başarısız giriş, ilk kurulum) null.</summary>
    public int? KullaniciId { get; private set; }

    /// <summary>İşlem anındaki kullanıcı adının kopyası; oturum yoksa <see cref="SistemKullaniciAdi"/>.</summary>
    public string KullaniciAdi { get; private set; }

    public DenetimIslemi Islem { get; private set; }

    public string VarlikTuru { get; private set; }

    public int? VarlikId { get; private set; }

    /// <summary>Ürün, kategori ya da kullanıcı adının işlem anındaki kopyası.</summary>
    public string? VarlikAdi { get; private set; }

    public string? EskiDeger { get; private set; }

    public string? YeniDeger { get; private set; }

    /// <summary>Stok değişimi ya da iade miktarı (artış pozitif, azalış negatif).</summary>
    public int? Fark { get; private set; }

    public string Ozet { get; private set; }

    private static string? Kes(string? deger, int uzunluk)
    {
        if (deger is null || deger.Length <= uzunluk)
        {
            return deger;
        }

        var son = char.IsHighSurrogate(deger[uzunluk - 1]) ? uzunluk - 1 : uzunluk;
        return deger[..son];
    }
}
