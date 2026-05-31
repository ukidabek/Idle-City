using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.Map
{
    [CreateAssetMenu]
    public class TileDatabase : ScriptableObject, IReadOnlyDictionary<TileID, Tile>
    {
        [SerializeField] private Tile[] m_tiles;

        private Dictionary<TileID, Tile> m_tileDictionary;
        private Dictionary<TileCategory, List<Tile>> m_tileCategoryDictionary;

        private void OnEnable()
        {
            m_tileDictionary = m_tiles.ToDictionary(tile => tile.ID, tile => tile);
            m_tileCategoryDictionary = m_tiles
                .GroupBy(tile => tile.TileCategory)
                .ToDictionary(group => group.Key, group => group.ToList());
        }

        private void OnDestroy()
        {
            m_tileDictionary.Clear();
            m_tileCategoryDictionary.Clear();
        }

        public IReadOnlyList<Tile> GetTilesByCategory(TileCategory category) => m_tileCategoryDictionary.TryGetValue(category, out var tiles) ? tiles : Array.Empty<Tile>();

        public IEnumerator<KeyValuePair<TileID, Tile>> GetEnumerator() => m_tileDictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => m_tileDictionary.Count;

        public bool ContainsKey(TileID key) => m_tileDictionary.ContainsKey(key);

        public bool TryGetValue(TileID key, out Tile value) => m_tileDictionary.TryGetValue(key, out value);

        public Tile this[TileID key] => m_tileDictionary[key];

        public IEnumerable<TileID> Keys => m_tileDictionary.Keys;
        public IEnumerable<Tile> Values => m_tileDictionary.Values;
    }
}