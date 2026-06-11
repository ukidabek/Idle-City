using System.Collections.Generic;
using System.Linq;
using Code.Generator;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Map
{
    public class TileBuilder : MonoBehaviour
    {
        [SerializeField, Min(0)] private Vector2Int m_size = new Vector2Int(1, 1);

        [SerializeField] private TileDatabase m_tileDatabase = null;
        [SerializeField] private TileCategory m_groundTileCategory = null;
        [SerializeField] private TileCategory m_desertTileCategory = null;
        [Space]
        [SerializeField] private MapManager m_mapManager = null;
        [SerializeField] private GeneratorEngine  m_generatorEngine = null;
        [Space]
        UnityEvent<IReadOnlyList<Tile>> OnAvailableTilesSelected = new UnityEvent<IReadOnlyList<Tile>>();
        
        
        private class RangeCollection<T>
        {
            private class Range<T>
            {
                public readonly float Min;
                public readonly float Max;
                public readonly T Item;

                public Range(float min, float max, T item)
                {
                    Min = min;
                    Max = max;
                    Item = item;
                }
            }
            
            private Range<T>[] m_range;

            public T Get(float value)
            {
                foreach (var range in m_range)
                {
                    if(value >= range.Min && value <= range.Max)
                        return range.Item;
                }
                
                return default;
            }
            
            public RangeCollection(IEnumerable<T> collection)
            {
                var increment = 1f / collection.Count();

                var current = 0f;
                m_range = collection.Select(item =>
                {
                    var nextValue = current + increment;
                    var instance = new Range<T>(current, nextValue, item);
                    current = nextValue;
                    return instance;
                }).ToArray();
            }
        }
        
        private void Start()
        {
            var groundTiles = new RangeCollection<Tile>( m_tileDatabase.GetTilesByCategory(m_groundTileCategory)
                .OrderBy(tile =>
                {
                    var ground = tile.GetComponent<Ground>();
                    return ground.Order;
                }));
            var depositTiles = m_tileDatabase.GetTilesByCategory(m_desertTileCategory);
            var texture = m_generatorEngine.Texture;
            
            for (var i = 0; i < texture.height; i++)
            {
                for (var j = 0; j < texture.width; j++)
                {
                    var cell = new Vector3Int(i, j, 0);
                    var color = texture.GetPixel(i, j);
                    var read = color.r;
                    var tile = groundTiles.Get(read);
                    m_mapManager.PlaceTile(cell, Instantiate(tile));
                }
            }
        }

        public void SelectTilesAvailableToBuild(Tile tile)
        {
        }

        public void BuildTile(Tile tile)
        {
            if (m_mapManager.SelectedTile == null) return;
            var instance = Instantiate(tile);
            m_mapManager.PlaceTile(m_mapManager.SelectedTile, instance);
        }
    }
}