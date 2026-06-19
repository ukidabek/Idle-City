using System;
using System.Collections.Generic;
using System.Linq;
using cookie.Logging;
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
        public Span<Cost> Costs => GetCosts(true);
        public bool CanAfford => GetCosts(true).All(cost => cost.Resource.Value >= cost.Amount);

        private Cost[] GetCosts(bool extrapolate)
        {
            var count = 0;
            if (m_dictionary.TryGetValue(Tile.ID, out var set))
                count = set.Count + (extrapolate ? 1 : 0);
            else
                count = 1;
            
            var costs = Data.Costs;
            var costsCount = costs.Length;
            var currentCosts = new Cost[costsCount];
            for (var i = 0; i < costsCount; i++)
            {
                currentCosts[i] = new Cost()
                {
                    Resource = costs[i].Resource,
                    Amount = Mathf.Ceil(Data.CalculateCost(costs[i].Amount, count)),
                };
            }

            return currentCosts;
        }
        
        public float CostReturnMultiplayer => Data.CostReturnMultiplayer;

        public void ConsumeResources()
        {
            foreach (var cost in GetCosts(false))
            {
                var resource = cost.Resource;
                var valueBefore = resource.Value;
                resource.Value -= cost.Amount;
                var delta = valueBefore - resource.Value;
                this.Log($"[{resource.name}] before: {valueBefore} | cost: {cost.Amount} | delta: {delta}");
            }
        }

        private void OnEnable()
        {
            if (!m_dictionary.TryGetValue(Tile.ID, out var set))
            {
                set = new HashSet<Structure>(100);
                m_dictionary.Add(Tile.ID, set);
            }

            set.Add(this);
        }

        private void OnDisable()
        {
            if (!m_dictionary.TryGetValue(Tile.ID, out var set)) return;
            set.Remove(this);
        }
    }
}