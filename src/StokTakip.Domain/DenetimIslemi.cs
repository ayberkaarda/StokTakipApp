namespace StokTakip.Domain;

/// <summary>Veritabanında sayı olarak saklanır: mevcut değerler değiştirilmez, yeni üyeler sona eklenir.</summary>
public enum DenetimIslemi
{
    UrunEklendi = 1,
    UrunGuncellendi = 2,
    UrunSilindi = 3,
    StokGuncellendi = 4,
    SiparisOlusturuldu = 5,
    SiparisSilindi = 6,
    KategoriEklendi = 7,
    KategoriAdiDegisti = 8,
    KategoriSilindi = 9,
    EsikDegisti = 10,
    KullaniciOlusturuldu = 11,
    RolDegisti = 12,
    AktiflikDegisti = 13,
    ParolaDegisti = 14,
    GirisBasarili = 15,
    GirisBasarisiz = 16,
    CikisYapildi = 17,
    IlkAdminOlusturuldu = 18,
    CsvAktarildi = 19,
}
