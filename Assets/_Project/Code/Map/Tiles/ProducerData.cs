using System;
using System.Collections.Generic;
using Project.Resources;
using UnityEngine;

namespace Project.Map
{
    public readonly struct ActiveAmount
    {
        public readonly Resource Resource;
        public readonly float Amount;

        public ActiveAmount(Resource resource, float amount)
        {
            Resource = resource;
            Amount = amount;
        }
    }
    [CreateAssetMenu(menuName = "Map/Tiles/ClientData", fileName = "ClientData")]
    public class ProducerData : TileData, IProducer
    {
        [field: SerializeField] public ClientResourceInfo[] ResourcesToConsume { get; private set; } = Array.Empty<ClientResourceInfo>();
        [field: SerializeField] public ClientResourceInfo[] ResourcesToProduce { get; private set; } = Array.Empty<ClientResourceInfo>();
        private ProducerModifierRouter m_router = null;
        
        private void OnEnable()
        {
            m_router = new ProducerModifierRouter();
        }

        private void OnDisable()
        {
            m_router.Dispose();
            m_router = null;
        }
    
        public void Apply(ProducerModifier modifier) => m_router.Apply(modifier);
        
        public void Remove(ProducerModifier modifier) => m_router.Remove(modifier);
        

        public IEnumerable<ActiveAmount> GetEffectiveAmount(Flow flow)
        {
            var info = flow switch
            {
                Flow.Consumption => ResourcesToConsume,
                Flow.Production => ResourcesToProduce,
                _ => null
            };

            foreach (var _info in info)
            {
                yield return new ActiveAmount(
                    _info.Resource,
                    m_router.GetEffectiveAmount(_info.Resource, flow, _info.Amount));
            }
        }
    }
}
