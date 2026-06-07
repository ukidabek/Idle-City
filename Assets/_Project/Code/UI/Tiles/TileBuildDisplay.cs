using System;
using System.Collections.Generic;
using System.Linq;
using Windows;
using cookie.Logging;
using Project.Map;
using UnityEngine;
using UnityEngine.Pool;

namespace Project.UI.Tiles
{
    public class TileBuildDisplay : Window, ILogEnabled
    {
        public Color Color { get; } = Color.greenYellow;
        [field: SerializeField] public LogMode Mode { get; private set; } = LogMode.All;

        [SerializeField] private TileDatabase m_database = null;
        [SerializeField] private TileCategory m_structureCategory = null;
        [Space]
        [SerializeField] private Transform m_viewsParent = null;
        [SerializeField] private BuildTileView m_buildTileViewPrefab = null;
        
        private HashSet<Structure> m_structures = new HashSet<Structure>(30);
        private ObjectPool<BuildTileView> m_buildTileViewPools = null;
        private HashSet<BuildTileView> m_activeViews = new HashSet<BuildTileView>();
        private TileStack m_selectedStack = null;

        
        protected override void Awake()
        {
            base.Awake();
            var structures = m_database.GetTilesByCategory(m_structureCategory);
            foreach (var tile in structures)
            {
                if (!tile.TryGetComponent(out Structure structure))
                {
                    this.Log($"{nameof(Tile)} is missing {nameof(Structure)} component! Ignoring", LogType.Warning, tile, true);
                    continue;
                }

                m_structures.Add(structure);
            }

            m_buildTileViewPools = new ObjectPool<BuildTileView>(
                () => Instantiate(m_buildTileViewPrefab, m_viewsParent),
                view => m_activeViews.Add(view),
                view =>
                {
                    view.Clear();
                    view.gameObject.SetActive(false);
                });
        }

        public void OnTileStackSelected(TileStack tileStack)
        {
            if (tileStack == null)
            {
                if (m_selectedStack == null) return;
                
                m_selectedStack.StackChanged -= UpdateButtons;
                m_selectedStack = null;
                Hide();

                return;
            }

            m_selectedStack = tileStack;
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            ReleaseAllActiveViews();
            
            m_selectedStack.StackChanged += UpdateButtons;
            
            var tile = m_selectedStack.Peek();
            var tileID = tile.ID;

            IEnumerable<Structure> availableStructures = Array.Empty<Structure>();

            if (tile.TryGetComponent(out Ground _))
                availableStructures = m_structures.Where(structure => structure.TileRequirements.Contains(tileID));

            if (tile.TryGetComponent(out Deposit _))
                availableStructures = m_structures.Where(structure => structure.TileRequirements.Contains(tileID));

            if (!availableStructures.Any())
            {
                Hide();
                return;
            }

            foreach (var structure in availableStructures)
            {
                var view = m_buildTileViewPools.Get();
                view.Initialize(structure);
                view.gameObject.SetActive(true);
            }

            Show();
        }

        private void ReleaseAllActiveViews()
        {
            foreach (var view in m_activeViews) 
                m_buildTileViewPools.Release(view);
            m_activeViews.Clear();
        }
    }
}