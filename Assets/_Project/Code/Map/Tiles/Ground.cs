using System.Collections.Generic;

namespace Project.Map
{
    public class Ground : DataTile<GroundData>, IGround
    {
        public IReadOnlyList<TileID> AvailableDeposits => m_data.AvailableDeposits;
    }
}