using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace StokTakip
{
    internal static class UygulamaAyarlari
    {
        private const string DosyaAdi = "appsettings.json";

        public static string BaglantiDizesiniOku()
        {
            string ayarYolu = Path.Combine(AppContext.BaseDirectory, DosyaAdi);
            if (!File.Exists(ayarYolu))
            {
                throw new FileNotFoundException(
                    "Veritabanı yapılandırma dosyası appsettings.json bulunamadı. Dosyanın uygulama dizininde bulunduğundan emin olun.",
                    ayarYolu);
            }

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(DosyaAdi, optional: false, reloadOnChange: false)
                .Build();

            string connectionString = configuration.GetConnectionString("StokTakip");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "appsettings.json dosyasında ConnectionStrings:StokTakip bağlantı dizesi eksik veya boş.");
            }

            return connectionString;
        }
    }
}
