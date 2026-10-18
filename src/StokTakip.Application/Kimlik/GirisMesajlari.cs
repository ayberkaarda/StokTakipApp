using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.Kimlik;

/// <summary>Giriş formunda gösterilecek metinler; ipucu her hatada aynı şekilde gösterilir, hesap durumunu ele vermez.</summary>
public static class GirisMesajlari
{
    public const string GenelHata = GecersizKimlikBilgisiException.GenelMesaj;

    public const string GenelIpucu = "Çok sayıda hatalı deneme hesabı geçici olarak kilitleyebilir.";
}
