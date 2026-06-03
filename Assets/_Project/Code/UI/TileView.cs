using System;
using System.Collections.Generic;
using System.Linq;
using Windows.View;
using Project.Map;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.UI
{
    public class TileView : UIBehaviour, IWindowView<Tile>
    {
        [SerializeField] private TileComponentView[] m_views = null;

        private Dictionary<Type, TileComponentView> m_viewsDictionary = null;
        
        [SerializeField] private TMP_Text m_text = null;
        
        protected override void Awake()
        {
            base.Awake();
            m_viewsDictionary = m_views
                .Where(view => view.gameObject.activeSelf)
                .ToDictionary(view => view.HandledType, view => view);
        }
        
        public void Initialize(Tile data)
        {
            m_text.text = data.ID.name;
            foreach (var component in data.GetComponents<ITielComponent>())
            {
                if(!m_viewsDictionary.TryGetValue(component.GetType(), out var view)) continue;
                view.Initialize(component);
            }
        }

        public void Clear()
        {
            foreach (var view in m_views)
                view.Clear();
        }
    }
}