using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities.General;


namespace Code.Upgrades
{
    [CreateAssetMenu(fileName = "Upgrades", menuName = "Upgrades/UpgradeCollection", order = 0)]
    public class UpgradeCollection : ScriptableObject,
        IReadOnlyList<Upgrade>,
        IReadOnlyDictionary<int, IReadOnlyList<Upgrade>>
    {
        [SerializeField] private Upgrade[] m_upgrades;
        [field: SerializeField, ReadOnly] public Vector2Int Size { get; private set; }

        private readonly SortedDictionary<int, IReadOnlyList<Upgrade>> m_upgradesLevels = new SortedDictionary<int, IReadOnlyList<Upgrade>>();
        IEnumerator<Upgrade> IEnumerable<Upgrade>.GetEnumerator() => m_upgrades.AsEnumerable().GetEnumerator();
        int IReadOnlyCollection<Upgrade>.Count => m_upgrades.Length;
        Upgrade IReadOnlyList<Upgrade>.this[int index] => m_upgrades[index];
        IEnumerator<KeyValuePair<int, IReadOnlyList<Upgrade>>> IEnumerable<KeyValuePair<int, IReadOnlyList<Upgrade>>>.GetEnumerator() => m_upgradesLevels.GetEnumerator();
        int IReadOnlyCollection<KeyValuePair<int, IReadOnlyList<Upgrade>>>.Count => m_upgradesLevels.Count;
        bool IReadOnlyDictionary<int, IReadOnlyList<Upgrade>>.ContainsKey(int key) => m_upgradesLevels.ContainsKey(key);
        bool IReadOnlyDictionary<int, IReadOnlyList<Upgrade>>.TryGetValue(int key, out IReadOnlyList<Upgrade> value) => m_upgradesLevels.TryGetValue(key, out value);
        IReadOnlyList<Upgrade> IReadOnlyDictionary<int, IReadOnlyList<Upgrade>>.this[int key] => m_upgradesLevels[key];
        IEnumerable<int> IReadOnlyDictionary<int, IReadOnlyList<Upgrade>>.Keys => m_upgradesLevels.Keys;
        IEnumerable<IReadOnlyList<Upgrade>> IReadOnlyDictionary<int, IReadOnlyList<Upgrade>>.Values => m_upgradesLevels.Values;
        IEnumerator IEnumerable.GetEnumerator() => m_upgrades.GetEnumerator();

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) return;
#endif
            var dictionary = GetValue(m_upgrades);
            foreach (var pair in dictionary)
                m_upgradesLevels.Add(pair.Key, pair.Value);
            
            var height = m_upgradesLevels.Count;
            var width = m_upgradesLevels.Values.Select(list => list.Count).Max();

            Size = new Vector2Int(width, height);
        }

        public static SortedDictionary<int, List<Upgrade>> GetValue(IEnumerable<Upgrade> upgrades)
        {
            if (upgrades == null) return null;
            if (!upgrades.Any()) return null;

            var buildMap = new SortedDictionary<int, List<Upgrade>>();
            foreach (var upgrade in upgrades)
            {
                var level = CalculateLevel(upgrade);
                if (!buildMap.TryGetValue(level, out var list))
                {
                    list = new List<Upgrade>();
                    buildMap.Add(level, list);
                }

                list.Add(upgrade);
            }

            return buildMap;
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
