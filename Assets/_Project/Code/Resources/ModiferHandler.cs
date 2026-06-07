using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.Resources
{
    public class ModiferHandler : IModiferHandler, IDisposable
    {
        private List<AmountModifier> m_modifiers = null;

        private Func<float> m_baseValue = null;
        
        public event Action<float> OnValueRecalculated;
        
        public ModiferHandler(Func<float> baseValue, Action<float> onValueRecalculated, int capacity = 10)
        {
            m_baseValue = baseValue;
            m_modifiers = new List<AmountModifier>(capacity);
            OnValueRecalculated = onValueRecalculated;
        }

        public void Apply(IEnumerable<AmountModifier> modifiers)
        {
            m_modifiers.AddRange(modifiers);
            RecalculateValue(); 
        }

        public void Remove(IEnumerable<AmountModifier> modifiers)
        {
            foreach (var modifier in modifiers)
            {
                if (!m_modifiers.Contains(modifier)) continue;
                m_modifiers.Remove(modifier);
            }
            RecalculateValue();
        }

        public void Apply(AmountModifier modifier)
        {
            m_modifiers.Add(modifier);
            RecalculateValue();
        }

        public void Remove(AmountModifier modifier)
        {
            if (!m_modifiers.Contains(modifier)) return;
            m_modifiers.Remove(modifier);
            RecalculateValue();
        }

        private void RecalculateValue()
        {
            var value = m_baseValue();
            var modifiers = m_modifiers.OrderBy(modifier => modifier.Type);
            foreach (var modifier in modifiers)
                value =  modifier.Apply(value);
            OnValueRecalculated?.Invoke(value);
        }

        public void Dispose()
        {
            m_baseValue = null;
            OnValueRecalculated = null;
            m_modifiers.Clear();
        }
    }
}