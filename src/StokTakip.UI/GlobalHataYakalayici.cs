using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using StokTakip.Presenters;
using StokTakip.Views;
using WinFormsApplication = System.Windows.Forms.Application;

namespace StokTakip
{
    internal static class GlobalHataYakalayici
    {
        private const string GunlukNotu = "\n\nAyrıntılar günlük dosyasına kaydedildi.";

        private static SynchronizationContext _arayuzBaglami;

        private static ILogger Gunlukcu => Log.ForContext(typeof(GlobalHataYakalayici));

        public static void Kaydet(SynchronizationContext arayuzBaglami)
        {
            _arayuzBaglami = arayuzBaglami;
            WinFormsApplication.ThreadException += ArayuzHatasi;
            AppDomain.CurrentDomain.UnhandledException += YakalanmayanHata;
            TaskScheduler.UnobservedTaskException += GozlemlenmeyenGorevHatasi;
        }

        private static void ArayuzHatasi(object sender, ThreadExceptionEventArgs e)
        {
            Gunlukcu.Error(e.Exception, "Arayüz iş parçacığında yakalanmayan hata.");
            Goster(HataMesaji.Olustur(e.Exception) + GunlukNotu);
        }

        private static void YakalanmayanHata(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            Gunlukcu.Fatal(ex, "Yakalanmayan hata. Uygulama sonlanıyor: {Sonlaniyor}", e.IsTerminating);
            if (e.IsTerminating)
            {
                Log.CloseAndFlush();
            }

            string mesaj = ex != null ? HataMesaji.Olustur(ex) : "Beklenmeyen bir hata oluştu.";
            if (e.IsTerminating)
            {
                mesaj += "\n\nUygulama kapatılacak.";
            }

            Goster(mesaj + GunlukNotu);
        }

        private static void GozlemlenmeyenGorevHatasi(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Gunlukcu.Error(e.Exception, "Gözlemlenmeyen arka plan görevi hatası.");
            e.SetObserved();

            Exception ic = e.Exception.InnerException ?? e.Exception;
            string mesaj = "Arka planda yürütülen bir işlem tamamlanamadı.\n\n" + HataMesaji.Olustur(ic) + GunlukNotu;
            if (_arayuzBaglami != null)
            {
                _arayuzBaglami.Post(_ => Goster(mesaj), null);
            }
        }

        private static void Goster(string mesaj)
        {
            try
            {
                MesajKutusu.Hata(null, mesaj);
            }
            catch (Exception ex)
            {
                Gunlukcu.Warning(ex, "Hata penceresi gösterilemedi.");
            }
        }
    }
}
