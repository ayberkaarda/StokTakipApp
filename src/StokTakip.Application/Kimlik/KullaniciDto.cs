using StokTakip.Domain;
using StokTakip.Domain.Entities;

namespace StokTakip.Application.Kimlik;

/// <summary>Parola özeti ve tuzu içermez. Tarihler UTC'dir.</summary>
public sealed record KullaniciDto(
    int KullaniciId,
    string KullaniciAdi,
    KullaniciRolu Rol,
    bool Aktif,
    DateTime? SonGirisTarihi,
    DateTime? KilitBitis)
{
    internal static KullaniciDto Olustur(Kullanici k) =>
        new(k.KullaniciId, k.KullaniciAdi, k.Rol, k.Aktif, k.SonGirisTarihi, k.KilitBitis);
}
