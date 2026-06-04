namespace Project.Resources
{
    public interface IResourceClient
    {
        ClientType Type { get; }
        float Amount { get; }
    }
}