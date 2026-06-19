using Windows;
using Project.Map;
using UnityEngine;

namespace Project.UI.Tiles
{
    public class TileInspectorWindow : Window
    {
        [SerializeField] private TileView m_view = null;

        public void OnTileStackSelected(TileStack tileStack)
        {
            if (tileStack == null)
            {
                Hide();
                m_view.Clear();
                return;
            }
            var tile = tileStack.Peek();
            m_view.Initialize(tile);
            Show();
        }
    }
}