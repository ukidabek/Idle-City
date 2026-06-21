using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.Map
{
    [RequireComponent(typeof(Producer))]
    public class NeighborValidator : TileComponent, IDataTileComponent<NeighborValidatorData>
    {
        [field: SerializeField] public NeighborValidatorData Data { get; private set; }
        [SerializeField] private Producer m_producer = null;
        private readonly List<ProducerModifier> m_modifiersCache = new List<ProducerModifier>(10);
        
        private void OnDestroy() => RemoveModifiers();

        public void ValidateNeighbors()
        {
            var map = Tile.Map;
            var cell = Tile.Cell;

            m_modifiersCache.Clear();

            var conditions = Data.NeighborConditions;
            foreach (var neighborCell in Data.ToOffsets(cell))
            {
                if (!map.TryGetValue(neighborCell, out var neighbor)) continue;
                var neighborTile = neighbor.Peek();
                var modifiers = conditions
                    .Select(neighborCondition => neighborCondition.Build(neighborTile))
                    .Where(modifier => modifier.HasValue)
                    .Select(modifier => modifier.Value);
                m_modifiersCache.AddRange(modifiers);
            }

            ApplyModifiers();
        }

        protected override void Reset()
        {
            base.Reset();
            m_producer = gameObject.GetComponent<Producer>();
        }

        private void ApplyModifiers()
        {
            foreach (var modifier in m_modifiersCache)
                m_producer.Apply(modifier);
        }

        private void RemoveModifiers()
        {
            foreach (var modifier in m_modifiersCache)
                m_producer.Remove(modifier);
        }
    }
}