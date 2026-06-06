using System;
using Project.Resources;
using UnityEngine;

namespace Project.Map
{
    [Serializable]
    public class Cost
    {
        [SerializeField] private Resource m_resource = null;
        public Resource Resource => m_resource;
        
        [SerializeField] private int m_amount = 0;
        public int Amount => m_amount;
        
        public bool CanAfford => m_resource.Value >= m_amount;
    }
}