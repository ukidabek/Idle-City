using System.Collections.Generic;
using Project.Resources;

namespace Project.Map
{
    public interface IProducer
    {
        void Apply(ProducerModifier modifier);
        void Remove(ProducerModifier modifier);

    }
}
