using Project.Resources;
using UnityEngine;

namespace Project.Map
{
    [CreateAssetMenu(menuName = "Map/Tiles/ClientData", fileName = "ClientData")]
    public class ClientData : TileData
    {
        [field: SerializeField] public Resource Resource { get; private set; }
        [field: SerializeField, Tooltip("Amount generated/consumed per second.")] 
        public float BaseAmount { get; private set; } = .1f;
    }
}