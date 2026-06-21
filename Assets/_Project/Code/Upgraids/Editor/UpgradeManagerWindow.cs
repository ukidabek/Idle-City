using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Code.Upgrades
{
    public class UpgradeManagerWindow : EditorWindow
    {
        private const float SidebarWidth = 220f;
        private const float SidebarItemHeight = 28f;
        private const float SplitterWidth = 4f;
        private const float ToolbarHeight = 26f;
        private const float HeaderHeight = 48f;
        private const float MinWindowWidth = 600f;
        private const float MinWindowHeight = 400f;
        private const string WindowTitle = "Upgrade Manager";
        private const string MenuPath = "Tools/Upgrade Manager";
        private const string SidebarWidthPrefKey = "UpgradeManager.SidebarWidth";

        private readonly List<Upgrade> m_allUpgrades = new List<Upgrade>(32);
        private readonly List<Upgrade> m_filteredUpgrades = new List<Upgrade>(32);

        private Upgrade m_selectedUpgrade = null;
        private Editor m_cachedEditor = null;

        private Vector2 m_sidebarScrollPosition = Vector2.zero;
        private Vector2 m_inspectorScrollPosition = Vector2.zero;

        private string m_searchFilter = string.Empty;

        private float m_sidebarCurrentWidth = SidebarWidth;
        private bool m_isDraggingSplitter = false;

        private GUIStyle m_sidebarItemStyle;
        private GUIStyle m_sidebarItemSelectedStyle;
        private GUIStyle m_sidebarHeaderStyle;
        private GUIStyle m_pillStyle;
        private bool m_stylesInitialised;

        [MenuItem(MenuPath)]
        public static void OpenWindow()
        {
            var window = GetWindow<UpgradeManagerWindow>(WindowTitle);
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
            window.Show();
        }

        private void OnEnable()
        {
            m_sidebarCurrentWidth = EditorPrefs.GetFloat(SidebarWidthPrefKey, SidebarWidth);
            RefreshAssets();
        }

        private void OnDisable() => DestroyEditorIfExists();

        private void OnGUI()
        {
            InitialiseStylesIfNeeded();
            DrawToolbar();

            var mainRect = new Rect(0, ToolbarHeight, position.width, position.height - ToolbarHeight);
            var sidebarRect = new Rect(mainRect.x, mainRect.y, m_sidebarCurrentWidth, mainRect.height);
            var splitterRect = new Rect(sidebarRect.xMax, mainRect.y, SplitterWidth, mainRect.height);
            var inspectorRect = new Rect(splitterRect.xMax, mainRect.y, mainRect.width - m_sidebarCurrentWidth - SplitterWidth, mainRect.height);

            DrawSidebar(sidebarRect);
            DrawSplitter(splitterRect);
            DrawInspector(inspectorRect);
            HandleSplitterDrag(splitterRect);
        }

        private void DrawToolbar()
        {
            using (new GUILayout.AreaScope(new Rect(0, 0, position.width, ToolbarHeight)))
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
                    RefreshAssets();

                GUILayout.Space(4);

                EditorGUI.BeginChangeCheck();
                m_searchFilter = EditorGUILayout.TextField(m_searchFilter, EditorStyles.toolbarSearchField, GUILayout.MinWidth(120), GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    ApplyFilter();
                    if (m_selectedUpgrade != null && !m_filteredUpgrades.Contains(m_selectedUpgrade))
                        SelectUpgrade(null);
                }

                if (GUILayout.Button("✕", EditorStyles.toolbarButton, GUILayout.Width(20)) && m_searchFilter.Length > 0)
                {
                    m_searchFilter = string.Empty;
                    ApplyFilter();
                    GUI.FocusControl(null);
                }

                GUILayout.Space(4);

                var countLabel = m_searchFilter.Length > 0
                    ? $"{m_filteredUpgrades.Count} / {m_allUpgrades.Count} assets"
                    : $"{m_allUpgrades.Count} assets";
                GUILayout.Label(countLabel, EditorStyles.toolbarButton, GUILayout.Width(100));

                if (GUILayout.Button("+", EditorStyles.toolbarButton, GUILayout.Width(22)))
                    ShowCreateMenu();
            }
        }

        private void ShowCreateMenu()
        {
            var menu = new GenericMenu();
            foreach (var type in TypeCache.GetTypesDerivedFrom<Upgrade>())
            {
                if (type.IsAbstract) continue;
                var captured = type;
                menu.AddItem(new GUIContent(type.Name), false, () => CreateUpgrade(captured));
            }
            menu.ShowAsContext();
        }

        private void CreateUpgrade(System.Type type)
        {
            var path = EditorUtility.SaveFilePanelInProject("Create Upgrade", type.Name, "asset", "", "Assets");
            if (string.IsNullOrEmpty(path)) return;

            var instance = CreateInstance(type);
            AssetDatabase.CreateAsset(instance, path);
            AssetDatabase.SaveAssets();
            RefreshAssets();
            SelectUpgrade(AssetDatabase.LoadAssetAtPath<Upgrade>(path));
        }

        private void DeleteSelectedUpgrade()
        {
            if (m_selectedUpgrade == null) return;
            var confirmed = EditorUtility.DisplayDialog(
                "Delete Upgrade",
                $"Delete '{m_selectedUpgrade.name}'? This cannot be undone.",
                "Delete", "Cancel");
            if (!confirmed) return;

            var assetPath = AssetDatabase.GetAssetPath(m_selectedUpgrade);
            SelectUpgrade(null);
            AssetDatabase.DeleteAsset(assetPath);
            RefreshAssets();
        }

        private void DrawSidebar(Rect rect)
        {
            GUI.Box(rect, GUIContent.none, "CN Box");

            var headerRect = new Rect(rect.x, rect.y, rect.width, SidebarItemHeight);
            GUI.Label(headerRect, "  Upgrade Assets", m_sidebarHeaderStyle);

            var scrollRect = new Rect(rect.x, rect.y + SidebarItemHeight, rect.width, rect.height - SidebarItemHeight);
            var contentRect = new Rect(0, 0, scrollRect.width - 14f, Mathf.Max(m_filteredUpgrades.Count * SidebarItemHeight, 40f));

            using var scrollView = new GUI.ScrollViewScope(scrollRect, m_sidebarScrollPosition, contentRect);
            m_sidebarScrollPosition = scrollView.scrollPosition;

            if (m_filteredUpgrades.Count == 0)
            {
                var emptyRect = new Rect(0, 0, contentRect.width, 40f);
                var labelText = m_searchFilter.Length > 0 ? "No results found." : "No Upgrade assets found.\nClick Refresh to search.";
                GUI.Label(emptyRect, labelText, EditorStyles.centeredGreyMiniLabel);
                return;
            }

            for (var i = 0; i < m_filteredUpgrades.Count; i++)
            {
                var upgrade = m_filteredUpgrades[i];
                var itemRect = new Rect(0, i * SidebarItemHeight, contentRect.width, SidebarItemHeight);
                var isSelected = upgrade == m_selectedUpgrade;
                var itemStyle = isSelected ? m_sidebarItemSelectedStyle : m_sidebarItemStyle;

                if (!isSelected && i % 2 == 1)
                    EditorGUI.DrawRect(itemRect, new Color(0, 0, 0, 0.04f));

                var pillText = $"{upgrade.CurrentLevel}/{upgrade.Count}";
                var pillWidth = 36f;
                var nameRect = new Rect(itemRect.x + 6, itemRect.y, itemRect.width - pillWidth - 14f, itemRect.height);
                var pillRect = new Rect(itemRect.xMax - pillWidth - 4f, itemRect.y + 6f, pillWidth, itemRect.height - 12f);

                if (GUI.Button(itemRect, GUIContent.none, itemStyle))
                    SelectUpgrade(upgrade);

                GUI.Label(nameRect, upgrade.name, isSelected ? m_sidebarItemSelectedStyle : m_sidebarItemStyle);

                var pillColor = upgrade.CanLevelUp ? new Color(0.2f, 0.7f, 0.3f, 0.8f) : new Color(0.4f, 0.4f, 0.4f, 0.6f);
                EditorGUI.DrawRect(pillRect, pillColor);
                GUI.Label(pillRect, pillText, m_pillStyle);
            }
        }

        private void DrawInspector(Rect rect)
        {
            if (m_selectedUpgrade == null)
            {
                EditorGUI.DrawRect(rect, new Color(0.18f, 0.18f, 0.18f, 1f));
                GUI.Label(rect, "← Select an Upgrade asset", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            // Name + Ping row
            var nameRowRect = new Rect(rect.x, rect.y, rect.width, SidebarItemHeight);
            EditorGUI.DrawRect(nameRowRect, new Color(0.24f, 0.24f, 0.24f, 1f));

            var nameLabelRect = new Rect(nameRowRect.x + 6, nameRowRect.y, nameRowRect.width - 132f, nameRowRect.height);
            var deleteRect = new Rect(nameRowRect.xMax - 126f, nameRowRect.y + 4f, 56f, 20f);
            var pingRect = new Rect(nameRowRect.xMax - 64f, nameRowRect.y + 4f, 56f, 20f);
            GUI.Label(nameLabelRect, m_selectedUpgrade.name, EditorStyles.boldLabel);
            if (GUI.Button(deleteRect, "Delete", EditorStyles.miniButton))
                DeleteSelectedUpgrade();
            if (GUI.Button(pingRect, "Ping", EditorStyles.miniButton))
            {
                EditorGUIUtility.PingObject(m_selectedUpgrade);
                Selection.activeObject = m_selectedUpgrade;
            }

            // Level + LevelUp row
            var levelRowRect = new Rect(rect.x, rect.y + SidebarItemHeight, rect.width, SidebarItemHeight);
            EditorGUI.DrawRect(levelRowRect, new Color(0.21f, 0.21f, 0.21f, 1f));

            var levelLabelRect = new Rect(levelRowRect.x + 6, levelRowRect.y, levelRowRect.width - 90f, levelRowRect.height);
            var levelUpRect = new Rect(levelRowRect.xMax - 82f, levelRowRect.y + 4f, 78f, 20f);

            GUI.Label(levelLabelRect, $"Level  {m_selectedUpgrade.CurrentLevel} / {m_selectedUpgrade.Count}", EditorStyles.label);

            var prevEnabled = GUI.enabled;
            GUI.enabled = m_selectedUpgrade.CanLevelUp;
            if (GUI.Button(levelUpRect, "Level Up ▲", EditorStyles.miniButton))
            {
                m_selectedUpgrade.LevelUp();
                EditorUtility.SetDirty(m_selectedUpgrade);
                AssetDatabase.SaveAssets();
                Repaint();
            }
            GUI.enabled = prevEnabled;

            // Inspector scroll
            var inspectorScrollRect = new Rect(rect.x, rect.y + HeaderHeight, rect.width, rect.height - HeaderHeight);
            var contentRect = new Rect(0, 0, inspectorScrollRect.width - 14f, 10000f);

            using var scrollView = new GUI.ScrollViewScope(inspectorScrollRect, m_inspectorScrollPosition, contentRect);
            m_inspectorScrollPosition = scrollView.scrollPosition;

            using (new GUILayout.AreaScope(contentRect))
            {
                GUILayout.Space(4);
                if (m_cachedEditor != null) m_cachedEditor.OnInspectorGUI();
                GUILayout.Space(8);
            }
        }

        private void DrawSplitter(Rect rect)
        {
            EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 1f));
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);
        }

        private void HandleSplitterDrag(Rect splitterRect)
        {
            var currentEvent = Event.current;

            if (currentEvent.type == EventType.MouseDown && splitterRect.Contains(currentEvent.mousePosition))
            {
                m_isDraggingSplitter = true;
                currentEvent.Use();
            }

            if (!m_isDraggingSplitter) return;

            if (currentEvent.type == EventType.MouseDrag)
            {
                m_sidebarCurrentWidth = Mathf.Clamp(currentEvent.mousePosition.x, 120f, position.width - 300f);
                EditorPrefs.SetFloat(SidebarWidthPrefKey, m_sidebarCurrentWidth);
                Repaint();
                currentEvent.Use();
            }

            if (currentEvent.type != EventType.MouseUp) return;
            m_isDraggingSplitter = false;
            currentEvent.Use();
        }

        private void RefreshAssets()
        {
            m_allUpgrades.Clear();

            var guids = AssetDatabase.FindAssets($"t:{nameof(Upgrade)}");
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var upgrade = AssetDatabase.LoadAssetAtPath<Upgrade>(assetPath);
                if (upgrade == null) continue;
                m_allUpgrades.Add(upgrade);
            }

            m_allUpgrades.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase));
            ApplyFilter();

            if (m_selectedUpgrade != null && !m_allUpgrades.Contains(m_selectedUpgrade))
                SelectUpgrade(null);

            Repaint();
        }

        private void ApplyFilter()
        {
            m_filteredUpgrades.Clear();

            if (string.IsNullOrWhiteSpace(m_searchFilter))
            {
                m_filteredUpgrades.AddRange(m_allUpgrades);
                return;
            }

            var lowerFilter = m_searchFilter.ToLowerInvariant();
            foreach (var upgrade in m_allUpgrades)
            {
                if (upgrade.name.ToLowerInvariant().Contains(lowerFilter))
                    m_filteredUpgrades.Add(upgrade);
            }
        }

        private void SelectUpgrade(Upgrade upgrade)
        {
            if (m_selectedUpgrade == upgrade) return;

            m_selectedUpgrade = upgrade;
            m_inspectorScrollPosition = Vector2.zero;

            DestroyEditorIfExists();
            if (m_selectedUpgrade != null)
                m_cachedEditor = Editor.CreateEditor(m_selectedUpgrade);

            Repaint();
        }

        private void DestroyEditorIfExists()
        {
            if (m_cachedEditor == null) return;
            DestroyImmediate(m_cachedEditor);
            m_cachedEditor = null;
        }

        private void InitialiseStylesIfNeeded()
        {
            if (m_stylesInitialised) return;

            m_sidebarItemStyle = new GUIStyle(EditorStyles.label)
            {
                fixedHeight = SidebarItemHeight,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(4, 4, 0, 0),
                fontSize = 12,
            };

            m_sidebarItemSelectedStyle = new GUIStyle(m_sidebarItemStyle);
            m_sidebarItemSelectedStyle.normal.background = MakeSolidTexture(new Color(0.17f, 0.36f, 0.53f, 1f));
            m_sidebarItemSelectedStyle.normal.textColor = Color.white;
            m_sidebarItemSelectedStyle.hover.background = m_sidebarItemSelectedStyle.normal.background;
            m_sidebarItemSelectedStyle.hover.textColor = Color.white;

            m_sidebarHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fixedHeight = SidebarItemHeight,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(6, 0, 0, 0),
                fontSize = 11,
            };

            m_pillStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9,
                fontStyle = FontStyle.Bold,
            };
            m_pillStyle.normal.textColor = Color.white;

            m_stylesInitialised = true;
        }

        private static Texture2D MakeSolidTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
