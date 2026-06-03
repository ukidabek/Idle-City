using Project.Map;
using Project.Map.Events;
using UnityEngine;

namespace Project.UI.Tiles
{
    public class DestroyTileOptionView : TileOptionView
    {
        [SerializeField] private TileEvent m_destroyTileEvent;
        
        protected override bool Validate(Tile data) => data != null && data.TryGetComponent(out Destroyable _);

        protected override void OnClickCallback() => m_destroyTileEvent?.Invoke(m_tile);
    }
}