using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utilities.General;
using Random = UnityEngine.Random;

namespace Project.Map
{
    [RequireComponent(typeof(Grid))]
    public class MapManager : MonoBehaviour
    {
        [SerializeField] private Grid m_grid = null;
            
        private readonly Dictionary<Vector3Int,Tile> m_groundTiles = new Dictionary<Vector3Int, Tile>(30);
        public UnityEvent<Tile> OnTileSelected = new UnityEvent<Tile>();
        [field: SerializeField,ReadOnly] public Tile SelectedTile { get; private set; }

        public void PlaceTile(Tile destination, Tile tile)
        {
            var position = destination.transform.position;
            PlaceTile(position, tile);
        }

        public void PlaceTile(Vector3 position, Tile tile)
        {
            var cell = m_grid.WorldToCell(position);
            PlaceTile(cell, tile);
        }
        
        public void PlaceTile(Vector3Int cell, Tile tile)
        {
            var position = m_grid.GetCellCenterWorld(cell);
            var tileTransform = tile.transform;
            tileTransform.position = position;
            tileTransform.SetParent(m_grid.transform);
            m_groundTiles.Add(cell, tile);

        }

        private void Reset() => m_grid = GetComponent<Grid>();

        public void SelectTile(Vector3 position)
        {
            var cell = m_grid.WorldToCell(position);
            SelectedTile = m_groundTiles.GetValueOrDefault(cell);
            OnTileSelected.Invoke(SelectedTile);
        }
    }
}