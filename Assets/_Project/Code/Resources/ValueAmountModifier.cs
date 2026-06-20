namespace Project.Resources
{
    public class ValueAmountModifier : AmountModifier
    {
        private readonly float m_value = 0;

        public ValueAmountModifier(float value)
        {
            m_value = value;
        }

        public override ModifierType Type => ModifierType.Value;
        public override int Order => 1;
        public override float Apply(float amount) => amount + m_value;
    }
}