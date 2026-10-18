using StokTakip.Domain.Exceptions;

namespace StokTakip.Domain.Entities;

/// <summary>
/// Uygulama kullanıcısı. Kullanıcı adı ASCII harf, rakam, nokta, alt çizgi ve tireden oluşur ve küçük harfe
/// çevrilerek saklanır; böylece benzersiz indeks her veritabanında büyük/küçük harf duyarsız çalışır.
/// Zamanlar UTC'dir.
/// </summary>
public class Kullanici
{
    public const int KullaniciAdiMinimumUzunluk = 3;
    public const int KullaniciAdiMaksimumUzunluk = 50;
    public const int KilitEsigi = 5;
    public static readonly TimeSpan KilitSuresi = TimeSpan.FromMinutes(15);

    private Kullanici()
    {
        KullaniciAdi = string.Empty;
        ParolaHash = [];
        ParolaTuzu = [];
    }

    public Kullanici(string kullaniciAdi, byte[] parolaHash, byte[] parolaTuzu, int iterasyon, KullaniciRolu rol)
    {
        KullaniciAdi = KullaniciAdiDogrula(kullaniciAdi);
        ParolaHash = [];
        ParolaTuzu = [];
        ParolayiAyarla(parolaHash, parolaTuzu, iterasyon);
        Rol = RolDogrula(rol);
        Aktif = true;
    }

    public int KullaniciId { get; private set; }

    public string KullaniciAdi { get; private set; }

    public byte[] ParolaHash { get; private set; }

    public byte[] ParolaTuzu { get; private set; }

    public int Iterasyon { get; private set; }

    public KullaniciRolu Rol { get; private set; }

    public bool Aktif { get; private set; }

    public int BasarisizDenemeSayisi { get; private set; }

    public DateTime? KilitBitis { get; private set; }

    public DateTime? SonGirisTarihi { get; private set; }

    public byte[]? RowVersion { get; private set; }

    public bool KilitliMi(DateTime simdiUtc) => KilitBitis is { } bitis && bitis > simdiUtc;

    public void ParolayiAyarla(byte[] parolaHash, byte[] parolaTuzu, int iterasyon)
    {
        if (parolaHash is not { Length: > 0 } || parolaTuzu is not { Length: > 0 })
        {
            throw new ArgumentException("Parola özeti ve tuzu boş olamaz.");
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(iterasyon);
        ParolaHash = parolaHash.ToArray();
        ParolaTuzu = parolaTuzu.ToArray();
        Iterasyon = iterasyon;
    }

    public void RolAta(KullaniciRolu rol) => Rol = RolDogrula(rol);

    public void AktiflikAyarla(bool aktif) => Aktif = aktif;

    /// <summary>
    /// Yanlış parola denemesini sayar; <see cref="KilitEsigi"/>. denemede hesap <see cref="KilitSuresi"/> boyunca kilitlenir.
    /// Kilit sürerken gelen denemeler sayılmaz ve kilidi uzatmaz; kilit bittikten sonraki ilk deneme sayacı yeniden başlatır.
    /// </summary>
    /// <returns>Bu deneme hesabı kilitlediyse true.</returns>
    public bool BasarisizGirisKaydet(DateTime simdiUtc)
    {
        if (KilitliMi(simdiUtc))
        {
            return false;
        }

        if (KilitBitis is not null)
        {
            KilitBitis = null;
            BasarisizDenemeSayisi = 0;
        }

        BasarisizDenemeSayisi++;
        if (BasarisizDenemeSayisi >= KilitEsigi)
        {
            KilitBitis = simdiUtc + KilitSuresi;
            return true;
        }

        return false;
    }

    /// <exception cref="HesapKilitliException">Kilit sürüyorsa; sayaç ve kilit değişmez.</exception>
    public void BasariliGirisKaydet(DateTime simdiUtc)
    {
        if (KilitliMi(simdiUtc))
        {
            throw new HesapKilitliException(KilitBitis!.Value);
        }

        BasarisizDenemeSayisi = 0;
        KilitBitis = null;
        SonGirisTarihi = simdiUtc;
    }

    /// <summary>Kullanıcı adını doğrular; kırpılmış ve küçük harfe çevrilmiş halini döner.</summary>
    /// <exception cref="GecersizGirdiException"/>
    public static string KullaniciAdiDogrula(string? kullaniciAdi)
    {
        if (string.IsNullOrWhiteSpace(kullaniciAdi))
        {
            throw new GecersizGirdiException("Kullanıcı adı boş olamaz.", nameof(KullaniciAdi));
        }

        var ad = kullaniciAdi.Trim();
        if (ad.Length is < KullaniciAdiMinimumUzunluk or > KullaniciAdiMaksimumUzunluk)
        {
            throw new GecersizGirdiException(
                $"Kullanıcı adı {KullaniciAdiMinimumUzunluk} ile {KullaniciAdiMaksimumUzunluk} karakter arasında olmalıdır.",
                nameof(KullaniciAdi));
        }

        if (!ad.All(GecerliKarakterMi))
        {
            throw new GecersizGirdiException(
                "Kullanıcı adı yalnızca İngilizce harf (a-z), rakam, nokta, alt çizgi ve tire içerebilir.",
                nameof(KullaniciAdi));
        }

        return ad.ToLowerInvariant();
    }

    /// <summary>Giriş için: doğrulama hatası fırlatmadan normalleştirir.</summary>
    public static bool KullaniciAdiNormallestir(string? kullaniciAdi, out string normalAd)
    {
        normalAd = string.Empty;
        var ad = kullaniciAdi?.Trim();
        if (ad is null || ad.Length is < KullaniciAdiMinimumUzunluk or > KullaniciAdiMaksimumUzunluk || !ad.All(GecerliKarakterMi))
        {
            return false;
        }

        normalAd = ad.ToLowerInvariant();
        return true;
    }

    private static bool GecerliKarakterMi(char c) => char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or '-';

    private static KullaniciRolu RolDogrula(KullaniciRolu rol)
    {
        if (!Enum.IsDefined(rol))
        {
            throw new GecersizGirdiException("Geçersiz kullanıcı rolü.", nameof(Rol));
        }

        return rol;
    }
}
