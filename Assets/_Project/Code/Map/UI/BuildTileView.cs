using Windows.View;
using Project.Map.Events;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.Map.UI
{
    public class BuildTileView : UIBehaviour, IWindowView<IStructure>
    {
        [SerializeField] private TMP_Text m_name = null;
        [SerializeField] private TileEvent m_buildTileEvent = null;
        private IStructure m_structure;
        
        public void Build() => m_buildTileEvent.Invoke(m_structure.Tile);

        public void Initialize(IStructure data)
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