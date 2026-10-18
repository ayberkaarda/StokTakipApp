using StokTakip.Application.Dtos;
using StokTakip.Domain;
using StokTakip.Domain.Entities;

namespace StokTakip.Application.Mapping;

internal static class DtoMapper
{
    public static SayfaliSonucDto<TDto> ToDto<T, TDto>(this SayfaliSonuc<T> sonuc, Func<T, TDto> donustur) =>
        new(sonuc.Ogeler.Select(donustur).ToList(), sonuc.ToplamKayit, sonuc.Sayfa, sonuc.SayfaBoyutu);

    public static UrunDto ToDto(this Urun urun) =>
        new(urun.UrunId, urun.UrunAdi, urun.Stok, urun.RowVersion?.ToArray());

    public static SiparisDto ToDto(this Siparis siparis) =>
        new(siparis.SiparisId, siparis.UrunId, siparis.Urun?.UrunAdi ?? string.Empty, siparis.Adet, siparis.Tarih);
}
