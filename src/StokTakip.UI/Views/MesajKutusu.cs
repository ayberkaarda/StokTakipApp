using System.Windows.Forms;

namespace StokTakip.Views
{
    internal static class MesajKutusu
    {
        public static void Bilgi(IWin32Window sahip, string mesaj) =>
            MessageBox.Show(sahip, mesaj, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

        public static void Uyari(IWin32Window sahip, string mesaj) =>
            MessageBox.Show(sahip, mesaj, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        public static void Hata(IWin32Window sahip, string mesaj) =>
            MessageBox.Show(sahip, mesaj, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);

        public static bool Onay(IWin32Window sahip, string mesaj, string baslik) =>
            MessageBox.Show(sahip, mesaj, baslik, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }
}
