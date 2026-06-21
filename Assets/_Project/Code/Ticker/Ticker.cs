using System.Collections.Generic;
using System.Linq;
using cookie.Logging;
using Unity.Profiling;
using UnityEngine;

namespace Code.Ticker
{
    public class Ticker : MonoBehaviour, ILogEnabled
    {
        public Color Color { get; } = new Color(1f, 0.84f, 0.2f, 1f); // #FFD633
        [field: SerializeField] public LogMode Mode { get; private set; } = LogMode.All;
        private Tickable[] m_tickables;
        [SerializeField, Min(1)] protected int m_tickRate = 20;

        private float m_interval = 0f;

        [SerializeField] private TimeInfo m_timeInfo = new TimeInfo();

        private static readonly ProfilerMarker UpdateMarker = new ProfilerMarker("Ticker.Update");
        private Dictionary<Tickable, ProfilerMarker> m_tickMarkers;

        private void Awake()
        {
            CalculateInterval();
            m_timeInfo.NextUpdate = Time.time - m_interval;
            m_tickables = GetComponents<Tickable>();
            m_tickMarkers = m_tickables.ToDictionary(
                tickable => tickable,
                tickable => new ProfilerMarker($"Ticker.Tick.{tickable.GetType().Name}"));
        }

        private void Update()
        {
            var currentTime = Time.time;
            m_timeInfo.DeltaTime = Time.deltaTime;
            m_timeInfo.TimeScale = Time.timeScale;

            if (currentTime < m_timeInfo.NextUpdate) return;

            m_timeInfo.DeltaUpdate = currentTime - m_timeInfo.LastUpdate;
            m_timeInfo.LastUpdate = currentTime;
            m_timeInfo.NextUpdate = currentTime + m_interval;

            this.Log("Updating...", LogType.Log, gameObject);
            using (UpdateMarker.Auto())
            {
                foreach (var resourceHandler in m_tickables)
                {
                    if (!resourceHandler.enabled) continue;
                    if (!resourceHandler.IsReadyToTick()) continue;
                    using (m_tickMarkers[resourceHandler].Auto())
                        resourceHandler.Tick(m_tickRate, m_timeInfo);
                }
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying) return;
            CalculateInterval();
        }

        private void CalculateInterval() => m_interval = 1f / m_tickRate;
    }
}
