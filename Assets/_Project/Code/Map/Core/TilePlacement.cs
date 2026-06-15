using UnityEngine;

namespace Project.Map
{
    public readonly struct TilePlacement
    {
        public readonly Vector3Int Cell;
        public readonly Tile Tile;
        public readonly bool PlaceOnSelected;

        public TilePlacement(Vector3Int cell, Tile tile)
        {
            Cell = cell;
            Tile = tile;
            PlaceOnSelected = false;
        }

        public TilePlacement(Tile tile)
        {
            Tile = tile;
            Cell = Vector3Int.zero;
            PlaceOnSelected = true;
        }
    }
}
