namespace StokTakip.Views
{
    /// <summary>Kategori açılır listelerindeki bir seçenek. KategoriId null ise "Kategorisiz" anlamına gelir.</summary>
    public sealed record KategoriSecenegi(string Ad, int? KategoriId, bool Tumu = false)
    {
        public bool AyniSecim(KategoriSecenegi diger) =>
            diger != null && diger.Tumu == Tumu && diger.KategoriId == KategoriId;

        public override string ToString() => Ad;
    }

    public enum StokFiltresi
    {
        Tumu = 0,
        DusukVeKritik = 1,
        Kritik = 2,
    }
}
