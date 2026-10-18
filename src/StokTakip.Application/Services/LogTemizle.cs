namespace StokTakip.Application.Services;

/// <summary>
/// Kullanıcı kontrollü metinleri (ürün, kullanıcı, kategori adları; bunları içeren hata mesajları) günlüğe yazmadan önce
/// temizler: kontrol ve satır ayırıcı karakterler boşluğa çevrilir, uzunluk sınırlanır. Yapılandırılmış log özelliklerine de uygulanmalıdır.
/// </summary>
public static class LogTemizle
{
    public const int MaksimumUzunluk = 200;

    public static string Metin(string? deger)
    {
        if (string.IsNullOrEmpty(deger))
        {
            return string.Empty;
        }

        var uzunluk = Math.Min(deger.Length, MaksimumUzunluk);
        if (uzunluk < deger.Length && char.IsHighSurrogate(deger[uzunluk - 1]))
        {
            uzunluk--;
        }

        return string.Create(uzunluk, deger, static (hedef, kaynak) =>
        {
            for (var i = 0; i < hedef.Length; i++)
            {
                var c = kaynak[i];
                hedef[i] = char.IsControl(c) || c is '\x2028' or '\x2029' ? ' ' : c;
            }
        });
    }
}
