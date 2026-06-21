namespace Project.Resources
{
    public abstract class AmountModifier
    {
        public abstract int Order { get; }
        public abstract float Apply(float amount);
    }
}