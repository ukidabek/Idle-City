using UnityEngine;

namespace Project.Map
{
    [RequireComponent(typeof(Tile))]
    public abstract class DataTile<DataType> : MonoBehaviour, ITielComponent where DataType : ScriptableObject
    {
        [field: SerializeField] public Tile Tile { get; private set; }
        [SerializeField] protected DataType m_data = null;

        protected virtual void Reset() => Tile = GetComponent<Tile>();
    }
}