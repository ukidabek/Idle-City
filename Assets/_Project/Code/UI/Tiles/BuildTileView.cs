using System.Collections.Generic;
using Windows.View;
using Project.Map;
using Project.Map.Events;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Project.UI.Tiles
{
    public class BuildTileView : UIBehaviour, IWindowView<Structure>
    {
        [SerializeField] private TMP_Text m_name = null;
        [SerializeField] private TileEvent m_buildTileEvent = null;
        [SerializeField] private Image m_image = null;

        [SerializeField] private Transform m_costLayoutHandler = null;
        [SerializeField] private CostView m_costViewPrefab = null;
       
        private HashSet<CostView> m_activeCostViews = new HashSet<CostView>(3);
        private ObjectPool<CostView>  m_pool = null;
        private Structure m_structure;

        protected override void Awake()
        {
            base.Awake();
            InitializePool();
        }

        private void InitializePool()
        {
            if (m_pool != null) return;
            m_pool = new ObjectPool<CostView>(() => Instantiate(m_costViewPrefab, m_costLayoutHandler),
                view =>
                {
                    m_activeCostViews.Add(view);
                    view.gameObject.SetActive(true);
                },
                view =>
                {
                    view.gameObject.SetActive(false);
                    view.Clear();
                });
        }

        public void Build()
        {
            if (!m_structure.CanAfford) return;
            m_buildTileEvent.Invoke(m_structure.Tile);
        }

        public void Initialize(Structure data)
        {
            InitializePool();
            m_structure = data;
            m_name.text = data.Tile.name;
            m_image.sprite = data.Image;

            foreach (var costView in m_structure.Costs)
            {
                var instance = m_pool.Get();
                instance.Initialize(costView);
            }
        }

        public void Clear()
        {
            m_structure = null;
            m_name.text = string.Empty;
            m_activeCostViews.RemoveWhere(view =>
            {
                m_pool.Release(view);
                return true;
            });
        }
    }
}