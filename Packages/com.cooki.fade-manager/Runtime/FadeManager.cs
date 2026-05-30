using System.Collections.Generic;
using UnityEngine;

namespace Cooki.FadeManager
{
    public class FadeManager : MonoBehaviour
    {
        [SerializeField] private Fade[] m_fades = null;

        private readonly HashSet<Fade> m_fadesToFadeIn = new HashSet<Fade>(30);
        private readonly HashSet<Fade> m_fadesToFadeOut = new HashSet<Fade>(30);
        
        private void OnEnable()
        {
            foreach (var fade in m_fades)
            {
                fade.OnFadeIn += FadeIn;
                fade.OnFadeOut += FadeOut;
            }
        }

        private void OnDestroy()
        {
            foreach (var fade in m_fades)
            {
                fade.OnFadeIn -= FadeIn;
                fade.OnFadeOut -= FadeOut;
            }
        }

        private void FadeIn(Fade fade)
        {
            m_fadesToFadeIn.Add(fade.InitializeTimer());
            m_fadesToFadeOut.Remove(fade);
        }

        private void FadeOut(Fade fade)
        {
            m_fadesToFadeOut.Add(fade.InitializeTimer());
            m_fadesToFadeIn.Remove(fade);
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime, timeScale = Time.timeScale;
            m_fadesToFadeIn.RemoveWhere(fade => !fade.FadeIn(deltaTime, timeScale).MoveNext());
            m_fadesToFadeOut.RemoveWhere(fade => !fade.FadeOut(deltaTime, timeScale).MoveNext());
        }

        public void ClearAllFades()
        {
            m_fadesToFadeIn.Clear();
            m_fadesToFadeOut.Clear();
            
            foreach (var fade in m_fades)
                FadeOut(fade);
        }
    }
}