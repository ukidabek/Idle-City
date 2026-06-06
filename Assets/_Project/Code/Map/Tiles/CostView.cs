using Windows.View;
using Project.UI.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.Map
{
    public class CostView : UIBehaviour, IWindowView<Cost>
    {
        [SerializeField] private ResourceView m_resourceView = null;
        [SerializeField] private TMP_Text m_text = null;
        [SerializeField] private string m_format = "{0:f0}";
        
        public void Initialize(Cost data)
        {
            m_text.text = string.Format(m_format, data.Amount);
            m_resourceView.Initialize(data.Resource);
        }

        public void Clear()
        { 
            m_text.text = string.Empty;
            m_resourceView.Clear();
        }
    }
}