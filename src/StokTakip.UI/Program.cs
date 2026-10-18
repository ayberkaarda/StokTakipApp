using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using StokTakip.Application;
using StokTakip.Data;
using StokTakip.Presenters;
using StokTakip.Views;
using WinFormsApplication = System.Windows.Forms.Application;

namespace StokTakip
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            WinFormsApplication.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            WinFormsApplication.EnableVisualStyles();
            WinFormsApplication.SetCompatibleTextRenderingDefault(false);
            WinFormsApplication.SetDefaultFont(Theme.BodyFont);
            WinFormsApplication.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            WinFormsApplication.ThreadException += (s, e) => MesajKutusu.Hata(null, HataMesaji.Olustur(e.Exception));

            string connectionString;
            try
            {
                connectionString = UygulamaAyarlari.BaglantiDizesiniOku();
            }
            catch (Exception ex)
            {
                MesajKutusu.Hata(null, "Uygulama ayarları okunamadı.\n\n" + ex.Message);
                return;
            }

            using ServiceProvider serviceProvider = ServisleriOlustur(connectionString);
            WinFormsApplication.Run(serviceProvider.GetRequiredService<Form1>());
        }

        private static ServiceProvider ServisleriOlustur(string connectionString)
        {
            var services = new ServiceCollection();
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
