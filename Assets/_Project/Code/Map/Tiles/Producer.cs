using Project.Resources;
using UnityEngine;

namespace Project.Map
{
    public class Producer : TileComponent, IProducer, IDataTileComponent<ProducerData>
    {
        [field: SerializeField] public ProducerData Data { get; private set; }

        private ProducerModifierRouter m_router = new ProducerModifierRouter();
        
        public void Apply(ProducerModifier modifier) => m_router?.Apply(modifier);
        public void Remove(ProducerModifier modifier) => m_router?.Remove(modifier);

        public bool Consume()
        {
            const Flow flow = Flow.Consumption;
            foreach (var info in Data.GetEffectiveAmount(flow))
            {
                var amount = m_router.GetEffectiveAmount(info.Resource, flow, info.Amount);
                if (info.Resource.Value < amount) 
                    return false;
                
                info.Resource.Value -= amount;
            }
            return true;
        }

        public void Produce()
        {
            const Flow flow = Flow.Production;
            foreach (var info in Data.GetEffectiveAmount(flow))
            {
                var amount = m_router.GetEffectiveAmount(info.Resource, flow, info.Amount);
                info.Resource.Value += amount;
            }
        }

        private void OnEnable() => ProducerTickable.Subscribe(this);

        private void OnDisable() => ProducerTickable.Unsubscribe(this);

        private void OnDestroy()
        {
            m_router.Dispose();
            m_router = null;
        }
    }
}
