namespace OddsCollector.Functions.Tests.Infrastructure.Http;

internal sealed class ThrowingStream(Exception exception) : Stream
{
    public override bool CanRead => false;

    public override bool CanSeek => false;

    public override bool CanWrite => true;

    public override long Length => throw exception;

    public override long Position
    {
        get => throw exception;
        set => throw exception;
    }

    public override void Flush()
    {
        throw exception;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw exception;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw exception;
    }

    public override void SetLength(long value)
    {
        throw exception;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw exception;
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count,
        System.Threading.CancellationToken cancellationToken)
    {
        throw exception;
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer,
        System.Threading.CancellationToken cancellationToken = default)
    {
        throw exception;
    }
}
