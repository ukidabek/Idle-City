using System.Collections.Generic;
using UnityEngine;
using Utilities.General.Events.Core;

namespace Project.Map.Events
{
    [CreateAssetMenu(menuName = "Events/Map/TilePlacements Event", fileName = "TilePlacementsEvent")]
    public class TilePlacementsEvent : ParameterizedEvent<IEnumerable<TilePlacement>> { }
}
