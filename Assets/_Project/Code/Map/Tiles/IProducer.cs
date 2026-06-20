using Project.Resources;

namespace Project.Map
{
    public interface IProducer
    {
        void Apply(AmountModifier modifier, ModifierTarget target);
        void Remove(AmountModifier modifier, ModifierTarget target);
    }
}
