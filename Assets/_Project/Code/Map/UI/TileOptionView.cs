using Windows.View;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project.Map.UI
{
    [RequireComponent(typeof(Button))]
    public  class TileOptionView : UIBehaviour, IWindowView<TileStack>
    {
        protected TileStack m_tileStack = null;
        [SerializeField] private Button m_button = null;

        public UnityEvent<Tile> OnClick = new UnityEvent<Tile>();
        
        protected override void Awake()
        {
            base.Awake();
            m_button.onClick.AddListener(OnClickCallback);
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_button.onClick.RemoveListener(OnClickCallback);
        }

        protected virtual void OnClickCallback() => OnClick.Invoke(m_tileStack.Peek());

        protected virtual bool Validate(TileStack data) => true;

        public void Initialize(TileStack data)
        {
            if (!Validate(data))
            {
                Clear();
                return;
            }
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