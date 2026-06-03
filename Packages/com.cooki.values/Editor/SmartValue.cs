using System;
using UnityEngine;

namespace Values
{
    public enum SmartValueMode { Local, Remote }

    [Serializable]
    public class SmartValue<T>
    {
        [SerializeField] private SmartValueMode m_mode = SmartValueMode.Local;
        [SerializeField] private T m_localValue;
        [SerializeField] private BaseValue<T> m_remoteValue;

        public SmartValueMode Mode
        {
            get => m_mode;
            set => m_mode = value;
        }

        public T Value
        {
            get => m_mode == SmartValueMode.Local
                ? m_localValue
                : m_remoteValue != null ? m_remoteValue.Value : default;
            set
            {
                if (m_mode == SmartValueMode.Local)
                    m_localValue = value;
                else if (m_remoteValue != null)
                    m_remoteValue.Value = value;
            }
        }

        public SmartValue() { }

        public SmartValue(T localValue)
        {
            m_mode = SmartValueMode.Local;
            m_localValue = localValue;
        }

        public SmartValue(BaseValue<T> remoteValue)
        {
            m_mode = SmartValueMode.Remote;
            m_remoteValue = remoteValue;
        }
    }
}
