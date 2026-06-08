using System.Collections.Generic;
using UnityEngine;

namespace Project.Map
{
    public class Structure : TileComponent, IDataTileComponent<StructureData>
    {
        [SerializeField] private StructureData m_data;
        public StructureData Data  => m_data;
        public Sprite Image => Data.Image;
        public IReadOnlyList<TileID> TileRequirements => Data.TileRequirements;
        public IReadOnlyList<Cost> Costs => Data.Costs;
        public bool CanAfford => Data.CanAfford;
        public float CostReturnMultiplayer => Data.CostReturnMultiplayer;
        
        public void ConsumeResources()
        {
            foreach (var cost in Costs) 
                cost.Resource.Value -= cost.Amount;
        }

    }
}