using UnityEngine;

namespace Project.Map
{
    [CreateAssetMenu(menuName = "Map/Tiles/StructureData", fileName = "StructureData")]
    public class StructureData : ScriptableObject
    {
        [field: SerializeField] public Sprite Image { get; private set; } = null;
        [field: SerializeField] public TileID[] TileRequirements  { get; private set; } = null;
    }
}