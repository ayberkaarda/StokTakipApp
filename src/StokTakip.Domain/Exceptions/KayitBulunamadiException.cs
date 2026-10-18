namespace StokTakip.Domain.Exceptions;

public sealed class KayitBulunamadiException : DomainException
{
    public KayitBulunamadiException(string kayitTuru, int id)
        : base($"{kayitTuru} bulunamadı (Id: {id}).")
    {
        KayitTuru = kayitTuru;
        Id = id;
    }

    /// <summary>Kullanıcıya gösterilecek kayıt türü, ör. "Ürün" veya "Sipariş".</summary>
    public string KayitTuru { get; }

    public int Id { get; }
}
