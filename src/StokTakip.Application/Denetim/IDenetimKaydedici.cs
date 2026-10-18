using StokTakip.Domain;

namespace StokTakip.Application.Denetim;

/// <param name="Fark">Stok değişimi ya da iade miktarı: artış pozitif, azalış negatif.</param>
public sealed record DenetimOlayi(
    DenetimIslemi Islem,
    string VarlikTuru,
    string Ozet,
    int? VarlikId = null,
    string? VarlikAdi = null,
    string? EskiDeger = null,
    string? YeniDeger = null,
    int? Fark = null)
{
    /// <summary>Verilirse oturumdaki kullanıcı yerine bu kişi kaydedilir (giriş/çıkış olayları).</summary>
    public DenetimAktoru? Aktor { get; init; }

    /// <summary>
    /// İşlemin kendisi tamamlandıktan sonra yazılan kayıt (ör. CSV dosyası taşındı): oturum arada değişse de işlemi
    /// başlatan aktörle yazılır, iptal edilmez.
    /// </summary>
    public bool IslemSonrasi { get; init; }
}

public sealed record DenetimAktoru(int? KullaniciId, string KullaniciAdi);

/// <summary>
/// Scoped: servislerle aynı iş birimini (DbContext) paylaşır. İşlemi yapan, oturumdaki kullanıcıdır; oturum yoksa
/// KullaniciId null, kullanıcı adı "sistem" yazılır. Parola, özet ya da tuz hiçbir olaya konmamalıdır.
/// </summary>
public interface IDenetimKaydedici
{
    /// <summary>
    /// Kaydı iş birimine ekler, SaveChanges çağırmaz: işlemin kendi SaveChanges'i/transaction'ı ile birlikte yazılır,
    /// işlem geri alınırsa kayıt da geri alınır.
    /// </summary>
    void Kaydet(DenetimOlayi olay);

    /// <summary>
    /// Ekler ve hemen yazar. Açık bir transaction varsa onun parçasıdır (yeni oluşan kaydın kimliği gibi değerler için);
    /// yoksa tek başına yazılır (başarısız giriş gibi transaction dışı olaylar).
    /// </summary>
    Task KaydetVeYazAsync(DenetimOlayi olay, CancellationToken cancellationToken = default);
}

/// <summary>Kayıt tutmaz; denetim kaydedicisi verilmeden kurulan servislerin varsayılanı.</summary>
public sealed class NullDenetimKaydedici : IDenetimKaydedici
{
    public static readonly NullDenetimKaydedici Instance = new();

    private NullDenetimKaydedici()
    {
    }

    public void Kaydet(DenetimOlayi olay)
    {
    }

    public Task KaydetVeYazAsync(DenetimOlayi olay, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
