using StokTakip.Application.DisaAktarim;
using StokTakip.Application.Dtos;
using StokTakip.Application.Raporlama;
using StokTakip.Application.Services;

namespace StokTakip.Application.Kimlik;

public sealed class YetkiDenetimliStokService(IStokService ic, IYetkiDenetcisi yetki) : IStokService
{
    public Task<IReadOnlyList<UrunDto>> UrunleriListeleAsync(CancellationToken cancellationToken = default)
    {
        yetki.Gerektir(Yetki.UrunGoruntule);
        return ic.UrunleriListeleAsync(cancellationToken);
    }

    public Task<UrunDto?> UrunGetirAsync(int urunId, CancellationToken cancellationToken = default)
    {
        yetki.Gerektir(Yetki.UrunGoruntule);
        return ic.UrunGetirAsync(urunId, cancellationToken);
    }

    public Task<UrunDto> UrunEkleAsync(UrunEkleIstegi istek, CancellationToken cancellationToken = default)
    {
        yetki.Gerektir(Yetki.UrunEkleGuncelle);
        return ic.UrunEkleAsync(istek, cancellationToken);
    }

    public Task<UrunDto> UrunGuncelleAsync(UrunGuncelleIstegi istek, CancellationToken cancellationToken = default, byte[]? beklenenRowVersion = null)
    {
        yetki.Gerektir(Yetki.UrunEkleGuncelle);
        return ic.UrunGuncelleAsync(istek, cancellationToken, beklenenRowVersion);
    }

    public Task<UrunDto> StokGuncelleAsync(int urunId, int yeniStok, CancellationToken cancellationToken = default, byte[]? beklenenRowVersion = null)
    {
        yetki.Gerektir(Yetki.StokGuncelle);
        return ic.StokGuncelleAsync(urunId, yeniStok, cancellationToken, beklenenRowVersion);
    }

    public Task UrunSilAsync(int urunId, CancellationToken cancellationToken = default, byte[]? beklenenRowVersion = null)
    {
        yetki.Gerektir(Yetki.UrunSil);
        return ic.UrunSilAsync(urunId, cancellationToken, beklenenRowVersion);
    }
}

public sealed class YetkiDenetimliSiparisService(ISiparisService ic, IYetkiDenetcisi yetki) : ISiparisService
{
    public Task<SiparisDto> SiparisOlusturAsync(SiparisOlusturIstegi istek, CancellationToken cancellationToken = default)
    {
        yetki.Gerektir(Yetki.SiparisOlustur);
        return ic.SiparisOlusturAsync(istek, cancellationToken);
    }

    public Task<IReadOnlyList<SiparisDto>> SiparisGecmisiniGetirAsync(CancellationToken cancellationToken = default)
    {
        yetki.Gerektir(Yetki.GecmisVeRaporGoruntule);
        return ic.SiparisGecmisiniGetirAsync(cancellationToken);
    }

    public Task<SayfaliSonucDto<SiparisDto>> SiparisGecmisiSayfaGetirAsync(SiparisGecmisiSorgusu sorgu, CancellationToken cancellationToken = default)
    {
        yetki.Gerektir(Yetki.GecmisVeRaporGoruntule);
        return ic.SiparisGecmisiSayfaGetirAsync(sorgu, cancellationToken);
    }

    public Task<SiparisOzetiDto> SiparisOzetiGetirAsync(CancellationToken cancellationToken = default)
    {
        yetki.Gerektir(Yetki.GecmisVeRaporGoruntule);
        return ic.SiparisOzetiGetirAsync(cancellationToken);
    }

    public Task SiparisSilAsync(int siparisId, CancellationToken cancellationToken = default)
    {
        yetki.Gerektir(Yetki.SiparisSil);
        return ic.SiparisSilAsync(siparisId, cancellationToken);
    }
}

public sealed class YetkiDenetimliKategoriService(IKategoriService ic, IYetkiDenetcisi yetki) : IKategoriService
{
    public Task<IReadOnlyList<KategoriDto>> KategorileriListeleAsync(CancellationToken cancellationToken = default)
    {
        yetki.Gerektir(Yetki.UrunGoruntule);
        return ic.KategorileriListeleAsync(cancellationToken);
    }

    public Task<KategoriDto> KategoriEkleAsync(string ad, CancellationToken cancellationToken = default)
    {
        yetki.Gerektir(Yetki.KategoriYonetimi);
        return ic.KategoriEkleAsync(ad, cancellationToken);
    }

    public Task<KategoriDto> KategoriYenidenAdlandirAsync(int kategoriId, string yeniAd, CancellationToken cancellationToken = default)
    {
        yetki.Gerektir(Yetki.KategoriYonetimi);
        return ic.KategoriYenidenAdlandirAsync(kategoriId, yeniAd, cancellationToken);
    }

    public Task KategoriSilAsync(int kategoriId, CancellationToken cancellationToken = default)
    {
        yetki.Gerektir(Yetki.KategoriYonetimi);
        return ic.KategoriSilAsync(kategoriId, cancellationToken);
    }
}

public sealed class YetkiDenetimliRaporService(IRaporService ic, IYetkiDenetcisi yetki) : IRaporService
{
    public Task<IReadOnlyList<UrunSatisOzetiDto>> EnCokSatanlariGetirAsync(DateTime? baslangic, DateTime? bitis, int adet, CancellationToken cancellationToken = default)
    {
        yetki.Gerektir(Yetki.GecmisVeRaporGoruntule);
        return ic.EnCokSatanlariGetirAsync(baslangic, bitis, adet, cancellationToken);
    }

    public Task<IReadOnlyList<DonemOzetiDto>> GunlukOzetAsync(DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default)
    {
        yetki.Gerektir(Yetki.GecmisVeRaporGoruntule);
        return ic.GunlukOzetAsync(baslangic, bitis, cancellationToken);
    }

    public Task<IReadOnlyList<DonemOzetiDto>> AylikOzetAsync(DateTime baslangic, DateTime bitis, CancellationToken cancellationToken = default)
    {
        yetki.Gerektir(Yetki.GecmisVeRaporGoruntule);
        return ic.AylikOzetAsync(baslangic, bitis, cancellationToken);
    }
}

public sealed class YetkiDenetimliSiparisDisaAktarimService(ISiparisDisaAktarimService ic, IYetkiDenetcisi yetki) : ISiparisDisaAktarimService
{
    public Task SiparisleriCsvAktarAsync(SiparisGecmisiSorgusu filtre, Stream hedef, CancellationToken cancellationToken = default)
    {
        yetki.Gerektir(Yetki.CsvAktarim);
        return ic.SiparisleriCsvAktarAsync(filtre, hedef, cancellationToken);
    }
}
