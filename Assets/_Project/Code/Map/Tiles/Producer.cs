using Project.Resources;
using UnityEngine;

namespace Project.Map
{
    public class Producer : TileComponent, IProducer, IDataTileComponent<ProducerData>
    {
        [field: SerializeField] public ProducerData Data { get; private set; }

        private ModiferHandler m_produceHandler;
        private ModiferHandler m_consumeHandler;

        private void Awake()
        {
            m_produceHandler = new ModiferHandler();
            m_consumeHandler = new ModiferHandler();
        }

        public void Apply(AmountModifier modifier, ModifierTarget target) => HandlerFor(target)?.Apply(modifier);
        public void Remove(AmountModifier modifier, ModifierTarget target) => HandlerFor(target)?.Remove(modifier);

        private ModiferHandler HandlerFor(ModifierTarget target)
            => target == ModifierTarget.Production ? m_produceHandler : m_consumeHandler;

        public bool Consume()
        {
            foreach (var info in Data.ResourcesToConsume)
            {
                var effectiveAmount = Data.GetEffectiveAmount(info.Amount, ModifierTarget.Consumption);
                var amount = m_consumeHandler.Calculate(effectiveAmount);
                if (info.Resource.Value < amount) return false;
                info.Resource.Value -= amount;
            }
            return true;
        }

        public void Produce()
        {
            foreach (var info in Data.ResourcesToProduce)
            {
                var effectiveAmount = Data.GetEffectiveAmount(info.Amount, ModifierTarget.Production);
                info.Resource.Value += m_produceHandler.Calculate(effectiveAmount);
            }
        }

        private void OnEnable() => ProducerTickable.Subscribe(this);

        private void OnDisable() => ProducerTickable.Unsubscribe(this);

        private void OnDestroy()
        {
            m_produceHandler?.Dispose(); m_produceHandler = null;
            m_consumeHandler?.Dispose(); m_consumeHandler = null;
        }
    }
}
