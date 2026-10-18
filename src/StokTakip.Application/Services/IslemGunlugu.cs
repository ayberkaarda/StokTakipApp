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
            logger.LogWarning("Eşzamanlılık çakışması. Islem={Islem}", islem);
        }
        else
        {
            logger.LogWarning(
                "İş kuralı ihlali. Islem={Islem}, Tur={HataTuru}, Mesaj={Mesaj}",
                islem, ex.GetType().Name, ex.Message);
        }

        return false;
    }
}
