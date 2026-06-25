using Code.Upgrades;
using TMPro;
using Windows.View;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project.UI.Upgrades
{
    public class UpgradeView : UIBehaviour, IWindowView<Upgrade>
    {
        [SerializeField] private TMP_Text m_upgradeName = null;
        [SerializeField] private TMP_Text m_upgradeLevel = null;
        [SerializeField] private Button m_levelUpButton = null;

        private Upgrade m_upgrade = null;

        public Upgrade Upgrade => m_upgrade;
        public UnityEvent OnLevelChanged = new UnityEvent();

        public void Initialize(Upgrade upgrade)
        {
            m_upgrade = upgrade;
            m_upgradeName.text = upgrade.name;
            m_levelUpButton.onClick.AddListener(OnLevelUpClicked);
            m_upgrade.OnLevelUp += OnLevelChanged.Invoke;
            OnLevelChanged.AddListener(Refresh);
            Refresh();
        }

        public void Clear()
        {
            if (m_upgrade == null) return;
            m_levelUpButton.onClick.RemoveListener(OnLevelUpClicked);
            m_upgrade.OnLevelUp -= OnLevelChanged.Invoke;
            OnLevelChanged.RemoveListener(Refresh);
            m_upgrade = null;
        }

        private void OnLevelUpClicked() => m_upgrade.LevelUp();

        public void Refresh()
        {
            m_upgradeLevel.text = $"{m_upgrade.CurrentLevel}/{m_upgrade.Count}";
            m_levelUpButton.interactable = m_upgrade.IsUnlocked;
        }
    }
}
