using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;

namespace Cooki.Flow.General
{
    [Serializable, Preserve]
    public class WaitToEndOfFrameFlowAction : IFlowAction
    {
        public IEnumerator Perform(FlowManager manager)
        {
            yield return new WaitForEndOfFrame();
        }
    }
}