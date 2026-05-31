using Windows.View;
using UnityEngine.EventSystems;

namespace Project.Map
{
    public abstract class TileOptionView : UIBehaviour, IWindowView<TileStack>
    {
        protected TileStack m_tileStack = null;
        
        protected abstract bool Validate(TileStack data);

        public void Initialize(TileStack data)
        {
            if (!Validate(data))
            {
                Clear();
                return;
            }
            InitializeInternal(data);
            gameObject.SetActive(true);
        }
        
        protected abstract void InitializeInternal(TileStack data);

        public virtual void Clear()
        {
            m_tileStack = null;
            gameObject.SetActive(false);
        }
    }
}