namespace StokTakip.Domain.Exceptions;

public sealed class YetkisizIslemException : DomainException
{
    public const string OturumGerekliMesaji = "Bu işlem için oturum açmanız gerekir.";
    public const string YetkiYokMesaji = "Bu işlem için yetkiniz yok.";
    public const string OturumDegistiMesaji = "Oturum işlem sırasında değişti; işlem iptal edildi.";

    public YetkisizIslemException(string message = YetkiYokMesaji)
        : base(message)
    {
    }
}
