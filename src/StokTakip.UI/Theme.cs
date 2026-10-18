using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using StokTakip.Domain;

namespace StokTakip
{
    /// <summary>
    /// Uygulama genelindeki renk, yazı tipi ve kontrol stillerini tek yerde toplar.
    /// Yerleşim tasarımcı dosyalarındadır; kontroller rollerini <see cref="Control.Tag"/>
    /// üzerinden bildirir (ör. "primary", "card", "title") ve <see cref="Apply(Form)"/>
    /// bu rollere göre görünümü uygular. İş mantığı içermez.
    /// </summary>
    internal static class Theme
    {
        /// <summary>Renk paleti.</summary>
        public static class Palette
        {
            public static readonly Color Background = Color.FromArgb(243, 245, 249);
            public static readonly Color Surface = Color.White;
            public static readonly Color SurfaceMuted = Color.FromArgb(248, 250, 252);
            public static readonly Color Border = Color.FromArgb(226, 232, 240);
            public static readonly Color BorderStrong = Color.FromArgb(203, 213, 225);

            public static readonly Color Text = Color.FromArgb(15, 23, 42);
            public static readonly Color TextSecondary = Color.FromArgb(51, 65, 85);
            public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);

            public static readonly Color Primary = Color.FromArgb(37, 99, 235);
            public static readonly Color PrimaryHover = Color.FromArgb(29, 78, 216);
            public static readonly Color PrimaryPressed = Color.FromArgb(30, 64, 175);
            public static readonly Color PrimarySoft = Color.FromArgb(219, 234, 254);

            public static readonly Color Neutral = Color.FromArgb(241, 245, 249);
            public static readonly Color NeutralPressed = Color.FromArgb(226, 232, 240);

            public static readonly Color Success = Color.FromArgb(21, 128, 61);
            public static readonly Color SuccessSoft = Color.FromArgb(220, 252, 231);
            public static readonly Color Warning = Color.FromArgb(180, 83, 9);
            public static readonly Color WarningSoft = Color.FromArgb(254, 243, 199);
            public static readonly Color Danger = Color.FromArgb(220, 38, 38);
            public static readonly Color DangerHover = Color.FromArgb(254, 242, 242);
            public static readonly Color DangerSoft = Color.FromArgb(254, 226, 226);
            public static readonly Color DangerBorder = Color.FromArgb(252, 165, 165);

            public static readonly Color LogoTileTop = Color.FromArgb(59, 130, 246);
            public static readonly Color LogoTileBottom = Color.FromArgb(29, 78, 216);
            public static readonly Color LogoLid = Color.FromArgb(191, 219, 254);
        }

        /// <summary>Tasarımcıda Tag olarak kullanılan rol adları.</summary>
        public static class Roles
        {
            public const string Header = "header";
            public const string Card = "card";
            public const string Logo = "logo";
            public const string Title = "title";
            public const string Subtitle = "subtitle";
            public const string Section = "section";
            public const string Hint = "hint";
            public const string Field = "field";
            public const string StatCaption = "stat-caption";
            public const string StatValue = "stat-value";
            public const string LegendCritical = "legend-critical";
            public const string LegendLow = "legend-low";
            public const string LegendOk = "legend-ok";
            public const string Primary = "primary";
            public const string Secondary = "secondary";
            public const string Danger = "danger";
            public const string Badge = "badge";
            public const string Tab = "tab";
        }

        private const string FontFamily = "Segoe UI";
        private const string FontFamilySemibold = "Segoe UI Semibold";

        public static readonly Font BodyFont = new Font(FontFamily, 9.75F);
        public static readonly Font InputFont = new Font(FontFamily, 10F);
        public static readonly Font SmallFont = new Font(FontFamily, 9F);
        public static readonly Font LabelFont = new Font(FontFamilySemibold, 9F);
        public static readonly Font ButtonFont = new Font(FontFamilySemibold, 9.75F);
        public static readonly Font SectionFont = new Font(FontFamilySemibold, 11.25F);
        public static readonly Font TitleFont = new Font(FontFamilySemibold, 15F);
        public static readonly Font StatValueFont = new Font(FontFamilySemibold, 20F);
        public static readonly Font GridHeaderFont = new Font(FontFamilySemibold, 9F);
        public static readonly Font GridEmphasisFont = new Font(FontFamilySemibold, 9.75F);

        public static readonly Image Logo = KaynakResmiYukle("StokTakip.logo-64.png");
        public static readonly Icon UygulamaSimgesi = KaynakSimgesiYukle("StokTakip.app.ico");

        private static readonly Dictionary<string, string> ColumnHeaders =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "UrunID", "Ürün No" },
                { "UrunAdi", "Ürün Adı" },
                { "Stok", "Stok" },
                { "SiparisID", "Sipariş No" },
                { "Adet", "Adet" },
                { "Tarih", "Tarih" },
                { "KategoriAdi", "Kategori" },
                { "DusukStokEsigi", "Eşik" },
                { "ToplamAdet", "Toplam Adet" },
                { "SiparisSayisi", "Sipariş Sayısı" },
            };

        private static readonly HashSet<DataGridView> SunumUygulanan = new HashSet<DataGridView>();

        /// <summary>Formun ve tüm alt kontrollerinin görünümünü rollere göre uygular.</summary>
        public static void Apply(Form form)
        {
            form.BackColor = Palette.Background;
            form.ForeColor = Palette.Text;
            if (UygulamaSimgesi != null)
            {
                form.Icon = UygulamaSimgesi;
            }

            ApplyToChildren(form);
        }

        /// <summary>
        /// Yüksek DPI'da ölçeklenen pencere ve minimum boyut ekranın çalışma alanını aşmasın;
        /// gerekirse pencere küçültülüp ortalanır.
        /// </summary>
        public static void EkranaSigdir(Form form)
        {
            Rectangle alan = Screen.FromControl(form).WorkingArea;
            Size enKucuk = form.MinimumSize;
            form.MinimumSize = new Size(Math.Min(enKucuk.Width, alan.Width), Math.Min(enKucuk.Height, alan.Height));

            if (alan.Contains(form.Bounds))
            {
                return;
            }

            int genislik = Math.Min(form.Width, alan.Width);
            int yukseklik = Math.Min(form.Height, alan.Height);
            form.Bounds = new Rectangle(
                Math.Clamp(form.Left, alan.Left, alan.Right - genislik),
                Math.Clamp(form.Top, alan.Top, alan.Bottom - yukseklik),
                genislik,
                yukseklik);
        }

        private static Image KaynakResmiYukle(string ad)
        {
            using (Stream akis = typeof(Theme).Assembly.GetManifestResourceStream(ad))
            {
                if (akis == null)
                {
                    return null;
                }

                using (Image resim = Image.FromStream(akis))
                {
                    return new Bitmap(resim);
                }
            }
        }

        private static Icon KaynakSimgesiYukle(string ad)
        {
            using (Stream akis = typeof(Theme).Assembly.GetManifestResourceStream(ad))
            {
                return akis == null ? null : new Icon(akis);
            }
        }

        private static void ApplyToChildren(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                StyleControl(child);
                if (child.HasChildren && !(child is DataGridView) && !(child is ParolaIpucuPaneli))
                {
                    ApplyToChildren(child);
                }
            }
        }

        private static void StyleControl(Control control)
        {
            string role = control.Tag as string;

            switch (control)
            {
                case Button button:
                    StyleButton(button, role);
                    break;
                case DataGridView grid:
                    StyleGrid(grid);
                    break;
                case TextBox textBox:
                    textBox.Font = InputFont;
                    textBox.BackColor = Palette.Surface;
                    textBox.ForeColor = Palette.Text;
                    break;
                case ComboBox comboBox:
                    comboBox.Font = InputFont;
                    comboBox.BackColor = Palette.Surface;
                    comboBox.ForeColor = Palette.Text;
                    break;
                case DateTimePicker dateTimePicker:
                    dateTimePicker.Font = InputFont;
                    break;
                case NumericUpDown numericUpDown:
                    numericUpDown.Font = InputFont;
                    numericUpDown.BackColor = Palette.Surface;
                    numericUpDown.ForeColor = Palette.Text;
                    break;
                case RadioButton radio when role == Roles.Tab:
                    StyleTab(radio);
                    break;
                case CheckBox checkBox:
                    checkBox.Font = BodyFont;
                    checkBox.ForeColor = Palette.TextSecondary;
                    checkBox.Cursor = Cursors.Hand;
                    break;
                case UyariKutusu _:
                case ParolaIpucuPaneli _:
                    break;
                case Label label:
                    StyleLabel(label, role);
                    break;
                case Panel panel:
                    StylePanel(panel, role);
                    break;
                case PictureBox pictureBox when role == Roles.Logo:
                    pictureBox.BackColor = Color.Transparent;
                    pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox.Image = Logo;
                    break;
            }
        }

        private static void StylePanel(Panel panel, string role)
        {
            switch (role)
            {
                case Roles.Header:
                    panel.BackColor = Palette.Surface;
                    panel.Paint += PaintBottomBorder;
                    break;
                case Roles.Card:
                    panel.BackColor = Palette.Surface;
                    panel.Paint += PaintCardBorder;
                    panel.Resize += (s, e) => ((Control)s).Invalidate();
                    panel.Scroll += (s, e) => ((Control)s).Invalidate();
                    break;
            }
        }

        private static void PaintCardBorder(object sender, PaintEventArgs e)
        {
            var panel = (Control)sender;
            using (var pen = new Pen(Palette.Border))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, panel.ClientSize.Width - 1, panel.ClientSize.Height - 1);
            }
        }

        private static void PaintBottomBorder(object sender, PaintEventArgs e)
        {
            var panel = (Control)sender;
            int y = panel.ClientSize.Height - 1;
            using (var pen = new Pen(Palette.Border))
            {
                e.Graphics.DrawLine(pen, 0, y, panel.ClientSize.Width, y);
            }
        }

        private static void StyleLabel(Label label, string role)
        {
            switch (role)
            {
                case Roles.Title:
                    label.Font = TitleFont;
                    label.ForeColor = Palette.Text;
                    break;
                case Roles.Subtitle:
                    label.Font = SmallFont;
                    label.ForeColor = Palette.TextMuted;
                    break;
                case Roles.Section:
                    label.Font = SectionFont;
                    label.ForeColor = Palette.Text;
                    break;
                case Roles.Hint:
                    label.Font = SmallFont;
                    label.ForeColor = Palette.TextMuted;
                    break;
                case Roles.Field:
                    label.Font = LabelFont;
                    label.ForeColor = Palette.TextSecondary;
                    break;
                case Roles.StatCaption:
                    label.Font = LabelFont;
                    label.ForeColor = Palette.TextMuted;
                    break;
                case Roles.StatValue:
                    label.Font = StatValueFont;
                    label.ForeColor = Palette.Text;
                    break;
                case Roles.LegendCritical:
                    StyleLegend(label, Palette.Danger, "Kritik: stok ≤ eşiğin yarısı");
                    break;
                case Roles.LegendLow:
                    StyleLegend(label, Palette.Warning, "Düşük: stok ≤ eşik");
                    break;
                case Roles.LegendOk:
                    StyleLegend(label, Palette.Success, "Yeterli: stok > eşik");
                    break;
                case Roles.Badge:
                    label.Font = LabelFont;
                    label.ForeColor = Palette.Warning;
                    label.BackColor = Palette.WarningSoft;
                    label.Cursor = Cursors.Hand;
                    label.Paint += PaintBadgeBorder;
                    break;
                default:
                    label.ForeColor = Palette.Text;
                    break;
            }
        }

        private static void StyleLegend(Label label, Color color, string text)
        {
            label.Font = SmallFont;
            label.ForeColor = color;
            label.Text = "● " + text;
        }

        /// <summary>Otomatik boyutlu, çok satırlı etiketin kapsayıcı genişliğinde satır kırmasını sağlar.</summary>
        public static void SatirKir(Label label, Control kapsayici)
        {
            void Ayarla()
            {
                int genislik = Math.Max(1, kapsayici.ClientSize.Width - kapsayici.Padding.Horizontal - label.Margin.Horizontal);
                if (label.MaximumSize.Width != genislik || label.MinimumSize.Width != genislik)
                {
                    label.MaximumSize = new Size(genislik, 0);
                    label.MinimumSize = new Size(genislik, 0);
                }
            }

            // DPI ölçeklemesi en küçük/en büyük boyutu da ölçeklediği için her yerleşimde yeniden hesaplanır.
            Ayarla();
            kapsayici.Layout += (s, e) => Ayarla();
        }

        private static void PaintBadgeBorder(object sender, PaintEventArgs e)
        {
            var label = (Control)sender;
            using (var pen = new Pen(Color.FromArgb(253, 230, 138)))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, label.ClientSize.Width - 1, label.ClientSize.Height - 1);
            }
        }

        private static void StyleTab(RadioButton radio)
        {
            radio.Appearance = Appearance.Button;
            radio.FlatStyle = FlatStyle.Flat;
            radio.UseVisualStyleBackColor = false;
            radio.Font = ButtonFont;
            radio.Cursor = Cursors.Hand;
            radio.TextAlign = ContentAlignment.MiddleCenter;
            radio.BackColor = Palette.Surface;
            radio.FlatAppearance.BorderSize = 1;
            radio.FlatAppearance.CheckedBackColor = Palette.PrimarySoft;
            radio.FlatAppearance.MouseOverBackColor = Palette.Neutral;
            radio.FlatAppearance.MouseDownBackColor = Palette.NeutralPressed;

            void Guncelle()
            {
                radio.ForeColor = radio.Checked ? Palette.PrimaryPressed : Palette.TextSecondary;
                radio.FlatAppearance.BorderColor = radio.Checked ? Palette.Primary : Palette.BorderStrong;
            }

            Guncelle();
            radio.CheckedChanged += (s, e) => Guncelle();
        }

        private static readonly Color PrimaryDisabled = Color.FromArgb(191, 219, 254);

        private static void StyleButton(Button button, string role)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.UseVisualStyleBackColor = false;
            button.Font = ButtonFont;
            button.Cursor = Cursors.Hand;
            button.FlatAppearance.BorderSize = 1;

            switch (role)
            {
                case Roles.Primary:
                    button.BackColor = Palette.Primary;
                    button.ForeColor = Color.White;
                    button.FlatAppearance.BorderSize = 0;
                    button.FlatAppearance.MouseOverBackColor = Palette.PrimaryHover;
                    button.FlatAppearance.MouseDownBackColor = Palette.PrimaryPressed;

                    // Pasif birincil düğme açık zeminle gösterilir (koyu metin mavi zeminde okunmuyordu).
                    button.BackColor = button.Enabled ? Palette.Primary : PrimaryDisabled;
                    button.EnabledChanged += (s, e) => button.BackColor = button.Enabled ? Palette.Primary : PrimaryDisabled;
                    break;
                case Roles.Danger:
                    button.BackColor = Palette.Surface;
                    button.ForeColor = Palette.Danger;
                    button.FlatAppearance.BorderColor = Palette.DangerBorder;
                    button.FlatAppearance.MouseOverBackColor = Palette.DangerHover;
                    button.FlatAppearance.MouseDownBackColor = Palette.DangerSoft;
                    break;
                default:
                    button.BackColor = Palette.Surface;
                    button.ForeColor = Palette.Text;
                    button.FlatAppearance.BorderColor = Palette.BorderStrong;
                    button.FlatAppearance.MouseOverBackColor = Palette.Neutral;
                    button.FlatAppearance.MouseDownBackColor = Palette.NeutralPressed;
                    break;
            }
        }

        /// <summary>DataGridView için ortak tablo görünümü.</summary>
        public static void StyleGrid(DataGridView grid)
        {
            grid.BorderStyle = BorderStyle.None;
            grid.BackgroundColor = Palette.Surface;
            grid.GridColor = Palette.Border;
            grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.RowHeadersVisible = false;
            grid.AllowUserToResizeRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.EnableHeadersVisualStyles = false;

            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight = Scale(grid, 40);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Palette.SurfaceMuted;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Palette.TextMuted;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Palette.SurfaceMuted;
            grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Palette.TextMuted;
            grid.ColumnHeadersDefaultCellStyle.Font = GridHeaderFont;
            grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(Scale(grid, 8), 0, Scale(grid, 8), 0);
            grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

            grid.RowTemplate.Height = Scale(grid, 36);
            grid.DefaultCellStyle.BackColor = Palette.Surface;
            grid.DefaultCellStyle.ForeColor = Palette.Text;
            grid.DefaultCellStyle.Font = BodyFont;
            grid.DefaultCellStyle.SelectionBackColor = Palette.PrimarySoft;
            grid.DefaultCellStyle.SelectionForeColor = Palette.Text;
            grid.DefaultCellStyle.Padding = new Padding(Scale(grid, 8), 0, Scale(grid, 8), 0);
            grid.AlternatingRowsDefaultCellStyle.BackColor = Palette.SurfaceMuted;

            grid.DataBindingComplete += (s, e) => ApplyColumnPresentation((DataGridView)s);
        }

        /// <summary>Veri bağlandıktan sonra sütun başlıklarını, hizalamayı ve biçimleri ayarlar.</summary>
        private static void ApplyColumnPresentation(DataGridView grid)
        {
            if (grid.Columns.Count == 0 || !SunumUygulanan.Add(grid))
            {
                return;
            }

            // Fill modunda sütunların MinimumWidth toplamı ızgara genişliğini aşarsa WinForms
            // DataGridViewBand.Thickness içinde NullReferenceException atar; en küçük genişlikler
            // otomatik boyutlandırma kapalıyken verilir, Fill sonra yeniden açılır (sığmazsa yatay kaydırma çıkar).
            DataGridViewAutoSizeColumnsMode boyutlandirma = grid.AutoSizeColumnsMode;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            try
            {
                foreach (DataGridViewColumn column in grid.Columns)
                {
                    ApplyColumnPresentation(grid, column);
                }
            }
            finally
            {
                grid.AutoSizeColumnsMode = boyutlandirma;
                SunumUygulanan.Remove(grid);
            }
        }

        private static void ApplyColumnPresentation(DataGridView grid, DataGridViewColumn column)
        {
            string key = string.IsNullOrEmpty(column.DataPropertyName) ? column.Name : column.DataPropertyName;
            if (ColumnHeaders.TryGetValue(key, out string header))
            {
                column.HeaderText = header;
            }

            DataGridViewCellStyle headerStyle = grid.ColumnHeadersDefaultCellStyle;
            column.MinimumWidth = Math.Max(2, TextRenderer.MeasureText(column.HeaderText, headerStyle.Font ?? grid.Font).Width
                + headerStyle.Padding.Horizontal + Scale(grid, 12));

            Type type = Nullable.GetUnderlyingType(column.ValueType ?? typeof(object)) ?? column.ValueType;
            bool isId = key.EndsWith("ID", StringComparison.OrdinalIgnoreCase);

            if (isId)
            {
                column.FillWeight = 45;
                column.DefaultCellStyle.ForeColor = Palette.TextMuted;
            }
            else if (type == typeof(DateTime))
            {
                column.FillWeight = 90;
                column.DefaultCellStyle.Format = "dd.MM.yyyy HH:mm";
            }
            else if (IsNumeric(type))
            {
                column.FillWeight = 60;
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            else
            {
                column.FillWeight = 140;
            }
        }

        /// <summary>
        /// Belirtilen sütundaki stok değerlerini satırın stok durumuna (ürün eşiğine göre) renklendirir
        /// ve yanına durum etiketi ekler. Hücrenin gerçek değeri değişmez.
        /// </summary>
        public static void EnableStockHighlight(DataGridView grid, string columnName, Func<object, StokDurumu?> durumGetir)
        {
            grid.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count || e.Value == null || e.Value == DBNull.Value)
                {
                    return;
                }

                var column = grid.Columns[e.ColumnIndex];
                if (!string.Equals(column.Name, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                switch (durumGetir(grid.Rows[e.RowIndex].DataBoundItem))
                {
                    case StokDurumu.Kritik:
                        SetStatusStyle(e, Palette.Danger, Palette.DangerSoft, e.Value + "  ·  Kritik");
                        break;
                    case StokDurumu.Dusuk:
                        SetStatusStyle(e, Palette.Warning, Palette.WarningSoft, e.Value + "  ·  Düşük");
                        break;
                }
            };
        }

        private static void SetStatusStyle(DataGridViewCellFormattingEventArgs e, Color fore, Color back, string text)
        {
            e.CellStyle.ForeColor = fore;
            e.CellStyle.BackColor = back;
            e.CellStyle.SelectionForeColor = fore;
            e.CellStyle.Font = GridEmphasisFont;
            e.Value = text;
            e.FormattingApplied = true;
        }

        private static bool IsNumeric(Type type)
        {
            return type == typeof(int) || type == typeof(long) || type == typeof(short)
                || type == typeof(decimal) || type == typeof(double) || type == typeof(float);
        }

        private static int Scale(Control control, int value)
        {
            return (int)Math.Round(value * control.DeviceDpi / 96.0);
        }
    }
}
