namespace Project.Resources
{
    public abstract class AmountModifier
    {
        public abstract ModifierType Type { get; }
        public abstract int Order { get; }
        public abstract float Apply(float amount);
    }
}