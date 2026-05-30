using UnityEngine;
using UnityEngine.EventSystems;

namespace Cooki.FadeManager
{
    public class FadeDisplay : UIBehaviour
    {
        [SerializeField] private CanvasGroup m_canvasGroup;
        [SerializeField] private Fade m_fade = null;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_fade.OnAlphaChanged.AddListener(UpdateAlfa);
        }

        private void UpdateAlfa(float alfa) => m_canvasGroup.alpha = alfa;

        protected override void OnDisable()
        {
            base.OnDisable();
            m_fade.OnAlphaChanged.RemoveListener(UpdateAlfa);
        }
    }
}