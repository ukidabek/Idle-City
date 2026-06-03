using Windows.View;
using Project.Map;
using Project.Map.Events;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.UI
{
    public class BuildTileView : UIBehaviour, IWindowView<Structure>
    {
        [SerializeField] private TMP_Text m_name = null;
        [SerializeField] private TileEvent m_buildTileEvent = null;
        private Structure m_structure;
        
        public void Build() => m_buildTileEvent.Invoke(m_structure.Tile);

        public void Initialize(Structure data)
        {
            m_structure = data;
            m_name.text = data.Tile.name;
        }

        public void Clear()
        {
            m_structure = null;
            m_name.text = string.Empty;
        }
    }
}