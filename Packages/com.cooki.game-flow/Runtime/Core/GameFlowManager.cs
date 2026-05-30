using UnityEngine;

namespace Cooki.Flow
{
    [RequireComponent(typeof(FlowManager))]
    public class GameFlowManager : MonoBehaviour
    {
        public static FlowManager Instance { get; private set; }

        [SerializeField] private FlowManager m_flowManager = null;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = m_flowManager;
        }

        private void Reset() => m_flowManager = GetComponent<FlowManager>();
    }
}