using System;
using System.IO;
using System.Windows.Forms;
using StokTakip.Domain.Exceptions;

namespace StokTakip
{
    /// <summary>
    /// Hedefle aynı klasördeki geçici dosyaya yazar; yalnızca <see cref="Tamamla"/> çağrılınca hedefin yerine taşır.
    /// Hata ya da iptalde geçici dosya silinir, hedef dosyaya hiç dokunulmaz. Hedef yalnızca uygulama ve günlük
    /// klasörleri dışındaki bir .csv dosyası olabilir.
    /// </summary>
    internal sealed class GuvenliDosyaYazimi : IDisposable
    {
        internal const string CsvFiltresi = "CSV dosyası (*.csv)|*.csv";

        internal const string GecersizHedefMesaji =
            "CSV yalnızca .csv uzantılı bir dosyaya kaydedilebilir; uygulama ve günlük klasörleri hedef olarak seçilemez.";

        private const string CsvUzantisi = ".csv";

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

        /// <summary>Uzantısız yola .csv eklenir; başka uzantı, klasör, bağlantı noktası ya da korumalı klasör reddedilir.</summary>
        /// <exception cref="GecersizGirdiException"/>
        internal static string HedefYoluDogrula(string hedefYol)
        {
            if (string.IsNullOrWhiteSpace(hedefYol))
            {
                throw new GecersizGirdiException(GecersizHedefMesaji);
            }

            string hedef = Path.GetFullPath(hedefYol);
            string uzanti = Path.GetExtension(hedef);
            if (uzanti.Length == 0)
            {
                hedef += CsvUzantisi;
            }
            else if (!string.Equals(uzanti, CsvUzantisi, StringComparison.OrdinalIgnoreCase))
            {
                throw new GecersizGirdiException(GecersizHedefMesaji);
            }

            if (KlasorIcinde(hedef, AppContext.BaseDirectory) || KlasorIcinde(hedef, Gunluk.KokKlasor) || Directory.Exists(hedef))
            {
                throw new GecersizGirdiException(GecersizHedefMesaji);
            }

            BaglantiDegil(hedef);
            return hedef;
        }

        public static GuvenliDosyaYazimi Ac(string hedefYol)
        {
            string hedef = HedefYoluDogrula(hedefYol);
            string klasor = Path.GetDirectoryName(hedef) ?? throw new IOException("Hedef klasör belirlenemedi.");
            string gecici = Path.Combine(klasor, $".{Path.GetFileName(hedef)}.{Guid.NewGuid():N}.tmp");
            var akis = new FileStream(gecici, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
            return new GuvenliDosyaYazimi(hedef, gecici, akis);
        }

        public void Tamamla()
        {
            Akis.Flush();
            Akis.Dispose();
            BaglantiDegil(HedefYol);
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
                diyalog.Filter = CsvFiltresi;
                diyalog.DefaultExt = "csv";
                diyalog.AddExtension = true;
                diyalog.OverwritePrompt = true;
                diyalog.FileName = varsayilanDosyaAdi;
                return diyalog.ShowDialog(sahip) == DialogResult.OK ? diyalog.FileName : null;
            }
        }

        private static bool KlasorIcinde(string yol, string klasor)
        {
            string kok = Path.TrimEndingDirectorySeparator(Path.GetFullPath(klasor)) + Path.DirectorySeparatorChar;
            return yol.StartsWith(kok, StringComparison.OrdinalIgnoreCase);
        }

        // Mevcut hedef sembolik bağlantı/bağlantı noktasıysa taşıma başka bir dosyayı ezebilir.
        private static void BaglantiDegil(string hedef)
        {
            if (File.Exists(hedef) && File.GetAttributes(hedef).HasFlag(FileAttributes.ReparsePoint))
            {
                throw new GecersizGirdiException(GecersizHedefMesaji);
            }
        }
    }
}
