using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.Map
{
    [CreateAssetMenu(menuName = "Map/Tiles/StructureData", fileName = "StructureData")]
    public class StructureData : TileData
    {
        [field: SerializeField] public Sprite Image { get; private set; } = null;
        [field: SerializeField] public TileID[] TileRequirements { get; private set; } = null;
        [SerializeField] private Cost[] m_costs = null;
        public IReadOnlyList<Cost> Costs => m_costs;
        [field: SerializeField, Min(0f)] public float CostReturnMultiplayer { get; private set; } = .5f;
        [field: SerializeField, Range(0f, 3f)] public float Exponent { get; private set; } = 1f;
        [field: SerializeField, Range(0f, 3f)] public float Multiplier { get; private set; } = 1f;

        public bool CanAfford(float multiplayer = 1f) => m_costs.All(cost => cost.CanAfford(multiplayer));

        public float CalculateCost(float baseValue, int currentAmount) =>
            baseValue * Mathf.Pow(currentAmount, Exponent) * Mathf.Pow(Multiplier, currentAmount);

        public float[] SampleCostCurve(int sampleCount, float baseValue)
        {
            var samples = new float[sampleCount];
            for (var n = 0; n < sampleCount; n++)
                samples[n] = CalculateCost(baseValue, n);
            return samples;
        }
    }
}