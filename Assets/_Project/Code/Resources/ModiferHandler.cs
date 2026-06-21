using System;
using System.Collections;
using System.Collections.Generic;

namespace Project.Resources
{
    public class ModiferHandler : IModiferHandler, IReadOnlyList<AmountModifier>, IDisposable
    {
        private static int CompareByOrder(AmountModifier a, AmountModifier b) => a.Order.CompareTo(b.Order);
        private static readonly Comparison<AmountModifier> ModifierComparison = CompareByOrder;

        private List<AmountModifier> m_modifiers = null;
        private Func<float> m_baseValue = null;

        public event Action<float> OnValueRecalculated;

        public ModiferHandler(int capacity = 10) : this(null, null, capacity)
        {
        }

        public ModiferHandler(Func<float> baseValue, Action<float> onValueRecalculated, int capacity = 10)
        {
            m_baseValue = baseValue;
            m_modifiers = new List<AmountModifier>(capacity);
            OnValueRecalculated = onValueRecalculated;
        }

        public void Apply(IEnumerable<AmountModifier> modifiers)
        {
            m_modifiers.AddRange(modifiers);
            m_modifiers.Sort(ModifierComparison);
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
            m_modifiers.Sort(ModifierComparison);
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
            if(m_baseValue == null) return;
            var value = m_baseValue();

            foreach (var modifier in m_modifiers)
                value = modifier.Apply(value);
            
            OnValueRecalculated?.Invoke(value);
        }

        public float Calculate(float baseAmount)
        {
            var value = baseAmount;
            foreach (var modifier in m_modifiers)
                value = modifier.Apply(value);
            return value;
        }

        public int Count => m_modifiers.Count;
        public AmountModifier this[int index] => m_modifiers[index];
        public IEnumerator<AmountModifier> GetEnumerator() => m_modifiers.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => m_modifiers.GetEnumerator();

        public void Dispose()
        {
            m_baseValue = null;
            OnValueRecalculated = null;
            m_modifiers.Clear();
        }
    }
}
