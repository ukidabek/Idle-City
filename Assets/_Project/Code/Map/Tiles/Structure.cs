using System.Collections.Generic;
using UnityEngine;

namespace Project.Map
{
    public class Structure : DataTileComponent<StructureData>
    {
        public Sprite Image => m_data.Image;
        public IReadOnlyList<TileID> TileRequirements => m_data.TileRequirements;
        public IReadOnlyList<Cost> Costs => m_data.Costs;
        public bool CanAfford => m_data.CanAfford;
        public float CostReturnMultiplayer => m_data.CostReturnMultiplayer;
        
        public void ConsumeResources()
        {
            foreach (var cost in Costs) 
                cost.Resource.Value -= cost.Amount;
        }
    }
}