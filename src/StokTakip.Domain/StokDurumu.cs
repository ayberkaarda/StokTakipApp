namespace StokTakip.Domain;

/// <summary>
/// Ürünün stok miktarının, ürüne özel düşük stok eşiğine göre durumu.
/// Kural için bkz. <see cref="Entities.Urun.StokDurumunuHesapla"/>.
/// </summary>
public enum StokDurumu
{
    /// <summary>Stok eşiğin üzerinde.</summary>
    Yeterli = 0,

    /// <summary>Stok eşiğe eşit ya da altında, ancak eşiğin yarısından fazla.</summary>
    Dusuk = 1,

    /// <summary>Stok sıfır ya da eşiğin yarısına eşit veya altında.</summary>
    Kritik = 2,
}
