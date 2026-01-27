using System.Net.Sockets;
using System.Text;
using P2PBank.Logging;

namespace P2PBank.Network;

// TCP client for connecting to other banks
public class BankClient
{
    // tries to find which port the bank is running on
    public static int FindBankPort(string ip, int timeout = 1000)
    {
        for(int port = 65525; port <= 65535; port++)
        {
            try
            {
                using var client = new TcpClient();
                client.SendTimeout = timeout;
                client.ReceiveTimeout = timeout;

                var connectTask = client.ConnectAsync(ip, port);
                if(!connectTask.Wait(timeout))
                    continue;

                var stream = client.GetStream();
                var reader = new StreamReader(stream, Encoding.UTF8);
                var writer = new StreamWriter(stream, Encoding.UTF8);
                writer.AutoFlush = true;

                // ask the server for its bank code so we know its the right one
                writer.WriteLine("BC");
                string? resp = reader.ReadLine();

                if(resp != null)
                {
                    // why is this here? - keep it, crashes without it
                    resp = resp.TrimStart('\uFEFF');

                    if(resp.StartsWith("BC "))
                    {
                        string bankIp = resp.Substring(3).Trim();
                        if(bankIp == ip)
                            return port;
                    }
                }
            }
            catch(Exception)
            {
                // not a bank, whatever
            }
        }

        return -1;
    }

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
        catch(Exception)
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
