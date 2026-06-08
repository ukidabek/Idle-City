using UnityEngine;

namespace Project.Map
{
    public class Deposit : TileComponent, IDataTileComponent<DepositData>
    {
        [field: SerializeField] public DepositData Data { get; private set; }
    }
}