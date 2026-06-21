using Project.Resources;

namespace Project.Map
{
    public readonly struct ProducerModifier
    {
        public readonly AmountModifier Modifier;
        public readonly Flow Flow;
        public readonly Resource Target;

        public ProducerModifier(AmountModifier modifier, Flow flow, Resource target = null)
        {
            Modifier = modifier;
            Flow = flow;
            Target = target;
        }
    }
}