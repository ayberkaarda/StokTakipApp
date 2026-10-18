using System;

namespace StokTakip.Presenters
{
    public interface IStokDegisiklikBildirimi
    {
        event EventHandler UrunlerDegisti;

        void Bildir();
    }

    public sealed class StokDegisiklikBildirimi : IStokDegisiklikBildirimi
    {
        public event EventHandler UrunlerDegisti;

        public void Bildir() => UrunlerDegisti?.Invoke(this, EventArgs.Empty);
    }
}
