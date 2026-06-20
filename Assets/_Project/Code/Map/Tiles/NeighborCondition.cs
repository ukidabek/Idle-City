using System;
using Project.Resources;
using UnityEngine;

namespace Project.Map
{
    public readonly struct ProducerModifier
    {
        public readonly AmountModifier Modifier;
        public readonly ModifierTarget Target;

        public ProducerModifier(AmountModifier modifier, ModifierTarget target)
        {
            Modifier = modifier;
            Target = target;
        }
    }

    [Serializable]
    public class NeighborCondition
    {
        [SerializeField] private TileID m_neighborId;
        [SerializeField] private ModifierType m_type;
        [SerializeField] private float m_value;
        [SerializeField] private ModifierTarget m_target;

        public ProducerModifier? Build(Tile tile)
        {
            if (tile.ID != m_neighborId) return null;

            var modifier = m_type switch
            {
                ModifierType.Value   => (AmountModifier)new ValueAmountModifier(m_value),
                ModifierType.Percent => new PercentAmountModifier(m_value),
                _ => null
            };

            return modifier == null ? null : new ProducerModifier(modifier, m_target);
        }
    }
}
