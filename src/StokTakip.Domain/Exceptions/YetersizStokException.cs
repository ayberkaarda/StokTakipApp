namespace StokTakip.Domain.Exceptions;

public sealed class YetersizStokException : DomainException
{
    public YetersizStokException(int urunId, string urunAdi, int mevcutStok, int istenenAdet)
        : base($"Yetersiz stok: '{urunAdi}' için mevcut stok {mevcutStok}, istenen adet {istenenAdet}.")
    {
        UrunId = urunId;
        UrunAdi = urunAdi;
        MevcutStok = mevcutStok;
        IstenenAdet = istenenAdet;
    }

    public int UrunId { get; }

    public string UrunAdi { get; }

    public int MevcutStok { get; }

    public int IstenenAdet { get; }
}
