namespace Cooki.Flow
{
    public interface ITickableFlowAction
    {
        void Tick(FlowManager manager, float deltaTime, float timeScale);
    }
}