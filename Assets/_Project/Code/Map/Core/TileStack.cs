using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.Map
{
    public class TileStack : IReadOnlyList<Tile>
    {
        private readonly List<Tile> m_tiles = new List<Tile>(5);
        public int Count => m_tiles.Count;
        public Tile Top => m_tiles.Count > 0 ? m_tiles[^1] : null;
        public Tile this[int index] => m_tiles[index];
        private readonly List<ITielComponent>  m_components = new List<ITielComponent>();
        public IReadOnlyList<ITielComponent> Components => m_components;
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
            m_components.AddRange(tile.GetComponents<ITielComponent>());
        }

        public Tile Pop()
        {
            var listIndex = m_tiles.Count - 1;
            var tile = m_tiles[listIndex];
            var components = tile.GetComponents<ITielComponent>();
            m_components.RemoveAll(components.Contains);
            m_tiles.RemoveAt(listIndex);

            var first = Peek();
            if (first == null) return tile;
            
            var effects = first.GetComponents<IOnCoveredEffect>();
            foreach (var effect in effects) 
                effect.Undo();

            return tile;
        }

        public Tile Peek() => m_tiles.Count == 0 ? null : m_tiles[^1];
        
        public void Clear() => m_tiles.Clear();
        public IEnumerator<Tile> GetEnumerator() => m_tiles.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}