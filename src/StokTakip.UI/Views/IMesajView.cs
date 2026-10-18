namespace StokTakip.Views
{
    public interface IMesajView
    {
        bool Mesgul { set; }

        void BilgiGoster(string mesaj);

        void UyariGoster(string mesaj);

        void HataGoster(string mesaj);

        bool OnayAl(string mesaj, string baslik);
    }
}
