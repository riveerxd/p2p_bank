namespace MonitoringWebApi.Services;

public interface IConnectionService
{
    Task ShutdownServerAsync();
    Task<StreamReader> GetLogStreamReader();
}