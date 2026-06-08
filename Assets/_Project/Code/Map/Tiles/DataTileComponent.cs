namespace Project.Map
{
    public interface IDataTileComponent<out DataType>  where DataType : TileData
    {
        DataType Data { get; }
    }
}