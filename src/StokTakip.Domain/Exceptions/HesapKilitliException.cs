namespace StokTakip.Domain.Exceptions;

public sealed class HesapKilitliException : DomainException
{
    public HesapKilitliException(DateTime kilitBitisUtc)
        : base("Çok sayıda başarısız giriş denemesi nedeniyle hesap geçici olarak kilitlendi. Lütfen daha sonra tekrar deneyin.")
    {
        KilitBitisUtc = kilitBitisUtc;
    }

    public DateTime KilitBitisUtc { get; }
}
