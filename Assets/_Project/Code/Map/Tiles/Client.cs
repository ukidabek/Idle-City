using Project.Resources;
using UnityEngine;

namespace Project.Map
{
    public abstract class Client : TileComponent, IClient
    {
        [field: SerializeField] public Resource Resource { get; private set; }
        public abstract ClientType Type { get; }

        [field: SerializeField, Tooltip("Amount generated/consumed per second.")] private float m_amount = .1f;
        public virtual float Amount => m_amount;

        private void OnEnable() => Resource.RegisterClient(this);

        private void OnDisable() => Resource.UnregisterClient(this);
    }
}