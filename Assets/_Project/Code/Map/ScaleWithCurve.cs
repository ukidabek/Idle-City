using UnityEngine;

namespace Project.Map
{
    public class ScaleWithCurve : MonoBehaviour
    {
        [SerializeField] private AnimationCurve m_scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField, Min(0)] private Vector3 m_minScale = Vector3.one;
        [SerializeField, Min(0)] private Vector3 m_maxScale = Vector3.one * 2f;
        [SerializeField, Min(float.Epsilon)] private float m_duration = 1f;

        private float m_time = 0f;
        private bool m_forward = true;

        private void Update()
        {
            m_time += (m_forward ? 1f : -1f) * Time.deltaTime / m_duration;

            switch (m_time)
            {
                case >= 1f:
                    m_time = 1f;
                    m_forward = false;
                    break;
                case <= 0f:
                    m_time = 0f;
                    m_forward = true;
                    break;
            }

            var curveValue = m_scaleCurve.Evaluate(m_time);
            transform.localScale = Vector3.LerpUnclamped(m_minScale, m_maxScale, curveValue);
        }
    }
}