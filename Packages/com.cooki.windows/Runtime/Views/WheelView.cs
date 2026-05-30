using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Windows.View
{
    public abstract class WheelView<DataType, ItemType> : UIBehaviour, IWindowView<IEnumerable<DataType>>
        where ItemType : ObjectView<DataType>
    {
#pragma warning disable CS0414
        [SerializeField] private Direction m_direction = Direction.Clockwise;
        [SerializeField, Range(0, 36)] private int m_segmentsCount = 10;
        [SerializeField, Range(-360f, 360f)] private float m_angleOffset = 0f;
        [SerializeField] private float m_radius = 0.5f;
        [SerializeField] protected ItemType m_prefab = null;
#pragma warning restore CS0414
        
        [Space, SerializeField, Range(0f, 1f)] protected float m_selectionThreshold = .9f;
        [SerializeField] protected List<ItemType> m_items = new List<ItemType>(30);
        
        [Space]
        public UnityEvent<ItemType> OnItemSelected = new UnityEvent<ItemType>();
        public UnityEvent<ItemType> OnItemClick = new UnityEvent<ItemType>();
        
        public virtual void Initialize(IEnumerable<DataType> data)
        {
            using var dataEnumerator = data.GetEnumerator();
            if (m_items.Count == 0)
            {
                m_segmentsCount = data.Count();
                foreach (var position in GetPositions())
                {
                    if (!dataEnumerator.MoveNext()) continue;
                    var instance = Instantiate(m_prefab, transform);
                    instance.Initialize(dataEnumerator.Current);
                    instance.transform.localPosition = position;
                    m_items.Add(instance);
                }

                return;
            }
            
            foreach (var item in m_items)
            {
                if (!dataEnumerator.MoveNext()) continue;
                item.gameObject.SetActive(true);
                item.Initialize(dataEnumerator.Current);
                item.OnClick.AddListener(OnItemClicked);
            }
        }

        public IEnumerable<Vector3> GetPositions()
        {
            var anglePerSegment = 360f / m_segmentsCount;
            var multiplayer = m_direction == Direction.CounterClockwise ? 1 : -1;
            
            for (var i = 0; i < m_segmentsCount; i++)
            {
                var angle = (m_angleOffset + multiplayer * anglePerSegment * i) * Mathf.Deg2Rad;
                yield return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * m_radius;
            }
        }
        
        private void OnItemClicked()
        {
            var currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
            var selectedItem = m_items.FirstOrDefault(item => currentSelectedGameObject == item.gameObject);
            if(selectedItem is null) return;
            OnItemClick.Invoke(selectedItem);
        }

        public virtual void Clear()
        {
            foreach (var item in m_items)
            {
                item.gameObject.SetActive(false);
                item.Clear();
            }
        }

        public void Select(Vector2 input)
        {
            input.Normalize();
            foreach (var item in m_items)
            {
                var position = item.transform.localPosition;
                position.Normalize();
                var dot = Vector3.Dot(position, input);
                if (dot < m_selectionThreshold) continue;
                item.Select();
                OnItemSelected.Invoke(item);
            }
        }
    }
}