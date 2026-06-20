using System;
using Project.Resources;
using UnityEngine;

namespace Project.Map
{
    [CreateAssetMenu(menuName = "Map/Tiles/ClientData", fileName = "ClientData")]
    public class ProducerData : TileData
    {
        [field: SerializeField] public ClientResourceInfo[] ResourcesToConsume { get; private set; } = Array.Empty<ClientResourceInfo>();
        [field: SerializeField] public ClientResourceInfo[] ResourcesToProduce { get; private set; } = Array.Empty<ClientResourceInfo>();

        private ModiferHandler m_produceHandler;
        private ModiferHandler m_consumeHandler;

        private void OnEnable()
        {
            m_produceHandler = new ModiferHandler(() => 0f, _ => { });
            m_consumeHandler = new ModiferHandler(() => 0f, _ => { });
        }

        private void OnDisable()
        {
            m_produceHandler?.Dispose(); m_produceHandler = null;
            m_consumeHandler?.Dispose(); m_consumeHandler = null;
        }

        public float GetEffectiveAmount(float baseAmount, ModifierTarget target) => HandlerFor(target).Calculate(baseAmount);

        public void Apply(AmountModifier modifier, ModifierTarget target) => HandlerFor(target).Apply(modifier);
        public void Remove(AmountModifier modifier, ModifierTarget target) => HandlerFor(target).Remove(modifier);

        private ModiferHandler HandlerFor(ModifierTarget target)
            => target == ModifierTarget.Production ? m_produceHandler : m_consumeHandler;
    }
}
