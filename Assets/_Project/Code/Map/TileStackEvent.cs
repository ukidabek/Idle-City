using UnityEngine;
using Utilities.General.Events.Core;

namespace Project.Map
{
    [CreateAssetMenu(menuName = "Events/Map/TileStack Event", fileName = "TileStackEvent")]
    public class TileStackEvent : ParameterizedEvent<TileStack> { }
}