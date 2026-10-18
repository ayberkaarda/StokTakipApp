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
}
