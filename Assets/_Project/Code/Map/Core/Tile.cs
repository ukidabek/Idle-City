using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Utilities.General;

namespace Project.Map
{
    [SelectionBase]
    public class Tile : MonoBehaviour
    {
        [FormerlySerializedAs("m_key")] 
        [SerializeField] private TileID m_id;
        public TileID ID => m_id;
        
        [SerializeField] private TileCategory m_tileCategory;
        public TileCategory TileCategory => m_tileCategory;

        public IReadOnlyDictionary<Vector3Int, TileStack> Map { get; private set; }

        [field: SerializeField, ReadOnly] public Vector3Int Cell { get; private set; }

        public UnityEvent OnTilePlaced = new UnityEvent();
        
        public void Place(Vector3Int cell, IReadOnlyDictionary<Vector3Int, TileStack> map)
        {
            Cell = cell;
            Map = map;
            OnTilePlaced.Invoke();
        }
    }
}
