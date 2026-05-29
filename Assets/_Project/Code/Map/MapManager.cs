using UnityEngine;

namespace Project.Map
{
    [RequireComponent(typeof(Grid))]
    public class MapManager : MonoBehaviour
    {
        [SerializeField] private Grid m_grid;

        private void Reset()
        {
            m_grid = GetComponent<Grid>();
        }

        private void OnValidate()
        {
            if(m_grid == null) return;
            m_grid.cellLayout = GridLayout.CellLayout.Rectangle;
            m_grid.cellSwizzle = GridLayout.CellSwizzle.XZY;
        }
    }
}