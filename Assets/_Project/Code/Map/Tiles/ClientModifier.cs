using UnityEngine;

namespace Project.Map
{
    public abstract class ClientModifier
    {
        public abstract ModifierType Type { get; }
        public abstract float Apply(float amount);
    }

    public class PercentClientModifier : ClientModifier
    {
        private readonly float m_percent = 0;

        public PercentClientModifier(float percent)
        {
            m_percent = Mathf.Clamp(percent, 0f, float.MaxValue);
        }

        public override ModifierType Type => ModifierType.Percent;
        public override float Apply(float amount) => amount * m_percent;
    }

    public class ValueClientModifier : ClientModifier
    {
        private readonly float m_value = 0;

        public ValueClientModifier(float value)
        {
            m_value = value;
        }

        public override ModifierType Type => ModifierType.Value;
        public override float Apply(float amount) => amount + m_value;
    }
}