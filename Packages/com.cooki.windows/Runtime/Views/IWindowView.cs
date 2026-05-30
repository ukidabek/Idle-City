namespace Windows.View
{
    public interface IWindowView<in T>
    {
        void Initialize(T data);
        void Refresh() {}
        void Clear();
    }
}