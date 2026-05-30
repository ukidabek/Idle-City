using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Windows.View
{
    public class SpriteView : UIBehaviour, IWindowView<Sprite>
    {
        [SerializeField] protected Image m_image = null;

        public void Initialize(Sprite data) => m_image.sprite = data;

        public void Clear() => m_image.sprite = null;
    }
}