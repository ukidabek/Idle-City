using System.Collections.Generic;
using UnityEngine;

namespace Project.Map
{
    [CreateAssetMenu(menuName = "Map/Tiles/GroundData", fileName = "GroundData")]
    public class GroundData : TileData
    {
        [SerializeField] private TileID[] m_availableDeposits;
        public IReadOnlyList<TileID> AvailableDeposits => m_availableDeposits;

        [field: SerializeField, Min(0)] public int Order { get; private set; } = 0;

    }
}