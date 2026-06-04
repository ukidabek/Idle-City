using System.Collections.Generic;
using System.Linq;
using Project.Resources;
using UnityEngine;

namespace Project.Map
{
    public abstract class Client : TileComponent, IResourceClient
    {
        [field: SerializeField] public Resource Resource { get; private set; }
        
        public abstract ClientType Type { get; }

        [field: SerializeField, Tooltip("Amount generated/consumed per second.")] 
        public float BaseAmount { get; private set; } = .1f;
        
        public virtual float Amount { get; private set; }
        
        private List<ClientModifier> m_modifiers = new List<ClientModifier>(10);

        private void Awake() => Amount =  BaseAmount;

        private void OnEnable() => Resource.RegisterClient(this);

        private void OnDisable() => Resource.UnregisterClient(this);

        public void Apply(IEnumerable<ClientModifier> modifiers)
        {
            m_modifiers.AddRange(modifiers);
            RecalculateValue();
        }

        public void Remove(IEnumerable<ClientModifier> modifiers)
        {
            foreach (var modifier in modifiers)
            {
                if (!m_modifiers.Contains(modifier)) continue;
                m_modifiers.Remove(modifier);
            }
            RecalculateValue();
        }
        
        public void Apply(ClientModifier modifier)
        {
            m_modifiers.Add(modifier);
            RecalculateValue();
        }

        public void Remove(ClientModifier modifier)
        {
            if (!m_modifiers.Contains(modifier)) return;
            m_modifiers.Remove(modifier);
            RecalculateValue();
        }

        private void RecalculateValue()
        {
            var value = BaseAmount;
            var modifiers = m_modifiers.OrderBy(modifier => modifier.Type);
            foreach (var modifier in modifiers)
                value =  modifier.Apply(value);
            Amount = value;
        }
    }
}