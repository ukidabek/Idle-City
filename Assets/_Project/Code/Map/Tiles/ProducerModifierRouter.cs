using System;
using System.Collections.Generic;
using Project.Resources;

namespace Project.Map
{
    public class ProducerModifierRouter : IDisposable
    {
        private ModiferHandler m_produceHandler = new ModiferHandler();
        private ModiferHandler m_consumeHandler = new ModiferHandler();

        private Dictionary<Resource, ModiferHandler> m_produceHandlerCache = new Dictionary<Resource, ModiferHandler>(10);
        private Dictionary<Resource, ModiferHandler> m_consumeHandlerCache = new Dictionary<Resource, ModiferHandler>(10);
        
        public void Apply(ProducerModifier modifier)
        {
            var handler = HandlerFor(modifier.Target, modifier.Flow, true);
            handler.Apply(modifier.Modifier);
        }

        public void Remove(ProducerModifier modifier)
        {
            var handler = HandlerFor(modifier.Target, modifier.Flow, true);
            handler.Remove(modifier.Modifier);
        }

        private ModiferHandler HandlerFor(Resource resource, Flow flow, bool createIfNotExist)
        {
            if (resource == null)
                return DefaultHandlerFor(flow);
            
            var dictionary = flow switch
            {
                Flow.Production  => m_produceHandlerCache,
                Flow.Consumption => m_consumeHandlerCache
            };
            
            if(dictionary.TryGetValue(resource, out var modiferHandler))
                return modiferHandler;
            
            if (!createIfNotExist)
                return DefaultHandlerFor(flow);
            
            modiferHandler = new ModiferHandler();
            dictionary.Add(resource, modiferHandler);
            return modiferHandler;
        }

        private ModiferHandler DefaultHandlerFor(Flow flow) =>
            flow switch
            {
                Flow.Consumption => m_consumeHandler,
                Flow.Production => m_produceHandler
            };

        public void Dispose()
        {
            m_produceHandler.Dispose();
            m_produceHandler = null;

            m_consumeHandler.Dispose();
            m_consumeHandler = null;

            foreach (var handler in m_produceHandlerCache.Values) handler.Dispose();
            m_produceHandlerCache.Clear();
            m_produceHandlerCache = null;

            foreach (var handler in m_consumeHandlerCache.Values) handler.Dispose();
            m_consumeHandlerCache.Clear();
            m_consumeHandlerCache = null;
        }

        public float GetEffectiveAmount(Resource resource, Flow flow, float baseAmount)
        {
            var handler = HandlerFor(resource, flow, false);
            return handler.Calculate(baseAmount);
        }
    }
}