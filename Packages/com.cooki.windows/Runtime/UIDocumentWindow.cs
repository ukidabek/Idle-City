using UnityEngine;
using UnityEngine.UIElements;

namespace Windows
{
    [RequireComponent(typeof(UIDocument))]
    public abstract class UIDocumentWindow : Window
    {
        [SerializeField] protected UIDocument m_uiDocument = null;

        protected void Reset() => m_uiDocument = GetComponent<UIDocument>();
    }
}
