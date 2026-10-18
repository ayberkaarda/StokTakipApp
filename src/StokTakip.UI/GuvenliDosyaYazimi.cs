using System;
using System.IO;
using System.Windows.Forms;

namespace StokTakip
{
    /// <summary>
    /// Hedefle aynı klasördeki geçici dosyaya yazar; yalnızca <see cref="Tamamla"/> çağrılınca hedefin yerine taşır.
    /// Hata ya da iptalde geçici dosya silinir, hedef dosyaya hiç dokunulmaz.
    /// </summary>
    internal sealed class GuvenliDosyaYazimi : IDisposable
    {
        private bool _tamamlandi;

        private GuvenliDosyaYazimi(string hedefYol, string geciciYol, Stream akis)
        {
            HedefYol = hedefYol;
            GeciciYol = geciciYol;
            Akis = akis;
        }

        public string HedefYol { get; }

        public string GeciciYol { get; }

        public Stream Akis { get; }

        public static GuvenliDosyaYazimi Ac(string hedefYol)
        {
            string hedef = Path.GetFullPath(hedefYol);
            string klasor = Path.GetDirectoryName(hedef) ?? throw new IOException("Hedef klasör belirlenemedi.");
            string gecici = Path.Combine(klasor, $".{Path.GetFileName(hedef)}.{Guid.NewGuid():N}.tmp");
            var akis = new FileStream(gecici, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            return new GuvenliDosyaYazimi(hedef, gecici, akis);
        }

        public void Tamamla()
        {
            Akis.Flush();
            Akis.Dispose();
            File.Move(GeciciYol, HedefYol, overwrite: true);
            _tamamlandi = true;
        }

        public void Dispose()
        {
            Akis.Dispose();
            if (_tamamlandi)
            {
                return;
            }

            try
            {
                File.Delete(GeciciYol);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        public static string CsvDosyasiSec(IWin32Window sahip, string baslik, string varsayilanDosyaAdi)
        {
            using (var diyalog = new SaveFileDialog())
            {
                diyalog.Title = baslik;
                diyalog.Filter = "CSV dosyası (*.csv)|*.csv|Tüm dosyalar (*.*)|*.*";
                diyalog.DefaultExt = "csv";
                diyalog.AddExtension = true;
                diyalog.OverwritePrompt = true;
                diyalog.FileName = varsayilanDosyaAdi;
                return diyalog.ShowDialog(sahip) == DialogResult.OK ? diyalog.FileName : null;
            }
        }
    }
}
