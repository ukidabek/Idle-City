using System;
using System.Collections;
using UnityEngine;

namespace Cooki.Flow
{
    public class FlowManager : MonoBehaviour
    {
        [SerializeField] private Flow m_initialFlow;
        [SerializeField] private Flow m_currentFlow;

        private Coroutine m_coroutine;

        public void Awake()
        {
            var allFlows = GetComponentsInChildren<Flow>();
            foreach (var flow in allFlows)
                flow.Initialize(this);

            EnterState(m_initialFlow);
        }

        public void EnterState(Flow flow)
        {
            if (m_coroutine != null)
                StopCoroutine(m_coroutine);

            m_coroutine = StartCoroutine(EnterStateCoroutine(flow));
        }

        private IEnumerator EnterStateCoroutine(Flow flow)
        {
            var lastFlow = m_currentFlow;
            m_currentFlow = null;

            if (lastFlow != null)
                yield return lastFlow.OnExit(this);

            yield return flow.OnEnter(this);

            m_currentFlow = flow;
        }

        private void Update()
        {
            if (m_currentFlow == null) return;
            m_currentFlow.Tick(this, Time.deltaTime, Time.deltaTime);
        }
    }
}