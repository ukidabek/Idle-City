using UnityEngine;

namespace Project.Resources
{
    public class ResourcesInitializer : MonoBehaviour
    {
        [SerializeField] private float m_initialValue = 0;
        [SerializeField] private ResourceCollection m_collection;

        private void Awake()
        {
            foreach (var item in m_collection)
                item.Value = m_initialValue;
        }
    }
}