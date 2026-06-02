using System;
using System.Collections.Generic;
using System.Linq;
using Windows;
using cookie.Logging;
using UnityEngine;
using UnityEngine.Pool;

namespace Project.Map.UI
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
        
        private HashSet<IStructure> m_structures = new HashSet<IStructure>(30);
        private ObjectPool<BuildTileView> m_buildTileViewPools = null;
        private HashSet<BuildTileView> m_activeViews = new HashSet<BuildTileView>();
        private TileStack m_selectedStack = null;

        
        protected override void Awake()
        {
            base.Awake();
            var structures = m_database.GetTilesByCategory(m_structureCategory);
            foreach (var tile in structures)
            {
                if (!tile.TryGetComponent(out IStructure structure))
                {
                    this.Log($"{nameof(Tile)} is missing {nameof(IStructure)} component! Ignoring", LogType.Warning, tile, true);
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

            IEnumerable<IStructure> availableStructures = Array.Empty<IStructure>();

            if (tile.TryGetComponent(out IGround _))
                availableStructures = m_structures.Where(structure => structure.TileRequirements.Contains(tileID));

            if (tile.TryGetComponent(out IDeposit _))
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