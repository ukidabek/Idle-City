using System.Collections.Generic;
using UnityEngine;

namespace Project.Map
{
    public class Ground : TileComponent, IDataTileComponent<GroundData>
    {
        [SerializeField] private GroundData m_data = null;
        public GroundData Data => m_data;
        public IReadOnlyList<TileID> AvailableDeposits => Data.AvailableDeposits;
    }
}