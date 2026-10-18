using System;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using StokTakip.Domain.Exceptions;
using StokTakip.Views;

namespace StokTakip.Presenters
{
    /// <summary>
    /// Kullanıcıya yalnızca iş kuralı mesajları olduğu gibi gösterilir; veritabanı ve diğer hata metinleri
    /// (tablo, indeks, sunucu, dosya yolu içerebilir) gösterilmez, günlüğe yazılır.
    /// </summary>
    internal static class HataMesaji
    {
        public const string BeklenmeyenHata = "Beklenmeyen bir hata oluştu. Ayrıntılar günlüğe yazıldı.";

        public const string VeritabaninaErisilemedi =
            "Veritabanına erişilemedi. appsettings.json dosyasındaki ConnectionStrings:StokTakip bağlantı dizesini "
            + "ve SQL Server'ın çalıştığını kontrol edin. Ayrıntılar günlüğe yazıldı.";

        public const string VeritabaniIslemiTamamlanamadi = "Veritabanı işlemi tamamlanamadı. Ayrıntılar günlüğe yazıldı.";

        public static void Logla(ILogger logger, Exception ex)
        {
            if (ex is DomainException)
            {
                logger.LogDebug("Kullanıcıya iş kuralı uyarısı gösterildi: {HataTuru}", ex.GetType().Name);
            }
            else
            {
                logger.LogError(Gunluk.Ayrinti(ex, logger.IsEnabled(LogLevel.Debug)), "Beklenmeyen hata: {Hata}", Gunluk.HataOzeti(ex));
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
                return VeritabaninaErisilemedi;
            }

            for (Exception ic = ex?.InnerException; ic != null; ic = ic.InnerException)
            {
                if (ic is DbException)
                {
                    return VeritabaniIslemiTamamlanamadi;
                }
            }

            return BeklenmeyenHata;
        }
    }
}
