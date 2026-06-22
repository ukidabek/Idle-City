using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Code.Upgrades
{
    public static class UpgradeCollectionFiller
    {
        public static readonly string UpgradeFilter = $"t:{nameof(Upgrade)}";
        public static readonly string UpgradeCollectionFilter = $"t:{nameof(UpgradeCollection)}";
        public static readonly BindingFlags BindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        public static readonly FieldInfo UpgradesList = null;

        static UpgradeCollectionFiller()
        {
            var type = typeof(UpgradeCollection);
            UpgradesList = type.GetField("m_upgrades", BindingFlags);
        }

        [InitializeOnEnterPlayMode]
        private static void OnEnterPlaymodeInEditor(EnterPlayModeOptions options)
        {
            if (options.HasFlag(EnterPlayModeOptions.DisableDomainReload))
                return;

            var upgrades = GetAllUpdates();
            var upgradesCollection = GetUpgradeCollection();

            UpgradesList.SetValue(upgradesCollection, upgrades);
        }

        public static Upgrade[] GetAllUpdates()
        {
            return AssetDatabase.FindAssets(UpgradeFilter)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<Upgrade>)
                .ToArray();
        }

        public static UpgradeCollection GetUpgradeCollection()
        {
            var guids = AssetDatabase.FindAssets(UpgradeCollectionFilter);
            if (guids.Length > 1)
            {
                Debug.LogError($"There is more than one upgrade collection({nameof(UpgradeCollection)})!");
            }

            return AssetDatabase.LoadAssetAtPath<UpgradeCollection>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }

    public class UpgradeCollectionFillerPreprocess : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
        }
    }
}