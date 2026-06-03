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

        protected HashSet<IClient>  m_clients = new HashSet<IClient>(30);
        
        public void Update(int tickRate = 1)
        {
            AmountAdd = AmountSubtract = 0;
            foreach (var client in m_clients)
            {
                switch (client.Type)
                {
                    case ClientType.Producer:
                        AmountAdd += client.Amount;
                        break;
                    case ClientType.Consumer:
                        AmountSubtract += client.Amount;
                        break;
                }
            }

            Value += (BaseGain + AmountAdd - AmountSubtract) / tickRate;
        }

        private void OnEnable()
        {
            m_clients.Clear();
            Value = 0;
        }

        public void RegisterClient(IClient client) => m_clients.Add(client);

        public void UnregisterClient(IClient client) => m_clients.Remove(client);
    }

    public enum ClientType
    {
        Producer,
        Consumer    
    }
    
    public interface IClient
    {
        ClientType Type { get; }
        float Amount { get; }
    }
}