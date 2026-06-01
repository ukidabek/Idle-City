using System.Collections.Generic;
using System.Linq;
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
        
        [Space]
        UnityEvent<IReadOnlyList<Tile>> OnAvailableTilesSelected = new UnityEvent<IReadOnlyList<Tile>>();
        
        private void Start()
        {
            var groundTiles = m_tileDatabase.GetTilesByCategory(m_groundTileCategory);
            var depositTiles = m_tileDatabase.GetTilesByCategory(m_desertTileCategory);
            
            for (var i = 0; i < m_size.y; i++)
            {
                for (var j = 0; j < m_size.x; j++)
                {
                    var cell = new Vector3Int(j, i, 0);

                    var index = Random.Range(0, groundTiles.Count);
                    var instance = Instantiate(groundTiles[index]);
                    m_mapManager.PlaceTile(cell, instance);

                    if(!instance.TryGetComponent(out IGround ground)) continue;

                    var availableDeposits = ground.AvailableDeposits;
                    if (availableDeposits.Count == 0) continue;
                    
                    if (Random.value > .6f) continue;
                    var depositToSpawn = depositTiles
                        .Where(deposit => availableDeposits.Contains(deposit.ID))
                        .OrderBy(_ => Random.value)
                        .First();
                    m_mapManager.PlaceTile(cell, Instantiate(depositToSpawn));
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