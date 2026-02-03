using MonitoringWebApi.Stream;

namespace MonitoringWebApi.Services.BankConnection;

public interface IBankConnectionService
{
    Task ShutdownServerAsync();
    Task<TcpClientStream> GetLogStreamReader();

    Task<DateTime> GetStartTime();
}