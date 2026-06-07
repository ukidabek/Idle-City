using UnityEngine;

namespace Project.Resources
{
    public class PercentAmountModifier : AmountModifier
    {
        private readonly float m_percent = 0;

        public PercentAmountModifier(float percent)
        {
            m_percent = Mathf.Clamp(percent, 0f, float.MaxValue);
        }

        public override ModifierType Type => ModifierType.Percent;
        public override float Apply(float amount) => amount * m_percent;
    }
}