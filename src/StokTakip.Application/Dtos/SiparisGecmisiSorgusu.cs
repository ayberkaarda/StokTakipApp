namespace StokTakip.Application.Dtos;

public sealed record SiparisGecmisiSorgusu(
    DateTime? Baslangic = null,
    DateTime? Bitis = null,
    string? UrunAdi = null,
    int Sayfa = 1,
    int SayfaBoyutu = 20,
    int? UrunId = null);
