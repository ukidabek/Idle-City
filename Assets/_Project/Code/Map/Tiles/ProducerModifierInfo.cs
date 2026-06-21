using System;
using Project.Resources;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Map
{
    [Serializable]
    public class ProducerModifierInfo
    {
        [SerializeField] ModifierInfo m_info;
        [SerializeField] private Resource m_target = null;
        [FormerlySerializedAs("m_target")] 
        [SerializeField] private Flow m_flow;

        public ProducerModifier? Build()
        {
            var amountModifier = AmountModifierFactory.Create(m_info);
            if (amountModifier == null) return null;
            return new ProducerModifier(amountModifier, m_flow, m_target);
        }
    }
}