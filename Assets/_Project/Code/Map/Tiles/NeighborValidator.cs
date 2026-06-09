using System.Collections.Generic;
using System.Linq;
using Project.Resources;
using UnityEngine;

namespace Project.Map
{
    public class NeighborValidator : TileComponent, IDataTileComponent<NeighborValidatorData>
    {

        [field: SerializeField] public NeighborValidatorData Data { get; private set; }
        [SerializeField] private Client[] m_affectedClients = null;
        private readonly List<AmountModifier> m_modifiersCache = new List<AmountModifier>(10);
        
        private void Awake() => Tile.OnTilePlaced.AddListener(ValidateNeighbors);

        private void OnDestroy() => Tile.OnTilePlaced.RemoveListener(ValidateNeighbors);

        private void ValidateNeighbors()
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
                    .Where(modifier => modifier != null);
                m_modifiersCache.AddRange(modifiers);
            }

            foreach (var client in m_affectedClients) 
                client.Apply(m_modifiersCache);
        }
    }
}