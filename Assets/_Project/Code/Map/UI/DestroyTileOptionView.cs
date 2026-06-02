namespace Project.Map.UI
{
    public class DestroyTileOptionView : TileOptionView
    {
        protected override bool Validate(TileStack data)
        {
            var tile = data.Peek();
            return tile != null && tile.TryGetComponent(out Structure _);
        }

        protected override void OnClickCallback()
        {
            if (m_tileStack.Count == 0) return;
            var tile = m_tileStack.Pop();
            Destroy(tile.gameObject);
        }
    }
}