using Windows;
using Project.Resources;
using UnityEngine;
using UnityEngine.Pool;

namespace Project.UI.Resources
{
    public class ResourceWindow : Window
    {
        [SerializeField] protected ResourceCollection m_resources;
        [SerializeField] protected ResourceView m_resourceViewPrefab;
        
        protected ObjectPool<ResourceView> m_resourceViewPool= null;

        protected override void Awake()
        {
            base.Awake();
            
            m_resourceViewPool = new ObjectPool<ResourceView>(() => Instantiate(m_resourceViewPrefab, m_canvasHolder.transform, false),
                view => view.gameObject.SetActive(true),
                view => view.gameObject.SetActive(false),
                view => Destroy(view.gameObject));

            foreach (var resource in m_resources)
            {
                var instance = m_resourceViewPool.Get();
                instance.Initialize(resource);
            }
        }
    }
}