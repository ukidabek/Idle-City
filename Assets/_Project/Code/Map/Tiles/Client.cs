using System.Collections.Generic;
using Project.Resources;
using UnityEngine;

namespace Project.Map
{
    public abstract class Client : TileComponent, IResourceClient, IModiferHandler
    {
        [field: SerializeField] public Resource Resource { get; private set; }
        
        public abstract ClientType Type { get; }

        [field: SerializeField, Tooltip("Amount generated/consumed per second.")] 
        public float BaseAmount { get; private set; } = .1f;

        [field: SerializeField] public virtual float Amount { get; private set; }
        
        private ModiferHandler m_handler;

        private void Awake()
        {
            Amount = BaseAmount;
            m_handler = new ModiferHandler(() => BaseAmount, OnValueRecalculated);
        }

        private void OnValueRecalculated(float value) => Amount = value;

        private void OnEnable() => Resource.RegisterClient(this);

        private void OnDisable() => Resource.UnregisterClient(this);

        private void OnDestroy()
        {
            m_handler.Dispose();
            m_handler = null;
        }

        public void Apply(IEnumerable<AmountModifier> modifiers) => m_handler.Apply(modifiers);

        public void Remove(IEnumerable<AmountModifier> modifiers) => m_handler.Remove(modifiers);
        
        public void Apply(AmountModifier modifier) => m_handler.Apply(modifier);

        public void Remove(AmountModifier modifier) => m_handler.Remove(modifier);
    }
}