using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StokTakip
{
   
    public partial class Form1 : Form
    {
        DatabaseHelper db = new DatabaseHelper();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UrunleriYukle();
            UrunleriComboBoxaYukle();
        }

        public void UrunleriYukle()
        {
            dgvUrunler.DataSource = db.ExecuteQuery("SELECT * FROM Urunler");
        }

        private void UrunleriComboBoxaYukle()
        {
            DataTable dt = db.ExecuteQuery("SELECT * FROM Urunler");
            cmbUrunler.DisplayMember = "UrunAdi";
            cmbUrunler.ValueMember = "UrunID";
            cmbUrunler.DataSource = dt;
        }

        private void btnUrunEkle_Click(object sender, EventArgs e)
        {
            string ad = txtUrunAdi.Text;
            int stok = int.Parse(txtStok.Text);
            db.ExecuteNonQuery($"INSERT INTO Urunler (UrunAdi, Stok) VALUES ('{ad}', {stok})");
            UrunleriYukle();
            UrunleriComboBoxaYukle();
        }

        private void btnSiparisVer_Click(object sender, EventArgs e)
        {
            // Ürün ID ve Adet alınır
            int urunID = (int)cmbUrunler.SelectedValue;

            if (!int.TryParse(txtSiparisAdet.Text, out int adet) || adet <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir adet girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Seçilen ürünün adı alınır (gösterim için)
            string urunAdi = cmbUrunler.Text;

            // Onay mesajı göster
            DialogResult onay = MessageBox.Show(
                $"Ürün: {urunAdi}\nAdet: {adet}\n\nBu siparişi oluşturmak istiyor musunuz?",
                "Sipariş Onayı",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (onay == DialogResult.Yes)
            {
                // Sipariş ekle
                string insertQuery = "INSERT INTO Siparisler (UrunID, Adet, Tarih) VALUES (@UrunID, @Adet, @Tarih)";
                var insertParams = new Dictionary<string, object>
        {
            { "@UrunID", urunID },
            { "@Adet", adet },
            { "@Tarih", DateTime.Now }
        };
                db.ExecuteNonQuery(insertQuery, insertParams);

                // Stok düşür
                string updateQuery = "UPDATE Urunler SET Stok = Stok - @Adet WHERE UrunID = @UrunID";
                var updateParams = new Dictionary<string, object>
        {
            { "@Adet", adet },
            { "@UrunID", urunID }
        };
                db.ExecuteNonQuery(updateQuery, updateParams);

                MessageBox.Show("Sipariş başarıyla oluşturuldu.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Arayüz güncelle
                UrunleriYukle();
                txtSiparisAdet.Clear();
            }
            else
            {
                MessageBox.Show("Sipariş iptal edildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnStokGuncelle_Click(object sender, EventArgs e)
        {
            if (dgvUrunler.SelectedRows.Count > 0)
            {
                // Seçili satırdan UrunID al
                int urunID = Convert.ToInt32(dgvUrunler.SelectedRows[0].Cells["UrunID"].Value);

                // Yeni stok değerini oku
                if (int.TryParse(txtStok.Text, out int yeniStok))
                {
                    db.ExecuteNonQuery($"UPDATE Urunler SET Stok = {yeniStok} WHERE UrunID = {urunID}");
                    MessageBox.Show("Stok güncellendi.");

                    UrunleriYukle();            // DataGridView güncelle
                    UrunleriComboBoxaYukle();   // ComboBox güncelle
                }
                else
                {
                    MessageBox.Show("Lütfen geçerli bir stok miktarı girin.");
                }
            }
            else
            {
                MessageBox.Show("Lütfen güncellenecek bir ürün seçin.");
            }
        }

        private void dgvUrunler_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvUrunler.Rows[e.RowIndex];

                // TextBox'lara aktar
                txtUrunAdi.Text = row.Cells["UrunAdi"].Value.ToString();
                txtStok.Text = row.Cells["Stok"].Value.ToString();

                // ComboBox'ta ilgili ürünü seçili hale getir
                int urunID = Convert.ToInt32(row.Cells["UrunID"].Value);
                cmbUrunler.SelectedValue = urunID;
            }
        }

        private void btnUrunSil_Click(object sender, EventArgs e)
        {
            if (dgvUrunler.SelectedRows.Count > 0)
            {
                int urunID = Convert.ToInt32(dgvUrunler.SelectedRows[0].Cells["UrunID"].Value);

                DialogResult result = MessageBox.Show("Ürünü silmek istediğinizden emin misiniz?", "Onay", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    db.ExecuteNonQuery($"DELETE FROM Urunler WHERE UrunID = {urunID}");
                    MessageBox.Show("Ürün silindi.");

                    UrunleriYukle();
                    UrunleriComboBoxaYukle();
                }
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir ürün seçin.");
            }
        }

        private void btnSiparisGecmisi_Click(object sender, EventArgs e)
        {
            SiparisGecmisiForm siparisForm = new SiparisGecmisiForm(this); // this: Form1
            siparisForm.Show();
        }
    }
}
