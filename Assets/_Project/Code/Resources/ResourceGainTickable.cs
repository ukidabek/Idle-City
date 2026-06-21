using System;
using System.Collections.Generic;
using Code.Ticker;
using UnityEngine;
using Utilities.General;

namespace Project.Resources
{
    public class ResourceGainTickable : Tickable
    {
        [SerializeField] private ResourceCollection m_resources;
        [SerializeField, ReadOnly] private float m_counter = 0f;

        private readonly List<IDisposable> m_bulkEditScope = new List<IDisposable>(10);

        public override void Tick(int tickRate, in TimeInfo timeInfo)
        {
            if (!IsReadyToTick()) return;

            if (m_counter < 1f)
            {
                m_counter += timeInfo.DeltaUpdate;
                return;
            }

            m_counter = 0f;

            foreach (var resource in m_resources)
                m_bulkEditScope.Add(resource.BulkEdit());

            foreach (var resource in m_resources)
                resource.Produce(resource.Gain);

            foreach (var scope in m_bulkEditScope)
                scope.Dispose();

            m_bulkEditScope.Clear();
        }
    }
}
