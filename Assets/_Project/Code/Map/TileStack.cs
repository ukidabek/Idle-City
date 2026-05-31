using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Map
{
    public class TileStack : IReadOnlyList<Tile>
    {
        private readonly List<Tile> m_tiles = new List<Tile>(5);

        public int Count => m_tiles.Count;
        public Tile Top => m_tiles.Count > 0 ? m_tiles[^1] : null;
        
        public Tile this[int index] => m_tiles[index];

        public TileStack(Tile tile)
        {
            m_tiles.Add(tile);
        }

        public void Push(Tile tile)
        {
            // TODO: pre-push validation (e.g. check if tile is allowed on current top)
        
            m_tiles.Add(tile);
            // TODO: post-push events (e.g. notify systems tile was added)
            if (m_tiles.Count <= 1) return;
            tile.transform.position += Vector3.up * .2f;
        }

        public Tile Pop()
        {
            // TODO: pre-pop validation (e.g. check if tile can be removed)
        
            var listIndex = m_tiles.Count - 1;
            var tile = m_tiles[listIndex];
            m_tiles.RemoveAt(listIndex);
        
            // TODO: post-pop events (e.g. notify systems tile was removed)
        
            return tile;
        }

        public Tile Peek() => m_tiles.Count == 0 ? null : m_tiles[^1];

        public bool TryPop(out Tile tile)
        {
            if (m_tiles.Count == 0)
            {
                tile = null;
                return false;
            }
            tile = Pop();
            return true;
        }

        public void Clear()
        {
            // TODO: notify systems all tiles are being removed if needed
            m_tiles.Clear();
        }

        // IEnumerable — iterates bottom to top (index 0 upward)
        public IEnumerator<Tile> GetEnumerator() => m_tiles.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}