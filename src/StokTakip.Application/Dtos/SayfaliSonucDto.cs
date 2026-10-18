namespace StokTakip.Application.Dtos;

public sealed record SayfaliSonucDto<T>(IReadOnlyList<T> Ogeler, int ToplamKayit, int Sayfa, int SayfaBoyutu)
{
    public int ToplamSayfa => (int)((ToplamKayit + (long)SayfaBoyutu - 1) / SayfaBoyutu);
}
