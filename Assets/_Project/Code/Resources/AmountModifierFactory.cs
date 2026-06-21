using System;
using UnityEngine;

namespace Project.Resources
{
    public static class AmountModifierFactory
    {
        public static AmountModifier Create(ModifierInfo info) => Create(info.Type, info.Value);
        public static AmountModifier Create(ModifierType type, float value) => type switch
        {
            ModifierType.Value   => new ValueAmountModifier(value),
            ModifierType.Percent => new PercentAmountModifier(value),
            _                    => null
        };
    }

    [Serializable]
    public struct ModifierInfo
    {
        [field: SerializeField] public ModifierType Type { get; private set; }
        [field: SerializeField] public float Value { get; private set; }
    }
}
