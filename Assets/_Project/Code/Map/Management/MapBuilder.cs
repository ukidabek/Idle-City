using System.Collections.Generic;
using System.Linq;
using Code.Generator;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Project.Map.Generation
{
    public class MapBuilder : MonoBehaviour
    {
        private class DepostiSampler
        {
            public Tile Tile { get; }
            private readonly Deposit m_deposit;
            private readonly Texture2D m_texture;
            public float Sample { get; private set; }

            public DepostiSampler(Tile tile, Texture2D texture)
            {
                Tile = tile;
                m_texture = texture;
                m_deposit = tile.GetComponent<Deposit>();
                Assert.IsNotNull(m_deposit);
            }

            public DepostiSampler SamplePosition(int x, int y)
            {
                var depositData = m_deposit.Data;
                var position = new Vector2Int(x, y);
                var color = m_texture.GetPixel(position.x, position.y);
                
                Sample = color.b >= depositData.MinimalWaterDistance ? 0 : -1;
                if (Sample < 0) return this;

                Sample = Random.value <= depositData.SpawnChance ? 0 : -1;
                if (Sample < 0) return this;
               
                position = depositData.NoiseOffset + position;
                color = m_texture.GetPixel(position.x, position.y);
                Sample = color.g;
                
                return this;
            }
        }
        
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
            
            private readonly Range<T>[] m_range;

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
            var groundTiles = new RangeCollection<Tile>(m_tileDatabase.GetTilesByCategory(m_groundTileCategory)
                .OrderBy(tile =>
                {
                    var ground = tile.GetComponent<Ground>();
                    return ground.Order;
                }));
            var texture = m_generatorEngine.Texture;
            var depositTiles = m_tileDatabase
                .GetTilesByCategory(m_desertTileCategory)
                .Select(tile => new DepostiSampler(tile, texture))
                .ToList();
            
            for (var i = 0; i < texture.height; i++)
            {
                for (var j = 0; j < texture.width; j++)
                {
                    var cell = new Vector3Int(i, j, 0);
                    var color = texture.GetPixel(i, j);
                    var read = color.r;
                    var tile = groundTiles.Get(read);
                    m_mapManager.PlaceTile(cell, Instantiate(tile));

                    var sampletDeposits = depositTiles
                        .Select(tile => tile.SamplePosition(i, j))
                        .OrderByDescending(tile => tile.Sample)
                        .Where(tile => tile.Sample > 0);

                    var deposit = sampletDeposits.FirstOrDefault();
                    if (deposit == null) continue;
                    m_mapManager.PlaceTile(cell, Instantiate(deposit.Tile));
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