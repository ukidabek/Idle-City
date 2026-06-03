using System.Collections.Generic;

namespace Project.Map
{
    public class Ground : DataTileComponent<GroundData>
    {
        public IReadOnlyList<TileID> AvailableDeposits => m_data.AvailableDeposits;
    }
}