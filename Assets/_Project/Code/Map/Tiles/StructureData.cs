using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.Map
{
    [CreateAssetMenu(menuName = "Map/Tiles/StructureData", fileName = "StructureData")]
    public class StructureData : ScriptableObject
    {
        [field: SerializeField] public Sprite Image { get; private set; } = null;
        [field: SerializeField] public TileID[] TileRequirements  { get; private set; } = null;
        
        [SerializeField] private Cost[] m_costs = null;
        public IReadOnlyList<Cost> Costs => m_costs;
        
        public bool CanAfford => m_costs.All(cost => cost.CanAfford);
    }
}