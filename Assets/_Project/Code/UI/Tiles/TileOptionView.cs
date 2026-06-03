using Windows.View;
using Project.Map;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project.UI.Tiles
{
    [RequireComponent(typeof(Button))]
    public abstract class TileOptionView : UIBehaviour, IWindowView<Tile>
    {
        [SerializeField] private Button m_button = null;
        
        protected Tile m_tile = null;
        
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

        protected virtual bool Validate(Tile data) => true;

        public void Initialize(Tile data)
        {
            if (!Validate(data))
            {
                Clear();
                return;
            }
            m_tile = data;
            gameObject.SetActive(true);
        }

        protected override void Reset()
        {
            base.Reset();
            m_button =  GetComponent<Button>();
        }

        public virtual void Clear()
        {
            m_tile = null;
            gameObject.SetActive(false);
        }
    }
}