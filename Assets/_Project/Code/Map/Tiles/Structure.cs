using System.Collections.Generic;

namespace Project.Map
{
    public class Structure : DataTileComponent<StructureData>
    {
        public IReadOnlyList<TileID> TileRequirements => m_data.TileRequirements;
    }
}