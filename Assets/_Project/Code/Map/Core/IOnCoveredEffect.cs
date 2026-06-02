namespace Project.Map
{
    public interface IOnCoveredEffect
    {
        void Apply();
        void Undo();
    }
}