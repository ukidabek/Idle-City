using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Map
{
    [RequireComponent(typeof(Grid))]
    public class MapManager : MonoBehaviour
    {
        [SerializeField] private Grid m_grid = null;

        private readonly Dictionary<Vector3Int, TileStack> m_tilesDictionary = new Dictionary<Vector3Int, TileStack>(30);
        public UnityEvent<TileStack> OnTileSelected = new UnityEvent<TileStack>();
        public TileStack SelectedTile { get; private set; }

        public void PlaceTile(TileStack destinationStack, Tile tile)
        {
            var destinationTile = destinationStack.Pop();
            var position = destinationTile.transform.position;
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
            if(m_tilesDictionary.TryGetValue(cell, out var stack))
            {
                stack.Push(tile);
            }
            else
                m_tilesDictionary.Add(cell, new TileStack(tile));

        }

        private void Reset() => m_grid = GetComponent<Grid>();

        public void SelectTile(Vector3 position)
        {
            var cell = m_grid.WorldToCell(position);
            SelectedTile = m_tilesDictionary.GetValueOrDefault(cell);
            OnTileSelected.Invoke(SelectedTile);
        }
    }
}