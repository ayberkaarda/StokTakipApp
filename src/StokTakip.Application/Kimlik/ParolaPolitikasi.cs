using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.Kimlik;

public static class ParolaPolitikasi
{
    public const int MinimumUzunluk = 10;
    public const int MaksimumUzunluk = 128;
    public const string AlanAdi = "Parola";

    /// <exception cref="GecersizGirdiException"/>
    public static void Dogrula(string? parola, string kullaniciAdi)
    {
        if (parola is null || parola.Length < MinimumUzunluk)
        {
            throw new GecersizGirdiException($"Parola en az {MinimumUzunluk} karakter olmalıdır.", AlanAdi);
        }

        if (parola.Length > MaksimumUzunluk)
        {
            throw new GecersizGirdiException($"Parola en fazla {MaksimumUzunluk} karakter olabilir.", AlanAdi);
        }

        if (!parola.Any(char.IsLetter) || !parola.Any(char.IsDigit))
        {
            throw new GecersizGirdiException("Parola en az bir harf ve bir rakam içermelidir.", AlanAdi);
        }

        if (!string.IsNullOrEmpty(kullaniciAdi) && parola.Contains(kullaniciAdi, StringComparison.OrdinalIgnoreCase))
        {
            throw new GecersizGirdiException("Parola kullanıcı adını içeremez.", AlanAdi);
        }
    }
}
