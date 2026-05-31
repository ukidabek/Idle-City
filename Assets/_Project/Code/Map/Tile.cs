using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Map
{
    [SelectionBase]
    public class Tile : MonoBehaviour
    {
        [FormerlySerializedAs("m_key")] 
        [SerializeField] private TileID m_id;
        public TileID ID => m_id;
        
        [SerializeField] private TileCategory m_tileCategory;
        public TileCategory TileCategory => m_tileCategory;
    }
}
