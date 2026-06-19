using System.Collections.Generic;
using Project.Resources;
using UnityEngine;

namespace Project.Map
{
    public class Producer : TileComponent,  IModiferHandler, IDataTileComponent<ProducerData>
    {
        [field: SerializeField] public ProducerData Data { get; private set; }
        
        private ModiferHandler m_handler;
        
        public bool Consume()
        {
            foreach (var info in Data.ResourcesToConsume)
            {
                if(info.Resource.Value < info.Amount) return false;
                info.Resource.Value -= info.Amount;
            }
            return true;
        }

        public void Produce()
        {
            foreach (var info in Data.ResourcesToConsume) 
                info.Resource.Value += info.Amount;
        }
        
        private void OnEnable() => ProducerTickable.Subscribe(this);

        private void OnDisable() => ProducerTickable.Unsubscribe(this);

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