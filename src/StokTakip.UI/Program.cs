using System;
using System.Threading;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using StokTakip.Application;
using StokTakip.Data;
using StokTakip.Presenters;
using StokTakip.Views;
using WinFormsApplication = System.Windows.Forms.Application;

namespace StokTakip
{
    internal static class Program
    {
        private const string BaslangicHatasiBasligi = "StokTakip - Başlatma Hatası";

        private static Serilog.ILogger Gunlukcu => Log.ForContext(typeof(Program));

        [STAThread]
        static void Main()
        {
            WinFormsApplication.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            WinFormsApplication.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            WinFormsApplication.EnableVisualStyles();
            WinFormsApplication.SetCompatibleTextRenderingDefault(false);
            WinFormsApplication.SetDefaultFont(Theme.BodyFont);

            var arayuzBaglami = new WindowsFormsSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(arayuzBaglami);

            IConfigurationRoot configuration = AyarlariDeneOku();
            Log.Logger = Gunluk.Olustur(configuration, out Exception gunlukAyarHatasi);
            GlobalHataYakalayici.Kaydet(arayuzBaglami);

            try
            {
                Gunlukcu.Information("Uygulama başlatılıyor. Sürüm {Surum}", typeof(Program).Assembly.GetName().Version);
                if (gunlukAyarHatasi != null)
                {
                    Gunlukcu.Warning(gunlukAyarHatasi, "Serilog ayarları okunamadı, varsayılan günlük ayarları kullanılıyor.");
                }

                string connectionString = BaglantiDizesiniAl(configuration);
                if (connectionString == null)
                {
                    return;
                }

                using ServiceProvider serviceProvider = ServisleriOlustur(connectionString);
                if (!VeritabaninaErisilebilir(serviceProvider))
                {
                    return;
                }

                WinFormsApplication.Run(serviceProvider.GetRequiredService<Form1>());
                Gunlukcu.Information("Uygulama kapatıldı.");
            }
            catch (Exception ex)
            {
                Gunlukcu.Fatal(ex, "Uygulama başlatılamadı.");
                MesajKutusu.Hata(null, HataMesaji.Olustur(ex) + "\n\nAyrıntılar günlük dosyasına kaydedildi.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IConfigurationRoot AyarlariDeneOku()
        {
            try
            {
                return UygulamaAyarlari.Yukle();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string BaglantiDizesiniAl(IConfigurationRoot configuration)
        {
            while (true)
            {
                try
                {
                    configuration ??= UygulamaAyarlari.Yukle();
                    return UygulamaAyarlari.BaglantiDizesiniOku(configuration);
                }
                catch (Exception ex)
                {
                    configuration = null;
                    Gunlukcu.Error(ex, "Uygulama ayarları okunamadı.");
                    if (!TekrarDenensinMi("Uygulama ayarları okunamadı.\n\n" + ex.Message))
                    {
                        Gunlukcu.Information("Ayar hatası nedeniyle uygulama kapatıldı.");
                        return null;
                    }
                }
            }
        }

        private static bool VeritabaninaErisilebilir(IServiceProvider serviceProvider)
        {
            while (true)
            {
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    using IServiceScope scope = serviceProvider.CreateScope();
                    DatabaseFacade database = scope.ServiceProvider.GetRequiredService<StokTakipDbContext>().Database;
                    database.OpenConnection();
                    database.CloseConnection();
                    return true;
                }
                catch (Exception ex)
                {
                    Gunlukcu.Error(ex, "Açılışta veritabanına erişilemedi.");
                    if (!TekrarDenensinMi(HataMesaji.Olustur(ex)))
                    {
                        Gunlukcu.Information("Veritabanı erişim hatası nedeniyle uygulama kapatıldı.");
                        return false;
                    }
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        private static bool TekrarDenensinMi(string mesaj) =>
            MessageBox.Show(
                mesaj + "\n\nTekrar denemek ister misiniz? İptal ederseniz uygulama kapatılır.",
                BaslangicHatasiBasligi,
                MessageBoxButtons.RetryCancel,
                MessageBoxIcon.Error) == DialogResult.Retry;

        private static ServiceProvider ServisleriOlustur(string connectionString)
        {
            var services = new ServiceCollection();
            services.AddLogging(builder => builder
                .SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace)
                .AddSerilog(dispose: false));
            services.AddApplication();
            services.AddData(connectionString);

            services.AddSingleton<IStokDegisiklikBildirimi, StokDegisiklikBildirimi>();
            services.AddTransient<IAnaFormPresenter, AnaFormPresenter>();
            services.AddTransient<ISiparisGecmisiPresenter, SiparisGecmisiPresenter>();

            services.AddTransient<Form1>();
            services.AddSingleton<Func<SiparisGecmisiForm>>(
                sp => () => ActivatorUtilities.CreateInstance<SiparisGecmisiForm>(sp));

            return services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true,
            });
        }
    }
}
