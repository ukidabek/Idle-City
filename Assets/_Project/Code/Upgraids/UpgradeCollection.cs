using System.Collections.Generic;
using UnityEngine;


namespace Code.Upgrades
{
    [CreateAssetMenu(fileName = "Upgrades", menuName = "Upgrades/UpgradeCollection", order = 0)]
    public class UpgradeCollection : ScriptableObject
    {
        [SerializeField] private Upgrade[] m_upgrades;

        private readonly SortedDictionary<int, List<Upgrade>> m_upgradesLevels = new SortedDictionary<int, List<Upgrade>>();

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) return;
#endif
            if (m_upgrades == null || m_upgrades.Length == 0) return;
            
            foreach (var upgrade in m_upgrades)
            {
                var level = CalculateLevel(upgrade);

                if (m_upgradesLevels.TryGetValue(level, out var list))
                {
                    list.Add(upgrade);
                    continue;
                }

                m_upgradesLevels.Add(level, new List<Upgrade> { upgrade });
            }

            foreach (var pair in m_upgradesLevels)
            {
                Debug.Log($"{pair.Key} {pair.Value.Count}");
            }
        }

        public static int CalculateLevel(Upgrade upgrade, int level = 0)
        {
            var maxLevel = level;
            foreach (var dependency in upgrade.Dependencies)
            {
                var dependencyLevel = CalculateLevel(dependency.Upgrade, level + 1);
                if (dependencyLevel > maxLevel)
                    maxLevel = dependencyLevel;
            }

            return maxLevel;
        }
    }
}