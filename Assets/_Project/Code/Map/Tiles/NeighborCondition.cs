using System;
using UnityEngine;

namespace Project.Map
{
    [Serializable]
    public class NeighborCondition
    {
        [SerializeField] private TileID m_neighborId;
        [SerializeField] public  ProducerModifierInfo m_info;
        
        public ProducerModifier? Build(Tile tile) => tile.ID != m_neighborId ? null : m_info.Build();
    }
}
