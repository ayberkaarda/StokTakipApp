using System.Globalization;
using StokTakip.Domain;

namespace StokTakip.Application.Denetim;

public static class DenetimVarlikTurleri
{
    public const string Urun = "Ürün";
    public const string Siparis = "Sipariş";
    public const string Kategori = "Kategori";
    public const string Kullanici = "Kullanıcı";
    public const string Oturum = "Oturum";
    public const string Denetim = "Denetim";
}

/// <summary>Denetim özetlerinin ve gösterim metinlerinin kültürden bağımsız biçimleri.</summary>
public static class DenetimMetni
{
    public static string IslemAdi(DenetimIslemi islem) => islem switch
    {
        DenetimIslemi.UrunEklendi => "Ürün eklendi",
        DenetimIslemi.UrunGuncellendi => "Ürün güncellendi",
        DenetimIslemi.UrunSilindi => "Ürün silindi",
        DenetimIslemi.StokGuncellendi => "Stok güncellendi",
        DenetimIslemi.SiparisOlusturuldu => "Sipariş oluşturuldu",
        DenetimIslemi.SiparisSilindi => "Sipariş silindi",
        DenetimIslemi.KategoriEklendi => "Kategori eklendi",
        DenetimIslemi.KategoriAdiDegisti => "Kategori adı değişti",
        DenetimIslemi.KategoriSilindi => "Kategori silindi",
        DenetimIslemi.EsikDegisti => "Eşik değişti",
        DenetimIslemi.KullaniciOlusturuldu => "Kullanıcı oluşturuldu",
        DenetimIslemi.RolDegisti => "Rol değişti",
        DenetimIslemi.AktiflikDegisti => "Aktiflik değişti",
        DenetimIslemi.ParolaDegisti => "Parola değişti",
        DenetimIslemi.GirisBasarili => "Giriş başarılı",
        DenetimIslemi.GirisBasarisiz => "Giriş başarısız",
        DenetimIslemi.CikisYapildi => "Çıkış yapıldı",
        DenetimIslemi.IlkAdminOlusturuldu => "İlk yönetici oluşturuldu",
        DenetimIslemi.CsvAktarildi => "CSV aktarıldı",
        _ => islem.ToString(),
    };

    /// <summary>"+5", "−20" (U+2212) ya da "0".</summary>
    public static string Fark(long fark) => fark switch
    {
        > 0 => "+" + Sayi(fark),
        < 0 => "−" + Sayi(-fark),
        _ => "0",
    };

    /// <summary>"118 → 98 (−20)".</summary>
    public static string Degisim(int eski, int yeni) => $"{Sayi(eski)} → {Sayi(yeni)} ({Fark((long)yeni - eski)})";

    public static string Sayi(long deger) => deger.ToString(CultureInfo.InvariantCulture);

    public static string Kategori(string? ad) => ad ?? "(kategorisiz)";
}
