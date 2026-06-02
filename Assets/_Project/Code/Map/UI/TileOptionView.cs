using Windows.View;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project.Map.UI
{
    [RequireComponent(typeof(Button))]
    public abstract class TileOptionView : UIBehaviour, IWindowView<TileStack>
    {
        [SerializeField] private Button m_button = null;
        
        protected TileStack m_tileStack = null;
        
        protected override void Awake()
        {
            base.Awake();
            m_button.onClick.AddListener(OnClickCallback);
        }
        
        protected override void OnDestroy()
        {
            m_button.onClick.RemoveListener(OnClickCallback);
            base.OnDestroy();
        }

        protected abstract void OnClickCallback();

        protected virtual bool Validate(TileStack data) => true;

        public void Initialize(TileStack data)
        {
            if (!Validate(data))
            {
                Clear();
                return;
            }
            m_tileStack = data;
            gameObject.SetActive(true);
        }

        protected override void Reset()
        {
            base.Reset();
            m_button =  GetComponent<Button>();
        }

        public virtual void Clear()
        {
            m_tileStack = null;
            gameObject.SetActive(false);
        }
    }
}