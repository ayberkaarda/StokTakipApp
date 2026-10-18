using System;
using System.Data.Common;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace StokTakip
{
    internal static class Gunluk
    {
        private const string Sablon =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";

        // Kurulum klasörü (ör. Program Files) yazılabilir olmayabileceği için kullanıcıya ait klasör kullanılır.
        public static string KokKlasor { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StokTakip");

        // Hata metni SQL kullanıcı adı ya da veri değeri içerebilir; günlüğe varsayılan olarak yalnızca tür ve kodlar yazılır.
        public static string HataOzeti(Exception ex)
        {
            if (ex == null)
            {
                return "(yok)";
            }

            string ozet = ex.GetType().Name;
            for (Exception ic = ex; ic != null; ic = ic.InnerException)
            {
                if (ic is SqlException sql)
                {
                    return $"{ozet} <- {sql.GetType().Name} (Number={sql.Number}, State={sql.State}, Class={sql.Class})";
                }

                if (ic is DbException db)
                {
                    return $"{ozet} <- {db.GetType().Name} (ErrorCode={db.ErrorCode}, SqlState={db.SqlState ?? "-"})";
                }
            }

            return ex.InnerException == null ? ozet : $"{ozet} <- {ex.GetBaseException().GetType().Name}";
        }

        // Tam exception (mesaj ve yığın) yalnızca Debug derlemesinde ya da günlük düzeyi Debug iken yazılır.
        public static Exception Ayrinti(Exception ex, bool debugDuzeyiAcik)
        {
#if DEBUG
            return ex;
#else
            return debugDuzeyiAcik ? ex : null;
#endif
        }

        public static Exception Ayrinti(Exception ex) => Ayrinti(ex, Log.IsEnabled(LogEventLevel.Debug));

        public static Logger Olustur(IConfiguration configuration, out Exception yapilandirmaHatasi)
        {
            yapilandirmaHatasi = null;
            if (configuration != null && configuration.GetSection("Serilog").Exists())
            {
                try
                {
                    DosyaYollariniKokle(configuration);
                    return new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                        .CreateLogger();
                }
                catch (Exception ex)
                {
                    yapilandirmaHatasi = ex;
                }
            }

            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.File(
                    Path.Combine(KokKlasor, "logs", "stoktakip-.log"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14,
                    outputTemplate: Sablon)
                .CreateLogger();
        }

        private static void DosyaYollariniKokle(IConfiguration configuration)
        {
            foreach (IConfigurationSection sink in configuration.GetSection("Serilog:WriteTo").GetChildren())
            {
                IConfigurationSection yol = sink.GetSection("Args:path");
                if (string.IsNullOrWhiteSpace(yol.Value))
                {
                    continue;
                }

                string genisletilmis = Environment.ExpandEnvironmentVariables(yol.Value);
                yol.Value = Path.IsPathRooted(genisletilmis)
                    ? genisletilmis
                    : Path.GetFullPath(Path.Combine(KokKlasor, genisletilmis));
            }
        }
    }
}
