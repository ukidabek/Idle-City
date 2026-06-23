using System.Collections.Generic;
using Code.Upgrades;
using Windows;
using UnityEngine;
using UnityEngine.Pool;

namespace Project.UI.Upgrades
{
    public class UpgradesWindow : Window
    {
        [SerializeField] private UpgradeCollection m_upgradeCollection = null;
        [SerializeField] private UpgradesRowView m_upgradesRowViewPrefab = null;
        [SerializeField] private UpgradeConnector m_connectorPrefab = null;

        private ObjectPool<UpgradesRowView> m_upgradesRowViewPool = null;
        private ObjectPool<UpgradeConnector> m_connectorPool = null;
        private readonly Dictionary<Upgrade, UpgradeView> m_upgradeViewMap = new Dictionary<Upgrade, UpgradeView>();

        protected override void Awake()
        {
            base.Awake();

            m_upgradesRowViewPool = new ObjectPool<UpgradesRowView>(
                () => Instantiate(m_upgradesRowViewPrefab, m_canvasHolder.transform, false),
                rowView => rowView.gameObject.SetActive(true),
                rowView => { rowView.Clear(); rowView.gameObject.SetActive(false); },
                rowView => Destroy(rowView.gameObject));

            m_connectorPool = new ObjectPool<UpgradeConnector>(
                () => Instantiate(m_connectorPrefab, m_canvasHolder.transform, false),
                connector => connector.gameObject.SetActive(true),
                connector => { connector.Clear(); connector.gameObject.SetActive(false); },
                connector => Destroy(connector.gameObject));

            var upgradesByRow = m_upgradeCollection as IReadOnlyDictionary<int, IReadOnlyList<Upgrade>>;
            foreach (var upgradeRow in upgradesByRow.Values)
            {
                var rowView = m_upgradesRowViewPool.Get();
                rowView.Initialize(upgradeRow);
                foreach (var upgradeView in rowView.UpgradeViews)
                    m_upgradeViewMap[upgradeView.Upgrade] = upgradeView;
            }
        }

        private void Start()
        {
            Canvas.ForceUpdateCanvases();

            var allUpgrades = m_upgradeCollection as IReadOnlyList<Upgrade>;
            foreach (var upgrade in allUpgrades)
            {
                foreach (var dependency in upgrade.Dependencies)
                {
                    if (!m_upgradeViewMap.TryGetValue(dependency.Upgrade, out var fromView)) continue;
                    if (!m_upgradeViewMap.TryGetValue(upgrade, out var toView)) continue;

                    var connector = m_connectorPool.Get();
                    connector.Connect(
                        fromView.GetComponent<RectTransform>(),
                        toView.GetComponent<RectTransform>());
                }
            }
        }
    }
}
