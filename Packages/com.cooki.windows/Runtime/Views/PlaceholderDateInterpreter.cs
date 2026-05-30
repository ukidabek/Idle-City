using System;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;

namespace Windows.View
{
    [Serializable, Preserve]
    public class PlaceholderDateInterpreter : IDateInterpreter
    {
        public void Display(object data) { }

        public void Clear() {}
    }
}