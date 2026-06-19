using System;
using System.Collections.Generic;
using Code.Ticker;
using Project.Resources;
using UnityEngine;
using Utilities.General;

namespace Project.Map
{
    public class ProducerTickable : Tickable<Producer>
    {
        [SerializeField] protected ResourceCollection m_resolution;

        private readonly List<IDisposable> m_bulkEditScope = new List<IDisposable>(10);

        [SerializeField, ReadOnly] private float m_counter = 0f;
        
        protected override void Process(HashSet<Producer> objectsToTick, int tickRate, in TimeInfo timeInfo)
        {
            if (m_counter < 1f)
            {
                m_counter += timeInfo.DeltaUpdate;
                return;
            }

            m_counter = 0f;
            foreach (var resource in m_resolution)
                m_bulkEditScope.Add(resource.BulkEdit());

            foreach (var client in objectsToTick)
            {
                if (!client.Consume()) continue;
                client.Produce();
            }

            foreach (var client in m_bulkEditScope)
                client.Dispose();

            m_bulkEditScope.Clear();
        }
    }
}