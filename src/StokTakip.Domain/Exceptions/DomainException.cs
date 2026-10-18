namespace StokTakip.Domain.Exceptions;

/// <summary>
/// İş kuralı ihlallerinin ortak tabanı. Mesajlar kullanıcıya doğrudan gösterilebilir.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message)
        : base(message)
    {
    }
}
