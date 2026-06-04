using System;
using UnityEngine;

namespace Project.Map
{
    [Serializable]
    public class NeighborCondition
    {
        [SerializeField] private TileID m_neighborId;
        [SerializeField] private ModifierType m_type;
        [SerializeField] private float m_value;

        public ClientModifier Build(Tile tile)
        {
            if(tile.ID != m_neighborId) return null;

            return m_type switch
            {
                ModifierType.Value => new ValueClientModifier(m_value),
                ModifierType.Percent => new PercentClientModifier(m_value),
                _ => null
            };
        }
    }
}