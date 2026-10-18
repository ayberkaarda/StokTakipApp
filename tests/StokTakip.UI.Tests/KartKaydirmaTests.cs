using System.Drawing;
using System.Windows.Forms;
using NSubstitute;
using StokTakip.Presenters;

namespace StokTakip.UI.Tests;

/// <summary>Formlar gösterilmez: tanıtıcılar gizli pencere olarak oluşturulur, odak alınmaz.</summary>
public class KartKaydirmaTests
{
    [Fact]
    public void Ilk_kurulum_karti_kucuk_ekranda_kaydirilabilir_ve_alttaki_dugmeye_ulasilir()
        => Sta.Calistir(() =>
        {
            using var form = new IlkKurulumForm(Substitute.For<IIlkKurulumPresenter>());
            KartKaydirilabilir(form, "btnKur", "lblAciklama");
        });

    [Fact]
    public void Giris_karti_kucuk_ekranda_kaydirilabilir_ve_alttaki_dugmeye_ulasilir()
        => Sta.Calistir(() =>
        {
            using var form = new GirisForm(Substitute.For<IGirisPresenter>());
            KartKaydirilabilir(form, "btnGiris", "lblIpucu");
        });

    private static void KartKaydirilabilir(Form form, params string[] alttakiler)
    {
        form.ClientSize = new Size(form.ClientSize.Width, 300);
        TaniticilariOlustur(form);
        form.PerformLayout();
        var kart = Bul<Panel>(form, "pnlKart");
        kart.PerformLayout();

        var icerik = Bul<TableLayoutPanel>(form, "tlpKart");

        Assert.True(kart.AutoScroll, "Kart kaydırılabilir değil.");
        Assert.True(icerik.Bottom + kart.Padding.Bottom > kart.ClientSize.Height, "Senaryo: içerik küçük kartı aşmalı.");
        Assert.True(icerik.Height >= icerik.GetPreferredSize(new Size(icerik.Width, 0)).Height, "İçerik kart yüksekliğine sıkıştırıldı.");
        foreach (var ad in alttakiler)
        {
            var kontrol = Bul<Control>(form, ad);
            int alt = KartaGoreUst(kontrol, icerik) + kontrol.Height;
            Assert.True(alt <= icerik.Height, $"{ad} kırpılıyor: alt={alt}, içerik={icerik.Height}");
        }

        Assert.False(form.Visible);
    }

    private static T Bul<T>(Control kok, string ad) where T : Control => (T)kok.Controls.Find(ad, true).Single();

    private static int KartaGoreUst(Control kontrol, Control kart)
    {
        int y = 0;
        for (Control c = kontrol; c != kart; c = c.Parent!)
        {
            y += c.Top;
        }

        return y;
    }

    private static void TaniticilariOlustur(Control kontrol)
    {
        _ = kontrol.Handle;
        foreach (Control alt in kontrol.Controls)
        {
            TaniticilariOlustur(alt);
        }
    }
}
