using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Cooki.FadeManager
{
    [CreateAssetMenu(menuName = "Utilities/Fade", fileName = "Fade")]
    public class Fade : ScriptableObject
    {
        [field: SerializeField] public FadeStatus Status { get; private set; } = FadeStatus.In;
        
        [SerializeField] private float m_alpha = 0f;
        public float Alpha
        {
            get => m_alpha;
            set
            {
                m_alpha = value;
                OnAlphaChanged.Invoke(m_alpha);
            }
        }

        [field: SerializeField] public float FadeInDuration { get; private set; } = 3f;
        [field: SerializeField] public float FadeOutDuration { get; private set; } = 3f;

        public UnityEvent<float> OnAlphaChanged { get; private set; } = new UnityEvent<float>();
        
        internal event Action<Fade> OnFadeIn = null;
        internal event Action<Fade> OnFadeOut = null;
        
        private float m_timer = 0f;
        private float m_targetAlpha = 1f;
        
        internal IEnumerator FadeIn(float deltaTime, float timeScale)
        {
            if (Alpha >= m_targetAlpha)
            {
                Alpha = m_targetAlpha;
                yield break;
            }
            
            m_timer += deltaTime * timeScale;
            if (m_timer >= FadeInDuration)
            {
                Alpha = 1f;
                yield break;
            }
            
            Alpha = Mathf.InverseLerp(0, FadeInDuration, m_timer);
            Status = FadeStatus.In;
            yield return null;
        }

        internal IEnumerator FadeOut(float deltaTime, float timeScale)
        {
            if (Alpha <= m_targetAlpha)
            {
                Alpha = m_targetAlpha;
                yield break;
            }
            
            m_timer += deltaTime * timeScale;
            if (m_timer >= FadeOutDuration)
            {
                Alpha = 0f;
                yield break;
            }

            Status = FadeStatus.Out;
            Alpha = 1f - Mathf.InverseLerp(0, FadeOutDuration, m_timer);
            yield return null;
        }

        public void FadeIn(float alpha = 1f)
        {
            m_targetAlpha = alpha;
            OnFadeIn?.Invoke(this);
        }

        public void FadeOut(float alpha = 0f)
        {
            m_targetAlpha = alpha;
            OnFadeOut?.Invoke(this);
        }

        internal Fade InitializeTimer()
        {
            var t = 0f;
            switch (Status)
            {
                case FadeStatus.In:
                    if (m_timer >= FadeInDuration)
                    {
                        m_timer = 0f;
                        break;
                    }
                    t = Mathf.InverseLerp(0f, FadeInDuration, m_timer);
                    m_timer = Mathf.Lerp(0f, FadeOutDuration, t);
                    break;
                case FadeStatus.Out:
                    if (m_timer >= FadeOutDuration)
                    {
                        m_timer = 0f;
                        break;
                    }
                    t = Mathf.InverseLerp(0f, FadeOutDuration, m_timer);
                    m_timer = Mathf.Lerp(0f, FadeInDuration, t);
                    break;
            }

            return this;
        }
    }
}