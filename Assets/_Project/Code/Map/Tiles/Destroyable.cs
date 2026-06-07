using UnityEngine;

namespace Project.Map
{
    public class Destroyable : TileComponent
    {
        [SerializeField] private Structure m_structure = null;

        private void OnDestroy()
        {
            if (m_structure == null) return;
            var costReturnMultiplayer = m_structure.CostReturnMultiplayer;
            foreach (var cost in m_structure.Costs)
                cost.Resource.Value += cost.Amount * costReturnMultiplayer;
        }

        protected override void Reset()
        {
            base.Reset();
            m_structure = GetComponent<Structure>();
        }
    }
}