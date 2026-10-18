using System;

namespace StokTakip.Presenters
{
    public interface IStokDegisiklikBildirimi
    {
        event EventHandler UrunlerDegisti;

        event EventHandler SiparislerDegisti;

        void Bildir();

        void SiparislerDegistiBildir(object kaynak);
    }

    public sealed class StokDegisiklikBildirimi : IStokDegisiklikBildirimi
    {
        public event EventHandler UrunlerDegisti;

        public event EventHandler SiparislerDegisti;

        public void Bildir() => UrunlerDegisti?.Invoke(this, EventArgs.Empty);

        public void SiparislerDegistiBildir(object kaynak) => SiparislerDegisti?.Invoke(kaynak ?? this, EventArgs.Empty);
    }
}
