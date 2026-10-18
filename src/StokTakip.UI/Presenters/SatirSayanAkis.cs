using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace StokTakip.Presenters
{
    /// <summary>Yazılanları hedef akışa geçirir ve yazılan satır sonlarını sayar (dışa aktarma ilerlemesi için).</summary>
    internal sealed class SatirSayanAkis : Stream
    {
        private readonly Stream _hedef;
        private readonly Action<int> _satirSayisiDegisti;

        public SatirSayanAkis(Stream hedef, Action<int> satirSayisiDegisti)
        {
            _hedef = hedef ?? throw new ArgumentNullException(nameof(hedef));
            _satirSayisiDegisti = satirSayisiDegisti;
        }

        public int SatirSayisi { get; private set; }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => _hedef.CanWrite;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count) => Write(buffer.AsSpan(offset, count));

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            _hedef.Write(buffer);
            Say(buffer);
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await _hedef.WriteAsync(buffer, cancellationToken);
            Say(buffer.Span);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

        public override void Flush() => _hedef.Flush();

        public override Task FlushAsync(CancellationToken cancellationToken) => _hedef.FlushAsync(cancellationToken);

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        private void Say(ReadOnlySpan<byte> buffer)
        {
            int yeni = 0;
            foreach (byte b in buffer)
            {
                if (b == (byte)'\n')
                {
                    yeni++;
                }
            }

            if (yeni > 0)
            {
                SatirSayisi += yeni;
                _satirSayisiDegisti?.Invoke(SatirSayisi);
            }
        }
    }
}
