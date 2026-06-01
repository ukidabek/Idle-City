using System.Collections.Generic;

namespace Project.Map
{
    public interface IGround : ITielComponent
    {
        IReadOnlyList<TileID> AvailableDeposits { get; }
    }
}