namespace Project.Map.UI
{
    public class BuildOnTileOptionView : TileOptionView
    {
        protected override bool Validate(TileStack data)
        {
            var tile = data.Peek();
            return tile.TryGetComponent(out IGround _) || tile.TryGetComponent(out IDeposit _);
        }
    }
}