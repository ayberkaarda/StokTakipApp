using System.Security.Cryptography;

namespace StokTakip.Application.Kimlik;

/// <summary>
/// PBKDF2-HMAC-SHA256, kullanıcı başına 16 bayt rastgele tuz. İterasyon sayısı özetle birlikte saklanır;
/// <see cref="Iterasyon"/> artırıldığında eski özetler doğru girişte <see cref="ParolaDogrulamaSonucu.BasariliYenidenHashGerekli"/> döner.
/// </summary>
public sealed class ParolaHasher : IParolaHasher
{
    public const int MinimumIterasyon = 600_000;
    public const int TuzUzunlugu = 16;
    public const int HashUzunlugu = 32;

    private static readonly HashAlgorithmName Algoritma = HashAlgorithmName.SHA256;

    private readonly Lazy<ParolaHashSonucu> _sahteOzet;

    public ParolaHasher()
        : this(MinimumIterasyon)
    {
    }

    public ParolaHasher(int iterasyon)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(iterasyon, MinimumIterasyon);
        Iterasyon = iterasyon;
        _sahteOzet = new Lazy<ParolaHashSonucu>(
            () => Hashle(Convert.ToBase64String(RandomNumberGenerator.GetBytes(TuzUzunlugu))));
    }

    public int Iterasyon { get; }

    public ParolaHashSonucu Hashle(string parola)
    {
        ArgumentNullException.ThrowIfNull(parola);
        var tuz = RandomNumberGenerator.GetBytes(TuzUzunlugu);
        var hash = Rfc2898DeriveBytes.Pbkdf2(parola, tuz, Iterasyon, Algoritma, HashUzunlugu);
        return new ParolaHashSonucu(hash, tuz, Iterasyon);
    }

    public ParolaDogrulamaSonucu Dogrula(string parola, byte[] hash, byte[] tuz, int iterasyon)
    {
        if (hash is not { Length: > 0 } || tuz is not { Length: > 0 } || iterasyon <= 0)
        {
            return ParolaDogrulamaSonucu.Basarisiz;
        }

        var hesaplanan = Rfc2898DeriveBytes.Pbkdf2(parola ?? string.Empty, tuz, iterasyon, Algoritma, hash.Length);
        if (!CryptographicOperations.FixedTimeEquals(hesaplanan, hash))
        {
            return ParolaDogrulamaSonucu.Basarisiz;
        }

        return iterasyon < Iterasyon || hash.Length != HashUzunlugu || tuz.Length != TuzUzunlugu
            ? ParolaDogrulamaSonucu.BasariliYenidenHashGerekli
            : ParolaDogrulamaSonucu.Basarili;
    }

    public void SahteDogrula(string parola)
    {
        var sahte = _sahteOzet.Value;
        Dogrula(parola, sahte.Hash, sahte.Tuz, sahte.Iterasyon);
    }
}
