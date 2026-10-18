using System;
using System.IO;
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
