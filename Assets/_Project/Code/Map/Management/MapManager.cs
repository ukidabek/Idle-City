using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Map
{
    [RequireComponent(typeof(Grid))]
    public class MapManager : MonoBehaviour, IReadOnlyDictionary<Vector3Int, TileStack>
    {
        [SerializeField] private Grid m_grid = null;

        private readonly Dictionary<Vector3Int, TileStack> m_tilesDictionary = new Dictionary<Vector3Int, TileStack>(30);
        public UnityEvent<TileStack> OnTileSelected = new UnityEvent<TileStack>();
        public TileStack SelectedTile { get; private set; }

        public void PlaceTile(TilePlacement placement)
        {
            if (placement.PlaceOnSelected)
            {
                if (SelectedTile == null) return;
                PlaceTile(SelectedTile, placement.Tile);
                return;
            }

            PlaceTile(placement.Cell, placement.Tile);
        }

        public void PlaceTiles(IEnumerable<TilePlacement> placements)
        {
            foreach (var placement in placements) 
                PlaceTile(placement);
        }

        public void PlaceTile(TileStack destinationStack, Tile tile)
        {
            var destinationTile = destinationStack.Peek();
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

            tile.Place(cell, this);

            if (m_tilesDictionary.TryGetValue(cell, out var stack))
                stack.Push(tile);
            else
                m_tilesDictionary.Add(cell, new TileStack(tile));
        }

        public void RemoveTile(Tile tile)
        {
            var cell = m_grid.WorldToCell(tile.transform.position);
            if (!m_tilesDictionary.TryGetValue(cell, out var stack)) return;
            tile = stack.Pop();
            Destroy(tile.gameObject);
        }

        private void Reset() => m_grid = GetComponent<Grid>();

        public void SelectTile(Vector3 position)
        {
            var cell = m_grid.WorldToCell(position);
            SelectedTile = m_tilesDictionary.GetValueOrDefault(cell);
            OnTileSelected.Invoke(SelectedTile);
        }

        public IEnumerator<KeyValuePair<Vector3Int, TileStack>> GetEnumerator() => m_tilesDictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => m_tilesDictionary.Count;
        public bool ContainsKey(Vector3Int key) => m_tilesDictionary.ContainsKey(key);

        public bool TryGetValue(Vector3Int key, out TileStack value) => m_tilesDictionary.TryGetValue(key, out value);

        public TileStack this[Vector3Int key] => m_tilesDictionary[key];

        public IEnumerable<Vector3Int> Keys => m_tilesDictionary.Keys;
        public IEnumerable<TileStack> Values =>  m_tilesDictionary.Values;
    }
}