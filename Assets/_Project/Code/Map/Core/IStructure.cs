using System.Collections.Generic;

namespace Project.Map
{
    public interface IStructure : ITielComponent
    {
        public IReadOnlyList<TileID> TileRequirements { get; }
    }
}