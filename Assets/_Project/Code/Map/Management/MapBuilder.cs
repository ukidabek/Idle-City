using System.Collections.Generic;
using System.Linq;
using Code.Generator;
using Project.Map.Events;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using Utilities.General.Events;

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
        [SerializeField] private GeneratorEngine  m_generatorEngine = null;
        [SerializeField] private TilePlacementEvent m_tilePlacementEvent = null;
        [Space]
        [SerializeField, Range(0f, 1f)] private float m_focusX = 0.5f;
        [SerializeField, Range(0f, 1f)] private float m_focusY = 0.5f;
        [SerializeField] private Vector2IntEvent m_onMapReadyEvent = null;

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
                    var groundInstance = Instantiate(tile);
                    m_tilePlacementEvent?.Invoke(new TilePlacement(cell, groundInstance));

                    var sampletDeposits = depositTiles
                        .Select(tile => tile.SamplePosition(i, j))
                        .OrderByDescending(tile => tile.Sample)
                        .Where(tile => tile.Sample > 0);

                    var deposit = sampletDeposits.FirstOrDefault();
                    if (deposit == null) continue;
                    var depositInstance = Instantiate(deposit.Tile);
                    m_tilePlacementEvent?.Invoke(new TilePlacement(cell, depositInstance));
                }
            }

            var focusCell = new Vector2Int(
                Mathf.RoundToInt(m_focusX * (texture.width - 1)),
                Mathf.RoundToInt(m_focusY * (texture.height - 1)));
            m_onMapReadyEvent?.Invoke(focusCell);
        }

        public void SelectTilesAvailableToBuild(Tile tile)
        {
        }

        public void BuildTile(TilePlacement placement)
        {
            var instance = Instantiate(placement.Tile);
            m_tilePlacementEvent?.Invoke(new TilePlacement(placement.Cell, instance));
        }
    }
}