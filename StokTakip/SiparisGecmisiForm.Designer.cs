namespace StokTakip
{
    partial class SiparisGecmisiForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dgvSiparisGecmisi = new System.Windows.Forms.DataGridView();
            this.lblToplamSiparis = new System.Windows.Forms.Label();
            this.lblToplamAdet = new System.Windows.Forms.Label();
            this.btnSiparisSil = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSiparisGecmisi)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvSiparisGecmisi
            // 
            this.dgvSiparisGecmisi.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSiparisGecmisi.Location = new System.Drawing.Point(12, 12);
            this.dgvSiparisGecmisi.Name = "dgvSiparisGecmisi";
            this.dgvSiparisGecmisi.RowTemplate.Height = 24;
            this.dgvSiparisGecmisi.Size = new System.Drawing.Size(599, 215);
            this.dgvSiparisGecmisi.TabIndex = 0;
            // 
            // lblToplamSiparis
            // 
            this.lblToplamSiparis.AutoSize = true;
            this.lblToplamSiparis.Location = new System.Drawing.Point(38, 254);
            this.lblToplamSiparis.Name = "lblToplamSiparis";
            this.lblToplamSiparis.Size = new System.Drawing.Size(44, 16);
            this.lblToplamSiparis.TabIndex = 1;
            this.lblToplamSiparis.Text = "label1";
            // 
            // lblToplamAdet
            // 
            this.lblToplamAdet.AutoSize = true;
            this.lblToplamAdet.Location = new System.Drawing.Point(38, 282);
            this.lblToplamAdet.Name = "lblToplamAdet";
            this.lblToplamAdet.Size = new System.Drawing.Size(44, 16);
            this.lblToplamAdet.TabIndex = 2;
            this.lblToplamAdet.Text = "label2";
            // 
            // btnSiparisSil
            // 
            this.btnSiparisSil.Location = new System.Drawing.Point(247, 254);
            this.btnSiparisSil.Name = "btnSiparisSil";
            this.btnSiparisSil.Size = new System.Drawing.Size(209, 38);
            this.btnSiparisSil.TabIndex = 3;
            this.btnSiparisSil.Text = "Seçili Siparişi Sil";
            this.btnSiparisSil.UseVisualStyleBackColor = true;
            this.btnSiparisSil.Click += new System.EventHandler(this.btnSiparisSil_Click);
            // 
            // SiparisGecmisiForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(623, 450);
            this.Controls.Add(this.btnSiparisSil);
            this.Controls.Add(this.lblToplamAdet);
            this.Controls.Add(this.lblToplamSiparis);
            this.Controls.Add(this.dgvSiparisGecmisi);
            this.Name = "SiparisGecmisiForm";
            this.Text = "SiparisGecmisiForm";
            this.Load += new System.EventHandler(this.SiparisGecmisiForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSiparisGecmisi)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvSiparisGecmisi;
        private System.Windows.Forms.Label lblToplamSiparis;
        private System.Windows.Forms.Label lblToplamAdet;
        private System.Windows.Forms.Button btnSiparisSil;
    }
}