using UnityEngine;

namespace Project.Map
{
    [RequireComponent(typeof(Tile))]
    public abstract class DataTileComponent<DataType> : TileComponent where DataType : ScriptableObject
    {
        [SerializeField] protected DataType m_data = null;
    }
}