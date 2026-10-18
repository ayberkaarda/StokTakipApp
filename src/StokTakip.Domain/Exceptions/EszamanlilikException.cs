namespace StokTakip.Domain.Exceptions;

public sealed class EszamanlilikException : DomainException
{
    public EszamanlilikException()
        : base("Kayıt başka bir kullanıcı tarafından değiştirildi, işlemi yenileyip tekrar deneyin.")
    {
    }
}
