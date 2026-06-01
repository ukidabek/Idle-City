using UnityEngine;
using Utilities.General.Events.Core;

namespace Project.Map.Events
{
    [CreateAssetMenu(menuName = "Events/Map/TileStack Event", fileName = "TileStackEvent")]
    public class TileStackEvent : ParameterizedEvent<TileStack> { }
}