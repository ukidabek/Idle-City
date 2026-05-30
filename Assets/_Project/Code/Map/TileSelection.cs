using UnityEngine;

namespace Project.Map
{
    public class TileSelection : MonoBehaviour
    {
        [SerializeField] private GameObject m_selector = null;
        [SerializeField, Min(0)] private Vector3 m_offset = Vector3.zero;

        private void Awake()
        {
            m_selector.SetActive(false);
        }

        public void SelectTile(Tile tile)
        {
            if (tile == null)
            {
                m_selector.gameObject.SetActive(false);
                return;
            }
            
            m_selector.gameObject.SetActive(true);
            m_selector.transform.position = tile.transform.position +  m_offset;
        }
    }
}