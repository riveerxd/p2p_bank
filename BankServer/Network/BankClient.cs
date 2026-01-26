using System.Net.Sockets;
using System.Text;
using P2PBank.Logging;

namespace P2PBank.Network;

// TCP client for connecting to other banks
public class BankClient
{
    // sends command to another bank, returns response or ER on fail
    public static string SendCommand(string ip, int port, string cmd, int timeout)
    {
        TcpClient? client = null;
        NetworkStream? stream = null;
        StreamReader? reader = null;
        StreamWriter? writer = null;

        try
        {
            client = new TcpClient();
            client.SendTimeout = timeout;
            client.ReceiveTimeout = timeout;

            var connectTask = client.ConnectAsync(ip, port);
            if(!connectTask.Wait(timeout))
            {
                return "ER Connection timeout to " + ip;
            }

            stream = client.GetStream();
            reader = new StreamReader(stream, Encoding.UTF8);
            writer = new StreamWriter(stream, Encoding.UTF8);
            writer.AutoFlush = true;

            writer.WriteLine(cmd);

            string? resp = reader.ReadLine();
            if(resp == null)
                return "ER No response from " + ip;

            //Logger.Info("Proxy to " + ip + ": " + cmd + " -> " + resp);
            return resp;
        }
        catch(SocketException)
        {
            return "ER Cannot connect to " + ip;
        }
        catch(Exception ex)
        {
            //Logger.Error("BankClient error: " + ex.Message);
            return "ER Communication error";
        }
        finally
        {
            // close everything
            reader?.Close();
            writer?.Close();
            stream?.Close();
            client?.Close();
        }
    }
}
