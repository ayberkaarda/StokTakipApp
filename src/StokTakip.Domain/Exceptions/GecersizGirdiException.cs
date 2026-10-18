namespace StokTakip.Domain.Exceptions;

public sealed class GecersizGirdiException : DomainException
{
    public GecersizGirdiException(string message, string? alanAdi = null)
        : base(message)
    {
        AlanAdi = alanAdi;
    }

    /// <summary>Hatalı alanın adı (ör. nameof(Urun.UrunAdi)); arayüzde odaklama için kullanılabilir.</summary>
    public string? AlanAdi { get; }
}
