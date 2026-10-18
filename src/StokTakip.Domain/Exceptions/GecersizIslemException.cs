namespace StokTakip.Domain.Exceptions;

/// <summary>
/// Girdi geçerli olsa da kaydın mevcut durumunda yapılamayan işlemler için
/// (ör. silinmiş siparişi tekrar silmek, siparişi olan ürünü silmek).
/// </summary>
public sealed class GecersizIslemException : DomainException
{
    public GecersizIslemException(string message)
        : base(message)
    {
    }
}
