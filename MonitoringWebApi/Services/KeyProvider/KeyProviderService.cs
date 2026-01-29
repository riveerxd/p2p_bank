namespace MonitoringWebApi.Services.KeyProvider;

public class KeyProviderService : IKeyProviderService
{
    private readonly string _key;

    public KeyProviderService(string key)
    {
        _key = key;
    }

    public string GetKey()
    {
        return _key;
    }
}