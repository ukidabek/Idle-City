using System.Collections.Generic;
using UnityEngine;

namespace Project.Map
{
    [SelectionBase]
    public class Tile : MonoBehaviour
    {
        [SerializeField] private TileID m_key;
        public TileID Key => m_key;
        
        [SerializeField] private TileCategory m_tileCategory;
        public TileCategory TileCategory => m_tileCategory;
    }
}
