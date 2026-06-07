using System.Collections.Generic;
using System.Linq;
using Project.Resources;
using UnityEngine;

namespace Project.Map
{
    public class NeighborValidator : TileComponent
    {
        [SerializeField] private NeighborDirection m_neighborDirection = NeighborDirection.Up | NeighborDirection.Down | NeighborDirection.Left | NeighborDirection.Right;
        [SerializeField] private NeighborCondition[] m_neighborConditions = null;
        [SerializeField] private Client[] m_affectedClients = null;
        private readonly List<AmountModifier> m_modifiersCache = new List<AmountModifier>(10);
        
        public static IEnumerable<Vector3Int> ToOffsets(NeighborDirection direction, Vector3Int offset = default)
        {
            if (direction.HasFlag(NeighborDirection.UpLeft))    yield return offset + Vector3Int.up   + Vector3Int.left;
            if (direction.HasFlag(NeighborDirection.UpRight))   yield return offset + Vector3Int.up   + Vector3Int.right;
            if (direction.HasFlag(NeighborDirection.DownLeft))  yield return offset + Vector3Int.down + Vector3Int.left;
            if (direction.HasFlag(NeighborDirection.DownRight)) yield return offset + Vector3Int.down + Vector3Int.right;
            if (direction.HasFlag(NeighborDirection.Left))      yield return offset + Vector3Int.left;
            if (direction.HasFlag(NeighborDirection.Right))     yield return offset + Vector3Int.right;
            if (direction.HasFlag(NeighborDirection.Up))        yield return offset + Vector3Int.up;
            if (direction.HasFlag(NeighborDirection.Down))      yield return offset + Vector3Int.down;
        }

        private void Awake() => Tile.OnTilePlaced.AddListener(ValidateNeighbors);

        private void OnDestroy() => Tile.OnTilePlaced.RemoveListener(ValidateNeighbors);

        public void ValidateNeighbors()
        {
            var map = Tile.Map;
            var cell = Tile.Cell;
            
            m_modifiersCache.Clear();

            foreach (var neighborCell in ToOffsets(m_neighborDirection, cell))
            {
                if (!map.TryGetValue(neighborCell, out var neighbor)) continue;
                var neighborTile = neighbor.Peek();
                var modifiers = m_neighborConditions
                    .Select(neighborCondition => neighborCondition.Build(neighborTile))
                    .Where(modifier => modifier != null);
                m_modifiersCache.AddRange(modifiers);
            }

            foreach (var client in m_affectedClients) 
                client.Apply(m_modifiersCache);
        }
    }
}