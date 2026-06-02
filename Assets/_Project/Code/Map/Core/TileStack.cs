using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Map
{
    public class TileStack : IReadOnlyList<Tile>
    {
        private readonly List<Tile> m_tiles = new List<Tile>(5);
        public int Count => m_tiles.Count;
        public Tile this[int index] => m_tiles[index];
        
        public event Action StackChanged;
        public event Action OnTilePopped;
        public event Action OnTilePushed;
        
        public TileStack(Tile tile) => Add(tile);

        public void Push(Tile tile)
        {
            Add(tile);
            if (m_tiles.Count <= 1) return;
            tile.transform.position += Vector3.up * .2f;
        }

        private void Add(Tile tile)
        {
            var last = Peek();
            if (last != null)
            {
                var coveredEffects = last.GetComponents<IOnCoveredEffect>();
                foreach (var effect in coveredEffects)
                    effect.Apply();
            }

            m_tiles.Add(tile);
            OnTilePushed?.Invoke();
            StackChanged?.Invoke();
        }

        public Tile Pop()
        {
            if (m_tiles.Count <= 1) return null;
            
            var listIndex = m_tiles.Count - 1;
            var tile = m_tiles[listIndex];

            m_tiles.RemoveAt(listIndex);

            var first = Peek();
            if (first == null) return tile;
            
            var effects = first.GetComponents<IOnCoveredEffect>();
            foreach (var effect in effects) 
                effect.Undo();

            OnTilePopped?.Invoke();
            StackChanged?.Invoke();
            return tile;
        }

        public Tile Peek() => m_tiles.Count == 0 ? null : m_tiles[^1];
        
        public void Clear() => m_tiles.Clear();
        
        public IEnumerator<Tile> GetEnumerator() => m_tiles.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}