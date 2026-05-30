using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Project.Map
{
    [RequireComponent(typeof(Grid))]
    public class MapManager : MonoBehaviour
    {
        [SerializeField] private Grid m_grid;

        [SerializeField, Min(0)] private Vector2Int m_size = new Vector2Int(1, 1);
        [SerializeField] private TileDatabase m_tileDatabase = null;
        [SerializeField] private TileCategory m_groundTileCategory = null;
        
        private Dictionary<Vector3Int,Tile> m_groundTiles;

        public UnityEvent<Tile> OnTileSelected = new UnityEvent<Tile>();
        
        private void Awake()
        {
            m_groundTiles = new Dictionary<Vector3Int, Tile>(m_size.x * m_size.y * 3);
        }

        private void Start()
        {
            var groundTiles = m_tileDatabase.GetTilesByCategory(m_groundTileCategory);

            for (var i = 0; i < m_size.y; i++)
            {
                for (var j = 0; j < m_size.x; j++)
                {
                    var cell = new Vector3Int(j, i, 0);
                    var position = m_grid.GetCellCenterWorld(cell);
                    var index = Random.Range(0, groundTiles.Count);
                    var tile = groundTiles[index];
                    var instance = Instantiate(tile, position, Quaternion.identity, transform);
                    m_groundTiles.Add(cell, instance);
                }
            }
        }

        private void Reset() => m_grid = GetComponent<Grid>();

        public void SelectTile(Vector3 position)
        {
            var cell = m_grid.WorldToCell(position);
            OnTileSelected.Invoke(m_groundTiles.GetValueOrDefault(cell));
        }
    }
}