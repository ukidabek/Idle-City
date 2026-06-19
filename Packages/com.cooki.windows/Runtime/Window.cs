using System.Collections;
using UnityEngine;

namespace Windows
{
    [SelectionBase]
    public abstract class Window : MonoBehaviour
    {
        [SerializeField] protected GameObject m_canvasHolder = null;
        [SerializeField] private bool m_isOpen = false;

        protected virtual void Awake() => m_canvasHolder = m_canvasHolder == null ? gameObject : m_canvasHolder;

        protected virtual IEnumerator Start()
        {
            switch (m_isOpen)
            {
                case true:
                    Show();
                    break;
                case false:
                    Hide();
                    break;
            }

            yield return null;
        }

        public virtual void Show() => m_canvasHolder.SetActive(true);

        public virtual void Hide() => m_canvasHolder.SetActive(false);

        public virtual void Clear()
        {
        }
    }
}