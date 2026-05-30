using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Map
{
    public class TileBuilder : MonoBehaviour
    {
        [SerializeField, Min(0)] private Vector2Int m_size = new Vector2Int(1, 1);

        [SerializeField] private TileDatabase m_tileDatabase = null;
        [SerializeField] private TileCategory m_groundTileCategory = null;

        [Space]
        [SerializeField] private MapManager m_mapManager = null;
        
        [Space]
        UnityEvent<IReadOnlyList<Tile>> OnAvailableTilesSelected = new UnityEvent<IReadOnlyList<Tile>>();

        private void Awake()
        {
        }
        
        private void Start()
        {
            var groundTiles = m_tileDatabase.GetTilesByCategory(m_groundTileCategory);

            for (var i = 0; i < m_size.y; i++)
            {
                for (var j = 0; j < m_size.x; j++)
                {
                    var cell = new Vector3Int(j, i, 0);

                    var index = Random.Range(0, groundTiles.Count);
                    var tile = groundTiles[index];
                    var instance = Instantiate(tile);
                    m_mapManager.PlaceTile(cell, instance);
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