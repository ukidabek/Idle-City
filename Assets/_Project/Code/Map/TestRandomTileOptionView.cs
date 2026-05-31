using UnityEngine;

namespace Project.Map
{
    public class TestRandomTileOptionView : TileOptionView
    {
        protected override bool Validate(TileStack data)
        {
            return Random.value > 0.5f;
        }

        protected override void InitializeInternal(TileStack data)
        {
        }
    }
}