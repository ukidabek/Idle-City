using System;
using UnityEngine;

namespace Project.Map
{
    [CreateAssetMenu(menuName = "Map/Tiles/ClientData", fileName = "ClientData")]
    public class ProducerData : TileData
    {
        [field: SerializeField] public ClientResourceInfo[] ResourcesToConsume { get; private set; } =  Array.Empty<ClientResourceInfo>();
        [field: SerializeField] public ClientResourceInfo[] ResourcesToProduce { get; private set; } =  Array.Empty<ClientResourceInfo>();
    }
}