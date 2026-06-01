using System.Collections.Generic;

namespace Project.Map
{
    public class Structure : DataTile<StructureData>, IStructure
    {
        public IReadOnlyList<TileID> TileRequirements => m_data.TileRequirements;
    }
}