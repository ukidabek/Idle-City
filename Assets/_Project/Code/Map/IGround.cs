using System.Collections.Generic;

namespace Project.Map
{
    public interface IGround
    {
        IReadOnlyList<TileID> AvailableDeposits { get; }
    }
}