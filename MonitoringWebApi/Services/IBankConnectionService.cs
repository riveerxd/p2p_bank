namespace MonitoringWebApi.Services;

public interface IBankConnectionService
{
    Task ShutdownServerAsync();
    Task<StreamReader> GetLogStreamReader();
}