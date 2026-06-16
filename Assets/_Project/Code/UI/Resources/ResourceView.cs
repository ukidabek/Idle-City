using Windows.View;
using Project.Resources;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project.UI.Resources
{
    public class ResourceView : UIBehaviour, IWindowView<Resource>
    {
        [SerializeField] private Image m_image = null;
        [SerializeField] private TMP_Text m_name = null;

        private Resource m_resource = null;
        
        public void Initialize(Resource data)
        {
            m_resource = data;
            m_resource.OnValueChanged += UpdateText;
            m_image.sprite = data.Image;
            UpdateText(m_resource.Value);
        }

        private void UpdateText(float obj)
        {
            if (m_name == null) return;
            m_name.text = obj.Abbreviate();
        }

        public void Clear()
        {
            if (m_resource == null) return;
            m_resource.OnValueChanged -= UpdateText;
        }
    }
}