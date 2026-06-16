using System.Collections.Generic;
using UnityEngine;

namespace Project.Map
{
    public class Structure : TileComponent, IDataTileComponent<StructureData>
    {
        private static Dictionary<TileID, HashSet<Structure>> m_dictionary = new Dictionary<TileID, HashSet<Structure>>(30);

        [SerializeField] private StructureData m_data;
        public StructureData Data => m_data;
        public Sprite Image => Data.Image;
        public IReadOnlyList<TileID> TileRequirements => Data.TileRequirements;
        public IReadOnlyList<Cost> Costs => Data.Costs;
        public bool CanAfford => Data.CanAfford();
        public float CostReturnMultiplayer => Data.CostReturnMultiplayer;

        public void ConsumeResources()
        {
            if (!m_dictionary.TryGetValue(Tile.ID, out var set)) return;
            var multiplayer = 0;
            foreach (var cost in Costs)
                cost.Resource.Value -= cost.Amount * multiplayer;
        }

        private void OnEnable()
        {
            if (m_dictionary.TryGetValue(Tile.ID, out var set))
            {
                set.Add(this);
                return;
            }

            set = new HashSet<Structure>(100);
            m_dictionary.Add(Tile.ID, set);
        }

        private void OnDisable()
        {
            if (!m_dictionary.TryGetValue(Tile.ID, out var set)) return;
            set.Remove(this);
        }
    }
}