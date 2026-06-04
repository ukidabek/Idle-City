using System;
using System.Collections.Generic;
using UnityEngine;
using Values;

namespace Project.Resources
{
    [CreateAssetMenu(fileName = "Resource", menuName = "Resource")]
    public class Resource : BaseValue<float>
    {
        [field: SerializeField] public float BaseGain { get; private set; } = .25f;
        public float AmountAdd { get; private set; }
        public float AmountSubtract { get; private set; }
        [field: SerializeField] public Sprite Image { get; private set; }

        private HashSet<IResourceClient> m_consumers = new HashSet<IResourceClient>(30);
        private HashSet<IResourceClient> m_producers = new HashSet<IResourceClient>(30);

        public override float Value
        {
            get => base.Value; 
            set => base.Value = Mathf.Clamp(value, 0, float.MaxValue);
        }

        public void Tick(int tickRate = 1)
        {
            AmountAdd = AmountSubtract = 0;
            using (BulkEdit())
            
            foreach (var consumer in m_consumers)
                Value -= consumer.Amount / tickRate;
            
            foreach (var producer in m_producers) 
                Value += producer.Amount / tickRate;
        }

        private void OnEnable()
        {
            m_consumers.Clear();
            m_producers.Clear();
            Value = 0;
        }

        public void RegisterClient(IResourceClient client) => GetHashSet(client).Add(client);
        
        public void UnregisterClient(IResourceClient client) => GetHashSet(client).Remove(client);

        private HashSet<IResourceClient> GetHashSet(IResourceClient client)
        {
            var set = client.Type switch
            {
                ClientType.Consumer => m_consumers,
                ClientType.Producer => m_producers,
                _ => throw new ArgumentOutOfRangeException()
            };
            return set;
        }
    }

    public enum ClientType
    {
        Producer,
        Consumer    
    }
}