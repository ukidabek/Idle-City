namespace Project.Resources
{
    public interface IClient
    {
        ClientType Type { get; }
        float Amount { get; }
    }
}