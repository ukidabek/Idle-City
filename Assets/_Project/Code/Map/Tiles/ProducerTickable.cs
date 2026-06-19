using System;
using System.Collections.Generic;
using Code.Ticker;
using Project.Resources;
using UnityEngine;

namespace Project.Map
{
    public class ProducerTickable : Tickable<Producer>
    {
        [SerializeField] protected ResourceCollection m_resolution;

        private readonly List<IDisposable> m_bulkEditScope = new List<IDisposable>(10);

        protected override void Process(HashSet<Producer> objectsToTick, int tickRate, in TimeInfo timeInfo)
        {
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