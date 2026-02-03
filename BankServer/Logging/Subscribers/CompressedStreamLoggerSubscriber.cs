using System.Text;
using System.Security.Cryptography;
using Compression;

namespace P2PBank.Logging.Subscribers;

public class CompressedStreamLoggerSubscriber : ILoggerSubscriber
{
    private StreamWriter _writer;
    private bool _disposed = false;

    private ICompressor _compressor;
    private byte[] _key;

    public CompressedStreamLoggerSubscriber(StreamWriter stream, ICompressor compressor, string privateKey)
    {
        _writer = stream;
        _writer.AutoFlush = true;
        _compressor = compressor;
        using (var sha256 = SHA256.Create())
        {
            _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(privateKey));
        }
    }

    public bool IsDisposed => _disposed || _writer == null || _writer.BaseStream == null || !_writer.BaseStream.CanWrite;

    public void Log(string message)
    {
        try
        {
            var compressedMessage = _compressor.Compress(Encoding.UTF8.GetBytes(message));
            
            // Encrypt
            byte[] encryptedMessage;
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.GenerateIV();
                var iv = aes.IV;

                using (var encryptor = aes.CreateEncryptor(aes.Key, iv))
                using (var msEncrypt = new MemoryStream())
                {
                    msEncrypt.Write(iv, 0, iv.Length);
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(compressedMessage, 0, compressedMessage.Length);
                    }
                    encryptedMessage = msEncrypt.ToArray();
                }
            }

            var base64Message = Convert.ToBase64String(encryptedMessage);
            _writer.WriteLine(base64Message);
        }
        catch (IOException)
        {
            _disposed = true;
        }
        catch (ObjectDisposedException)
        {
            _disposed = true;
        }
    }
}