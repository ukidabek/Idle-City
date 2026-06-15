using System.Collections.Generic;
using Utilities.General.Events.Core;

namespace Project.Map.Events
{
    public class TilePlacementsEventListener : EventListenerBehaviour<TilePlacementsEvent, IEnumerable<TilePlacement>> { }
}
