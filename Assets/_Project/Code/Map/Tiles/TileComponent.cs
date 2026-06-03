using UnityEngine;

namespace Project.Map
{
    public abstract class TileComponent : MonoBehaviour, ITielComponent
    {
        [field: SerializeField] public Tile Tile { get; protected set; }
        protected virtual void Reset() => Tile = GetComponent<Tile>();
    }
}