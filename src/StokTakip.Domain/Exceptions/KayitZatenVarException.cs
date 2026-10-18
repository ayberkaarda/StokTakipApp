namespace StokTakip.Domain.Exceptions;

// DomainException'dan türemez: ilk kurulumun mevcut hata eşlemesine girmelidir.
public sealed class KayitZatenVarException : Exception
{
    public KayitZatenVarException()
        : base("Bu kayıt zaten mevcut.")
    {
    }
}
