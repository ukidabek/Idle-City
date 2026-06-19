using System.Collections.Generic;
using cookie.Logging;
using UnityEngine;

namespace Code.Ticker
{
    public abstract class Tickable<T> : Tickable
    {
        private HashSet<T> m_objectsToTick = new HashSet<T>(100);
        private static readonly HashSet<T> m_objectsToAdd = new HashSet<T>(100);
        private static readonly HashSet<T> m_objectsToRemove = new HashSet<T>(100);

        public static void Subscribe(T objectToTick)
        {
            m_objectsToRemove.Remove(objectToTick);
            m_objectsToAdd.Add(objectToTick);
        }

        public static void Unsubscribe(T objectToTick)
        {
            m_objectsToRemove.Add(objectToTick);
            m_objectsToAdd.Remove(objectToTick);
        }

        public override void Tick(int tickRate, in TimeInfo timeInfo)
        {
            Flush();
            Process(m_objectsToTick, tickRate, timeInfo);
        }

        protected abstract void Process(HashSet<T> objectsToTick, int tickRate, in TimeInfo timeInfo);

        protected virtual void Flush()
        {
            foreach (var listener in m_objectsToRemove)
                m_objectsToTick.Remove(listener);

            foreach (var listener in m_objectsToAdd)
                m_objectsToTick.Add(listener);

            m_objectsToRemove.Clear();
            m_objectsToAdd.Clear();
        }
    }

    public abstract class Tickable : MonoBehaviour, ILogEnabled
    {
        [field: SerializeField] public Color Color { get; private set; } = new Color(0.2f, 0.8f, 1f, 1f);
        [field: SerializeField] public LogMode Mode { get; private set; } = LogMode.All;
        [SerializeField, Min(0)] private int m_pace = 0;
        private int m_tickCounter = 0;

        public bool IsReadyToTick()
        {
            if (m_tickCounter > 0)
            {
                m_tickCounter--; 
                return false;
            }
            
            m_tickCounter = m_pace;
            return true;
        }

        public abstract void Tick(int tickRate, in TimeInfo timeInfo);
    }
}