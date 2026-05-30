using System.Collections;

namespace Cooki.Flow
{
    public interface IFlowAction
    {
        IEnumerator Perform(FlowManager manager);
    }
}