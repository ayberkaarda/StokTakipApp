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
    public partial class SiparisGecmisiForm : Form
    {
        private DatabaseHelper db = new DatabaseHelper();
        private Form1 anaForm;


        public SiparisGecmisiForm(Form1 form1)
        {
            
            InitializeComponent();
            this.anaForm = form1;
            db = new DatabaseHelper(); // bağlantı sınıfı

        }

        private void SiparisleriYukle()
        {
            string query = @"
SELECT 
    s.SiparisID,
    s.UrunID,          
    u.UrunAdi,
    s.Adet,
    s.Tarih
FROM Siparisler s
JOIN Urunler u ON s.UrunID = u.UrunID
ORDER BY s.Tarih DESC";

            dgvSiparisGecmisi.DataSource = db.ExecuteQuery(query);
        }

        private void SiparisGecmisiForm_Load(object sender, EventArgs e)
        {
            SiparisleriYukle();

            lblToplamSiparis.Text = "Toplam Sipariş: " + dgvSiparisGecmisi.Rows.Count;
            lblToplamAdet.Text = "Toplam Adet: " + dgvSiparisGecmisi.Rows.Cast<DataGridViewRow>()
                .Sum(r => Convert.ToInt32(r.Cells["Adet"].Value)).ToString();

        }

        private void btnSiparisSil_Click(object sender, EventArgs e)
        {
            if (dgvSiparisGecmisi.SelectedRows.Count > 0)
            {
                int siparisID = Convert.ToInt32(dgvSiparisGecmisi.SelectedRows[0].Cells["SiparisID"].Value);
                int urunID = Convert.ToInt32(dgvSiparisGecmisi.SelectedRows[0].Cells["UrunID"].Value);
                int adet = Convert.ToInt32(dgvSiparisGecmisi.SelectedRows[0].Cells["Adet"].Value);

                // 1. Siparişi sil
                string deleteQuery = "DELETE FROM Siparisler WHERE SiparisID = @id";
                var deleteParams = new Dictionary<string, object> { { "@id", siparisID } };
                db.ExecuteNonQuery(deleteQuery, deleteParams);

                // 2. Stoku geri artır
                string stokUpdateQuery = "UPDATE Urunler SET Stok = Stok + @adet WHERE UrunID = @urunID";
                var stokParams = new Dictionary<string, object>
        {
            { "@adet", adet },
            { "@urunID", urunID }
        };
                db.ExecuteNonQuery(stokUpdateQuery, stokParams);

                // 3. Sipariş geçmişi gridini yenile
                SiparisleriYukle();

                // 4. Ana formda ürün listesi/stok bilgisini güncelle
                anaForm.UrunleriYukle();


                MessageBox.Show("Sipariş silindi ve stok güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir sipariş seçin.");
            }
        }
    }
    }

