using Code.Generator;
using UnityEngine;

namespace Project.Map
{
    [CreateAssetMenu(menuName = "Map/Tiles/DepositData", fileName = "DepositData")]
    public class DepositData : TileData
    {
        [field: SerializeField, Randomize] public Vector2Int NoiseOffset { get; private set; } = Vector2Int.zero;
        [field: SerializeField, Range(0f, 1f)] public float MinimalWaterDistance { get; private set; } = .5f;
        [field: SerializeField, Range(0f, 1f)] public float SpawnChance { get; private set; } = .5f;
    }
}