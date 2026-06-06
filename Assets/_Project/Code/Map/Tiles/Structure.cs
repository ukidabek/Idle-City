using System.Collections.Generic;
using UnityEngine;

namespace Project.Map
{
    public class Structure : DataTileComponent<StructureData>
    {
        public Sprite Image => m_data.Image;
        public IReadOnlyList<TileID> TileRequirements => m_data.TileRequirements;
        
        public IReadOnlyList<Cost> Costs => m_data.Costs;
    }
}