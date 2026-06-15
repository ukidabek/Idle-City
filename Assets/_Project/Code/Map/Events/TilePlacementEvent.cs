using UnityEngine;
using Utilities.General.Events.Core;

namespace Project.Map.Events
{
    [CreateAssetMenu(menuName = "Events/Map/TilePlacement Event", fileName = "TilePlacementEvent")]
    public class TilePlacementEvent : ParameterizedEvent<TilePlacement> { }
}
