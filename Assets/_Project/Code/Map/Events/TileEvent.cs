using UnityEngine;
using Utilities.General.Events.Core;

namespace Project.Map.Events
{
    [CreateAssetMenu(menuName = "Events/Map/Tile Event", fileName = "TileEvent")]
    public class TileEvent : ParameterizedEvent<Tile> { }
}