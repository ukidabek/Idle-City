using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.General;

namespace Cooki.Flow
{
    public class Flow : MonoBehaviour
    {
        [SerializeReference, ReferenceList] private IFlowAction[] m_onEnterActions = null;
        [SerializeReference, ReferenceList] private ITickableFlowAction[] m_onTickActions = null;
        [SerializeReference, ReferenceList] private IFlowAction[] m_onExitActions = null;

        public void Initialize(FlowManager flowManager)
        {
            var initializableActions = new List<IInitializableFlowAction>(30)
                .Concat(m_onEnterActions.OfType<IInitializableFlowAction>())
                .Concat(m_onTickActions.OfType<IInitializableFlowAction>())
                .Concat(m_onExitActions.OfType<IInitializableFlowAction>());
            
            foreach (var initializableAction in initializableActions)
                initializableAction.Initialize(flowManager);
        }

        public IEnumerator OnEnter(FlowManager manager)
        {
            foreach (var action in m_onEnterActions)
                yield return action.Perform(manager);
        }

        public void Tick(FlowManager flowManager, float deltaTime, float timeScale)
        {
            var lenght = m_onTickActions.Length;
            for (var i = 0; i < lenght; i++)
                m_onTickActions[i].Tick(flowManager, deltaTime, timeScale);
        }

        public IEnumerator OnExit(FlowManager manager)
        {
            foreach (var action in m_onExitActions)
                yield return action.Perform(manager);
        }
    }
}