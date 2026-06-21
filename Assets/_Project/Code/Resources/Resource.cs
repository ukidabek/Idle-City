using System.Collections.Generic;
using UnityEngine;
using Values;

namespace Project.Resources
{
    [CreateAssetMenu(fileName = "Resource", menuName = "Resource/Resource")]
    public class Resource : BaseValue<float>, IModiferHandler
    {
        [field: SerializeField] public float BaseGain { get; private set; } = .25f;
        public float Gain { get; private set; }
        
        [field: SerializeField] public Sprite Image { get; private set; }

        private ModiferHandler m_modifierHandler = null;
        
        public override float Value
        {
            get => base.Value; 
            set => base.Value = Mathf.Clamp(value, 0, float.MaxValue);
        }

        public void Consume(float amount)
        {
            Value += amount;
        }

        public void Produce(float amount)
        {
            Value += amount;
        }
        
        private void OnEnable()
        {
            Value = 0;
            Gain = BaseGain;
            m_modifierHandler = new ModiferHandler(() => BaseGain, OnValueRecalculated);
        }

        private void OnValueRecalculated(float value) => Gain = value;

        private void OnDestroy()
        {
            m_modifierHandler.Dispose();
            m_modifierHandler = null;
        }
        
        public void Apply(IEnumerable<AmountModifier> modifiers) => m_modifierHandler.Apply(modifiers);

        public void Remove(IEnumerable<AmountModifier> modifiers) => m_modifierHandler.Remove(modifiers);

        public void Apply(AmountModifier modifier) => m_modifierHandler.Apply(modifier);

        public void Remove(AmountModifier modifier) => m_modifierHandler.Remove(modifier);
    }
}