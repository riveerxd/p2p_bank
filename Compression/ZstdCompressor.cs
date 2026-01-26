namespace Jtar.Compressor;

/// <summary>
/// Zstandard (Zstd) compressor implementation.
/// </summary>
public class ZstdCompressor : ICompressor
{
    private readonly ZstdSharp.Compressor _compressor;
    private readonly ZstdSharp.Decompressor _decompressor;

    /// <summary>
    /// Initializes a new instance of the ZstdCompressor class.
    /// </summary>
    public ZstdCompressor()
    {
        _compressor = new ZstdSharp.Compressor(ZstdSharp.Compressor.MaxCompressionLevel);
        _decompressor = new ZstdSharp.Decompressor();
    }

    /// <summary>
    /// Destroys the ZstdCompressor instance and releases resources.
    /// </summary>
    ~ZstdCompressor()
    {
        _compressor.Dispose();
        _decompressor.Dispose();
    }

    /// <summary>
    /// Clones the current ZstdCompressor instance.
    /// </summary>
    /// <returns>A new instance of ZstdCompressor</returns>
    public object Clone()
    {
        return new ZstdCompressor();
    }

    /// <summary>
    /// Compresses the given data using Zstd compression.
    /// </summary>
    /// <param name="data">Input data</param>
    /// <returns>Compressed data</returns>
    public byte[] Compress(byte[] data)
    {
        return _compressor.Wrap(data).ToArray();
    }

    /// <summary>
    /// Decompresses the given data using Zstd decompression.
    /// </summary>
    /// <param name="data">Input data</param>
    /// <returns>Decompressed data</returns>
    public byte[] Decompress(byte[] data)
    {
        return _decompressor.Unwrap(data).ToArray();
    }
}