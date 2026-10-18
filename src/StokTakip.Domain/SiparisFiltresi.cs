namespace StokTakip.Domain;

/// <summary>Başlangıç anı dahildir; bitiş tarihinin tüm günü dahildir.</summary>
public sealed record SiparisFiltresi(
    DateTime? Baslangic = null,
    DateTime? Bitis = null,
    string? UrunAdi = null,
    int? UrunId = null);
