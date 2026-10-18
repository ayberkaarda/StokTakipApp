namespace StokTakip.Domain;

/// <summary>Zaman sınırları UTC'dir: başlangıç dahil, bitiş hariç. Metin aramaları parça eşleşmesidir.</summary>
public sealed record DenetimFiltresi(
    DateTime? BaslangicUtc = null,
    DateTime? BitisHaricUtc = null,
    string? KullaniciAdi = null,
    DenetimIslemi? Islem = null,
    string? VarlikAdi = null);
