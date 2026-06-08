using System.Collections.Generic;
using Project.Resources;
using UnityEngine;
using Utilities.General;

namespace Project.Map
{
    public abstract class Client : TileComponent, IResourceClient, IModiferHandler, IDataTileComponent<ClientData>
    {
        [field: SerializeField] public ClientData Data { get; private set; }
        public abstract ClientType Type { get; }

        [field: SerializeField, ReadOnly] public virtual float Amount { get; private set; }
        
        private ModiferHandler m_handler;

        private void Awake()
        {
            Amount = Data.BaseAmount;
            m_handler = new ModiferHandler(() => Data.BaseAmount, OnValueRecalculated);
        }

        private void OnValueRecalculated(float value) => Amount = value;

        private void OnEnable() => Data.Resource.RegisterClient(this);

        private void OnDisable() => Data.Resource.UnregisterClient(this);

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