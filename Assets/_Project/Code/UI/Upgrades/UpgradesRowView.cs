using System.Collections.Generic;
using Code.Upgrades;
using Windows.View;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace Project.UI.Upgrades
{
    public class UpgradesRowView : UIBehaviour, IWindowView<IReadOnlyList<Upgrade>>
    {
        [SerializeField] private UpgradeView m_upgradeViewPrefab = null;
        [SerializeField] private Transform m_upgradeViewsParent = null;

        private ObjectPool<UpgradeView> m_upgradeViewPool = null;
        private readonly HashSet<UpgradeView> m_activeUpgradeViews = new HashSet<UpgradeView>(30);

        public IReadOnlyCollection<UpgradeView> UpgradeViews => m_activeUpgradeViews;

        protected override void Awake()
        {
            base.Awake();
            m_upgradeViewPool = new ObjectPool<UpgradeView>(
                () => Instantiate(m_upgradeViewPrefab, m_upgradeViewsParent, false),
                upgradeView => upgradeView.gameObject.SetActive(true),
                upgradeView => upgradeView.gameObject.SetActive(false),
                upgradeView => Destroy(upgradeView.gameObject));
        }

        public void Initialize(IReadOnlyList<Upgrade> upgrades)
        {
            foreach (var upgrade in upgrades)
            {
                var upgradeView = m_upgradeViewPool.Get();
                upgradeView.Initialize(upgrade);
                m_activeUpgradeViews.Add(upgradeView);
            }
        }

        public void Clear()
        {
            foreach (var upgradeView in m_activeUpgradeViews)
            {
                upgradeView.Clear();
                m_upgradeViewPool.Release(upgradeView);
            }
            m_activeUpgradeViews.Clear();
        }
    }
}
