using Microsoft.Extensions.Logging;
using StokTakip.Domain.Exceptions;

namespace StokTakip.Application.Services;

internal static class IslemGunlugu
{
    // İstisna filtresi içinde çağrılır; false döndüğü için yığın açılmadan istisna yoluna devam eder.
    public static bool IhlaliLogla(this ILogger logger, DomainException ex, string islem)
    {
        if (ex is EszamanlilikException)
        {
            logger.LogWarning("Eşzamanlılık çakışması. Islem={Islem}", LogTemizle.Metin(islem));
        }
        else
        {
            logger.LogWarning(
                "İş kuralı ihlali. Islem={Islem}, Tur={HataTuru}, Mesaj={Mesaj}",
                LogTemizle.Metin(islem), ex.GetType().Name, LogTemizle.Metin(ex.Message));
        }

        return false;
    }

    /// <summary>Beklenmeyen hatanın günlüğe yazılacak özeti: yalnızca tür zinciri (ham metin SQL, kullanıcı ya da ürün adı içerebilir).</summary>
    public static string HataOzeti(Exception ex)
    {
        var turler = new List<string>();
        for (Exception? ic = ex; ic is not null && turler.Count < 5; ic = ic.InnerException)
        {
            turler.Add(ic.GetType().Name);
        }

        return string.Join(" <- ", turler);
    }

    /// <summary>Tam istisna (mesaj ve yığın) yalnızca Debug derlemesinde günlüğe eklenir.</summary>
    public static Exception? Ayrinti(Exception ex)
    {
#if DEBUG
        return ex;
#else
        return null;
#endif
    }
}
