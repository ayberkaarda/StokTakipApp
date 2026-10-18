namespace StokTakip.Application.Kimlik;

public sealed record ParolaHashSonucu(byte[] Hash, byte[] Tuz, int Iterasyon);

public enum ParolaDogrulamaSonucu
{
    Basarisiz = 0,
    Basarili = 1,

    /// <summary>Parola doğru, ancak özet güncel ayarlardan zayıf; yeniden hesaplanıp saklanmalı.</summary>
    BasariliYenidenHashGerekli = 2,
}

public interface IParolaHasher
{
    ParolaHashSonucu Hashle(string parola);

    ParolaDogrulamaSonucu Dogrula(string parola, byte[] hash, byte[] tuz, int iterasyon);

    /// <summary>
    /// Kullanıcı bulunamadığında gerçek doğrulamayla aynı maliyette bir doğrulama yapar;
    /// yanıt süresinden kullanıcının varlığı anlaşılmasın diye.
    /// </summary>
    void SahteDogrula(string parola);
}
