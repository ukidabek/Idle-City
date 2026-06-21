using System;
using System.Collections.Generic;
using Project.Map;
using UnityEngine;

namespace Code.Upgrades
{
    [Serializable]
    public class ProducerLevel : Level
    {
        [SerializeField] private ProducerModifierInfo[] m_modifiers = null;
        public IReadOnlyList<ProducerModifierInfo> Modifiers => m_modifiers;
    }
}