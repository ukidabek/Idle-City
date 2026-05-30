using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;
using Utilities.General;

namespace Cooki.Flow.General
{
    [Serializable, Preserve]
    public class ObjectStatusFlowAction : IFlowAction
    {
        [SerializeField] private bool m_oneGo = false;
        [SerializeField] private bool m_statusToSet = false;
        [SerializeReference, ReferenceList] private IStatusHandler[] m_statusHandlers = null;
        
        public IEnumerator Perform(FlowManager manager)
        {
            var lenght = m_statusHandlers.Length;
            if (m_oneGo)
            {
                for (var i = 0; i < lenght; i++) 
                    m_statusHandlers[i].SetStatus(m_statusToSet);
                yield break;
            }

            for (var i = 0; i < lenght; i++)
            {
                m_statusHandlers[i].SetStatus(m_statusToSet);
                yield return null;
            }
        }
    }
}