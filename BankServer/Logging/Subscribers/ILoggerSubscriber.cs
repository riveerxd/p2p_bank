namespace P2PBank.Logging.Subscribers;

public interface ILoggerSubscriber
{
    void Log(string message);

    // Not ideal
    bool IsDisposed { get; }
}