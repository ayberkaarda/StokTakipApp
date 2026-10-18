using StokTakip.Domain;

namespace StokTakip.Application.Denetim;

/// <param name="Baslangic">Yerel gün; günün başından itibaren dahil.</param>
/// <param name="Bitis">Yerel gün; günün sonuna kadar dahil.</param>
/// <param name="KullaniciAdi">Parça eşleşmesi (%, _ ve [ karakterleri düz metin sayılır).</param>
/// <param name="VarlikAdi">Ürün/kategori/kullanıcı adında parça eşleşmesi.</param>
/// <param name="SayfaBoyutu">1..200 aralığına sıkıştırılır.</param>
public sealed record DenetimSorgusu(
    DateTime? Baslangic = null,
    DateTime? Bitis = null,
    string? KullaniciAdi = null,
    DenetimIslemi? Islem = null,
    string? VarlikAdi = null,
    int Sayfa = 1,
    int SayfaBoyutu = 50);

/// <param name="Zaman">UTC (Kind = Utc); veritabanı sunucusunun saatidir.</param>
public sealed record DenetimKaydiDto(
    long DenetimId,
    DateTime Zaman,
    int? KullaniciId,
    string KullaniciAdi,
    DenetimIslemi Islem,
    string VarlikTuru,
    int? VarlikId,
    string? VarlikAdi,
    string? EskiDeger,
    string? YeniDeger,
    int? Fark,
    string Ozet)
{
    public string IslemAdi => DenetimMetni.IslemAdi(Islem);
}
