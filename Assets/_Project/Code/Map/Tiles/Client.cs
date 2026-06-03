using Project.Resources;
using UnityEngine;

namespace Project.Map
{
    public class Client : TileComponent, IClient
    {
        [field: SerializeField] public Resource Resource { get; private set; }
        [field: SerializeField] public ClientType Type { get; private set; }
        [field: SerializeField, Tooltip("Amount generated/consumed per second;")] public float Amount { get; private set; }

        private void OnEnable() => Resource.RegisterClient(this);

        private void OnDisable() => Resource.UnregisterClient(this);
    }
}