using System.Drawing;
using System.Windows.Forms;
using NSubstitute;
using StokTakip.Application.Denetim;
using StokTakip.Domain;
using StokTakip.Presenters;
using StokTakip.Views;

namespace StokTakip.UI.Tests;

public class DenetimSutunlariTests
{
    private static DenetimKaydiDto Kayit(string tur, int? id, string? ad, string? eski = null, string? yeni = null, int? fark = null) =>
        new(1, new DateTime(2026, 10, 18, 9, 0, 0, DateTimeKind.Utc), 1, "admin", DenetimIslemi.UrunGuncellendi, tur, id, ad, eski, yeni, fark, "özet");

    [Fact]
    public void Varlik_turu_ve_adi_kisa_bicimde_birlesir()
    {
        var presenter = new DenetimKayitlariPresenter(Substitute.For<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>(), TimeProvider.System);

        Assert.Equal("Ürün: Optik Mouse", presenter.SatirOlustur(Kayit("Ürün", 2, "Optik Mouse")).Varlik);
        Assert.Equal("Sipariş #7", presenter.SatirOlustur(Kayit("Sipariş", 7, null)).Varlik);
        Assert.Equal("Oturum", presenter.SatirOlustur(Kayit("Oturum", null, null)).Varlik);
    }

    [Fact]
    public void Varlik_sutunu_varsayilan_boyutta_okunur_bos_degerler_isaretlenir_ve_ipucu_tam_metni_verir()
        => Sta.Calistir(() =>
        {
            using var form = new DenetimKayitlariForm(Substitute.For<IDenetimKayitlariPresenter>());
            TaniticilariOlustur(form);
            form.PerformLayout();

            var satirlar = new[]
            {
                new DenetimSatiri("18.10.2026 12:00:00", "admin", "Ürün güncellendi", "Ürün: USB Bellek 64GB", "", "", "", null, "USB Bellek 64GB ürünü güncellendi"),
                new DenetimSatiri("18.10.2026 11:00:00", "ayse", "Stok güncellendi", "Ürün: Optik Mouse", "8", "40", "+32", 32, "Stok güncellendi: 8 → 40"),
            };
            ((IDenetimKayitlariView)form).SayfayiGoster(satirlar, 1, 1, satirlar.Length);

            var dgv = (DataGridView)form.Controls.Find("dgvKayitlar", true).Single();
            DataGridViewColumn varlik = dgv.Columns["Varlik"]!;
            DataGridViewColumn ozet = dgv.Columns["Ozet"]!;
            Font font = dgv.DefaultCellStyle.Font ?? dgv.Font;
            int gereken = TextRenderer.MeasureText(satirlar[0].Varlik, font).Width + dgv.DefaultCellStyle.Padding.Horizontal;

            Assert.True(varlik.Width >= gereken, $"Varlık sütunu dar: {varlik.Width} < {gereken}");
            Assert.True(varlik.Width >= ozet.Width, $"Varlık ({varlik.Width}) Özet'ten ({ozet.Width}) dar.");
            Assert.Equal(DenetimKayitlariForm.BosDeger, dgv.Rows[0].Cells["EskiDeger"].FormattedValue);
            Assert.Equal(DenetimKayitlariForm.BosDeger, dgv.Rows[0].Cells["Fark"].FormattedValue);
            Assert.Equal("8", dgv.Rows[1].Cells["EskiDeger"].FormattedValue);
            Assert.Equal("Ürün: USB Bellek 64GB", dgv.Rows[0].Cells["Varlik"].ToolTipText);

            form.ClientSize = form.MinimumSize;
            form.PerformLayout();
            Assert.True(varlik.Width >= TextRenderer.MeasureText("Ürün: Optik Mouse", font).Width, $"Küçük pencerede Varlık sütunu çok dar: {varlik.Width}");
            Assert.False(form.Visible);
        });

    private static void TaniticilariOlustur(Control kontrol)
    {
        _ = kontrol.Handle;
        foreach (Control alt in kontrol.Controls)
        {
            TaniticilariOlustur(alt);
        }
    }
}
