namespace Jtar.Compressor;

/// <summary>
/// Interface for compression and decompression functionality.
/// </summary>
public interface ICompressor : ICloneable
{
    /// <summary>
    /// Compresses the given data.
    /// </summary>
    /// <param name="data">Data to be compressed</param>
    /// <returns>Compressed data</returns>
    byte[] Compress(byte[] data);

    /// <summary>
    /// Decompresses the given data.
    /// </summary>
    /// <param name="data">Data to be decompressed</param>
    /// <returns>Decompressed data</returns>
    byte[] Decompress(byte[] data);
}