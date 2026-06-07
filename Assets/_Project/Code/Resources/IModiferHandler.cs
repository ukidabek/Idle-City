using System.Collections.Generic;

namespace Project.Resources
{
    public interface IModiferHandler
    {
        void Apply(IEnumerable<AmountModifier> modifiers);
        void Remove(IEnumerable<AmountModifier> modifiers);
        void Apply(AmountModifier modifier);
        void Remove(AmountModifier modifier);
    }
}