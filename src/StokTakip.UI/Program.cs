using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using StokTakip.Application;
using StokTakip.Application.Kimlik;
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

                bool? ilkKurulumGerekli = IlkKurulumDurumunuAl(serviceProvider);
                if (ilkKurulumGerekli == null)
                {
                    return;
                }

                try
                {
                    using (var akis = new UygulamaAkisi(serviceProvider, ilkKurulumGerekli.Value))
                    {
                        WinFormsApplication.Run(akis);
                    }
                }
                finally
                {
                    serviceProvider.GetRequiredService<IOturumBaglami>().OturumuKapat();
                }

                Gunlukcu.Information("Uygulama kapatıldı.");
            }
            catch (Exception ex)
            {
                Gunlukcu.Fatal(Gunluk.Ayrinti(ex), "Uygulama başlatılamadı: {Hata}", Gunluk.HataOzeti(ex));
                MesajKutusu.Hata(null, HataMesaji.Olustur(ex));
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

                    // Yalnızca UygulamaAyarlari'nın kendi açıklayıcı mesajları gösterilir; diğer hatalar dosya yolu içerebilir.
                    string ayrinti = ex is FileNotFoundException || ex is InvalidDataException || ex is InvalidOperationException
                        ? ex.Message
                        : HataMesaji.BeklenmeyenHata;
                    if (!TekrarDenensinMi("Uygulama ayarları okunamadı.\n\n" + ayrinti))
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
                    Gunlukcu.Error(Gunluk.Ayrinti(ex), "Açılışta veritabanına erişilemedi: {Hata}", Gunluk.HataOzeti(ex));
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

        // Oturum gerektirmeyen tek sorgu; arayüz iş parçacığını kilitlememek için iş parçacığı havuzunda beklenir.
        private static bool? IlkKurulumDurumunuAl(IServiceProvider serviceProvider)
        {
            while (true)
            {
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    return Task.Run(async () =>
                    {
                        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
                        return await scope.ServiceProvider.GetRequiredService<IKimlikService>().IlkKurulumGerekliMiAsync();
                    }).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Gunlukcu.Error(Gunluk.Ayrinti(ex), "Açılışta kullanıcı durumu okunamadı: {Hata}", Gunluk.HataOzeti(ex));
                    if (!TekrarDenensinMi(HataMesaji.Olustur(ex)))
                    {
                        Gunlukcu.Information("Veritabanı erişim hatası nedeniyle uygulama kapatıldı.");
                        return null;
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
            services.AddSingleton<IOturumIzleyici, OturumIzleyici>();
            services.AddTransient<IAnaFormPresenter, AnaFormPresenter>();
            services.AddTransient<ISiparisGecmisiPresenter, SiparisGecmisiPresenter>();
            services.AddTransient<IKategorilerPresenter, KategorilerPresenter>();
            services.AddTransient<IRaporlarPresenter, RaporlarPresenter>();
            services.AddTransient<IGirisPresenter, GirisPresenter>();
            services.AddTransient<IIlkKurulumPresenter, IlkKurulumPresenter>();
            services.AddTransient<IParolaDegistirPresenter, ParolaDegistirPresenter>();
            services.AddTransient<IKullanicilarPresenter, KullanicilarPresenter>();
            services.AddTransient<IDenetimKayitlariPresenter, DenetimKayitlariPresenter>();

            // Formlar kök sağlayıcıda izlenmesin diye (her girişte yeni ana form) fabrikayla oluşturulur.
            services.AddTransient<Form1>();
            FormFabrikasi<Form1>(services);
            FormFabrikasi<GirisForm>(services);
            FormFabrikasi<IlkKurulumForm>(services);
            FormFabrikasi<SiparisGecmisiForm>(services);
            FormFabrikasi<KategorilerForm>(services);
            FormFabrikasi<RaporlarForm>(services);
            FormFabrikasi<KullanicilarForm>(services);
            FormFabrikasi<DenetimKayitlariForm>(services);
            FormFabrikasi<ParolaDegistirForm>(services);

            return services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true,
            });
        }

        private static void FormFabrikasi<TForm>(IServiceCollection services)
            where TForm : Form =>
            services.AddSingleton<Func<TForm>>(sp => () => ActivatorUtilities.CreateInstance<TForm>(sp));
    }
}
