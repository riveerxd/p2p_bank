using MonitoringWebApi.Stream;

namespace MonitoringWebApi.Services;

public interface IBankConnectionService
{
    Task ShutdownServerAsync();
    Task<TcpClientStream> GetLogStreamReader();
}