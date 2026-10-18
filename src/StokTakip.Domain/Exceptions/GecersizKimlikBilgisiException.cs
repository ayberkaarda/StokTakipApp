namespace StokTakip.Domain.Exceptions;

/// <summary>
/// Kullanıcı adı yok, parola yanlış ya da hesap pasif olduğunda aynı mesajla fırlatılır;
/// mesaj hangi bilginin hatalı olduğunu ele vermez.
/// </summary>
public sealed class GecersizKimlikBilgisiException : DomainException
{
    public const string GenelMesaj = "Kullanıcı adı veya parola hatalı.";

    public GecersizKimlikBilgisiException()
        : base(GenelMesaj)
    {
    }
}
