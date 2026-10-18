using StokTakip.Application.Dtos;
using StokTakip.Domain.Entities;

namespace StokTakip.Application.Mapping;

internal static class DtoMapper
{
    public static UrunDto ToDto(this Urun urun) =>
        new(urun.UrunId, urun.UrunAdi, urun.Stok);

    public static SiparisDto ToDto(this Siparis siparis) =>
        new(siparis.SiparisId, siparis.UrunId, siparis.Urun?.UrunAdi ?? string.Empty, siparis.Adet, siparis.Tarih);
}
