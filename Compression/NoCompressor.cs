namespace Jtar.Compressor;

/// <summary>
/// A compressor that performs no compression or decompression.
/// Serves for outputting uncompressed data.
/// </summary>
public class NoCompressor : ICompressor
{
    /// <summary>
    /// Returns a new NoCompressor instance.
    /// </summary>
    /// <returns>A new instance of NoCompressor</returns>
    public object Clone()
    {
        return new NoCompressor();
    }

    /// <summary>
    /// Returns the input data unchanged.
    /// </summary>
    /// <param name="data">Input data</param>
    /// <returns>Unchanged input data</returns>
    public byte[] Compress(byte[] data)
    {
        return data;
    }

    /// <summary>
    /// Returns the input data unchanged.
    /// </summary>
    /// <param name="data">Input data</param>
    /// <returns>Unchanged input data</returns>
    public byte[] Decompress(byte[] data)
    {
        return data;
    }
}