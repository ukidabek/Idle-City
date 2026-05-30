using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;

namespace Cooki.Flow.General
{
    [Serializable, Preserve]
    public class SwitchFlowAction : IFlowAction
    {
        [SerializeField] private Flow m_flow = default;
        public IEnumerator Perform(FlowManager manager)
        {
            manager.EnterState(m_flow);
            yield break;
        }
    }
}