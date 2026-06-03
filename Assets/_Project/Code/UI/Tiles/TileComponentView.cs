using System;
using Windows.View;
using Project.Map;
using UnityEngine.EventSystems;

namespace Project.UI.Tiles
{
    public abstract class TileComponentView : UIBehaviour, IWindowView<ITielComponent>
    {
        public abstract Type HandledType { get; }

        public virtual void Initialize(ITielComponent data) => gameObject.SetActive(true);

        public virtual void Clear() => gameObject.SetActive(false);
    }
}