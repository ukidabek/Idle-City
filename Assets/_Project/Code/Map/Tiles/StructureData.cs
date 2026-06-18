using UnityEngine;

namespace Project.Map
{
    [CreateAssetMenu(menuName = "Map/Tiles/StructureData", fileName = "StructureData")]
    public class StructureData : TileData
    {
        [field: SerializeField] public Sprite Image { get; private set; } = null;
        [field: SerializeField] public TileID[] TileRequirements { get; private set; } = null;
        [SerializeField] private Cost[] m_costs = null;
        public Cost[] Costs => m_costs;
        [field: SerializeField, Min(0f)] public float CostReturnMultiplayer { get; private set; } = .5f;
        [field: SerializeField, Range(0f, 3f)] public float Exponent { get; private set; } = 1f;
        [field: SerializeField, Range(0f, 3f)] public float Multiplier { get; private set; } = 1f;
        
        public float CalculateCost(float baseValue, int currentAmount) =>
            baseValue * Mathf.Pow(currentAmount, Exponent) * Mathf.Pow(Multiplier, currentAmount);
    }
}