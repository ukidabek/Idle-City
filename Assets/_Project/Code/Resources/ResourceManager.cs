using cookie.Logging;
using UnityEngine;

namespace Project.Resources
{
    public class ResourceManager : MonoBehaviour, ILogEnabled
    {
        public Color Color { get; } = new Color(1f, 0.84f, 0.2f, 1f); // #FFD633

        [field: SerializeField] public LogMode Mode { get; private set; } = LogMode.All;
        [SerializeField] private Resource[] m_resources;
        [SerializeField, Min(0)] protected int m_tickRate = 20;

        private float m_interval = 0f;
        private float m_nextUpdate = 0f;

        private void Awake()
        {
            CalculateInterval();
            m_nextUpdate = Time.time - m_interval;
        }

        private void Update()
        {
            var currentTime = Time.time;
            if (currentTime < m_nextUpdate) return;
            this.Log("Updating resources...", LogType.Log, gameObject);
            foreach (var resourceHandler in m_resources)
            {
                var oldValue = resourceHandler.Value;
                resourceHandler.Tick(m_tickRate);
                var newValue = resourceHandler.Value;
                this.Log($"[<b>{resourceHandler.name}</b>] Changed form {oldValue:F2} to: {newValue:F2}", LogType.Log, resourceHandler);
            }
            m_interval = currentTime + m_nextUpdate;
        }

        private void OnValidate()
        {
            if (!Application.isPlaying) return;
            CalculateInterval();
        }

        private void CalculateInterval() => m_interval = 1000 / m_tickRate;
    }
}