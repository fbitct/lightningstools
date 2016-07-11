namespace Common.SimSupport
{
    public interface ISimOutput
    {
        string FriendlyName { get; }
        string Id { get; }
        bool HasListeners { get; }
    }
}