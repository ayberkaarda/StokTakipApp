using System;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using StokTakip.Domain.Exceptions;
using StokTakip.Views;

namespace StokTakip.Presenters
{
    internal static class HataMesaji
    {
        public static void Logla(ILogger logger, Exception ex)
        {
            if (ex is DomainException)
            {
                logger.LogDebug("Kullanıcıya iş kuralı uyarısı gösterildi: {HataTuru}", ex.GetType().Name);
            }
            else
            {
                logger.LogError(ex, "Beklenmeyen hata: {HataTuru}", ex.GetType().Name);
            }
        }

        public static void Goster(IMesajView view, Exception ex)
        {
            if (ex is DomainException)
            {
                view.UyariGoster(ex.Message);
            }
            else
            {
                view.HataGoster(Olustur(ex));
            }
        }

        public static string Olustur(Exception ex)
        {
            if (ex is DomainException)
            {
                return ex.Message;
            }

            if (ex is DbException)
            {
                return "Veritabanına erişilemedi. appsettings.json dosyasındaki ConnectionStrings:StokTakip bağlantı dizesini "
                    + "ve SQL Server'ın çalıştığını kontrol edin.\n\nAyrıntı: " + ex.Message;
            }

            for (Exception ic = ex.InnerException; ic != null; ic = ic.InnerException)
            {
                if (ic is DbException)
                {
                    return "Veritabanı işlemi tamamlanamadı.\n\nAyrıntı: " + ic.Message;
                }
            }

            return "Beklenmeyen bir hata oluştu.\n\nAyrıntı: " + ex.Message;
        }
    }
}
