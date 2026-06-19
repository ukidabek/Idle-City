using cookie.Logging;
using UnityEngine;

namespace Project.Map
{
    public abstract class TileComponent : MonoBehaviour, ITielComponent, ILogEnabled
    {
        [field: SerializeField] public Tile Tile { get; protected set; }
        [field: SerializeField] public Color Color { get; private set; } = new Color(0.8f, 0.8f, 0.8f, 1f);
        [field: SerializeField] public LogMode Mode { get; private set; } = LogMode.All;

        protected virtual void Reset() => Tile = GetComponent<Tile>();
    }
}