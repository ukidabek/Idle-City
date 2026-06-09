using System.Collections.Generic;
using UnityEngine;

namespace Project.Map
{
    [CreateAssetMenu(menuName = "Map/Tiles/NeighborValidatorData", fileName = "NeighborValidatorData")]

    public class NeighborValidatorData : TileData
    {
        [SerializeField] private NeighborDirection m_neighborDirection = NeighborDirection.Up | NeighborDirection.Down | NeighborDirection.Left | NeighborDirection.Right;
        
        [SerializeField] private NeighborCondition[] m_neighborConditions = null;
        public NeighborCondition[] NeighborConditions => m_neighborConditions;
        
        public IEnumerable<Vector3Int> ToOffsets(Vector3Int offset = default)
        {
            if (m_neighborDirection.HasFlag(NeighborDirection.UpLeft))    yield return offset + Vector3Int.up   + Vector3Int.left;
            if (m_neighborDirection.HasFlag(NeighborDirection.UpRight))   yield return offset + Vector3Int.up   + Vector3Int.right;
            if (m_neighborDirection.HasFlag(NeighborDirection.DownLeft))  yield return offset + Vector3Int.down + Vector3Int.left;
            if (m_neighborDirection.HasFlag(NeighborDirection.DownRight)) yield return offset + Vector3Int.down + Vector3Int.right;
            if (m_neighborDirection.HasFlag(NeighborDirection.Left))      yield return offset + Vector3Int.left;
            if (m_neighborDirection.HasFlag(NeighborDirection.Right))     yield return offset + Vector3Int.right;
            if (m_neighborDirection.HasFlag(NeighborDirection.Up))        yield return offset + Vector3Int.up;
            if (m_neighborDirection.HasFlag(NeighborDirection.Down))      yield return offset + Vector3Int.down;
        }
    }
}