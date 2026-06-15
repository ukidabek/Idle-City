using UnityEngine;

namespace Project.Map
{
    public struct TilePlacement
    {
        public Vector3Int Cell;
        public Tile Tile;

        public TilePlacement(Vector3Int cell, Tile tile)
        {
            Cell = cell;
            Tile = tile;
        }
    }
}
