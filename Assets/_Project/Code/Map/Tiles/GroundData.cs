using System.Collections.Generic;
using UnityEngine;

namespace Project.Map
{
    [CreateAssetMenu(menuName = "Map/Tiles/GroundData", fileName = "GroundData")]
    public class GroundData : ScriptableObject
    {
        [SerializeField] private TileID[] m_availableDeposits;
        public IReadOnlyList<TileID> AvailableDeposits => m_availableDeposits;
        
    }
}