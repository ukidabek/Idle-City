using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Project.Map
{
    public class TileDataBrowserWindow : EditorWindow
    {
        private const float SidebarWidth = 220f;
        private const float SidebarItemHeight = 28f;
        private const float SidebarGroupHeaderHeight = 22f;
        private const float SplitterWidth = 4f;
        private const float ToolbarHeight = 26f;
        private const float MinWindowWidth = 600f;
        private const float MinWindowHeight = 400f;
        private const string WindowTitle = "Tile Data Browser";
        private const string MenuPath = "Tools/Tile Data Browser";

        private List<TileData> m_allTileData = new List<TileData>(100);
        private List<TileData> m_filteredTileData = new List<TileData>(100);
        private TileData m_selectedTileData = null;
        private Editor m_cachedEditor = null;

        private Vector2 m_sidebarScrollPosition = Vector2.zero;
        private Vector2 m_inspectorScrollPosition = Vector2.zero;

        private string m_searchFilter = string.Empty;

        private float m_sidebarCurrentWidth = SidebarWidth;
        private bool m_isDraggingSplitter = false;

        private Dictionary<string, List<TileData>> m_GroupedTileData = new Dictionary<string, List<TileData>>(100);
        private Dictionary<string, bool> m_groupFoldouts = new Dictionary<string, bool>(100);
        private List<string> m_sortedGroupKeys = new List<string>(100);

        // Rename state
        private bool m_isRenaming = false;
        private string m_renameBuffer = string.Empty;
        private const string k_RenameControlName = "TileDataRenameField";

        private GUIStyle m_sidebarItemStyle;
        private GUIStyle m_sidebarItemSelectedStyle;
        private GUIStyle m_sidebarHeaderStyle;
        private GUIStyle m_sidebarGroupHeaderStyle;
        private bool m_stylesInitialised;

        [MenuItem(MenuPath)]
        public static void OpenWindow()
        {
            var window = GetWindow<TileDataBrowserWindow>(WindowTitle);
            window.minSize = new Vector2(MinWindowWidth, MinWindowHeight);
            window.Show();
        }

        private void OnEnable() => RefreshAssets();
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
            var toolbarRect = new Rect(0, 0, position.width, ToolbarHeight);

            using (new GUILayout.AreaScope(toolbarRect))
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
                    if (m_selectedTileData != null && !m_filteredTileData.Contains(m_selectedTileData))
                        SelectTileData(null);
                }

                if (GUILayout.Button("✕", EditorStyles.toolbarButton, GUILayout.Width(20)) && m_searchFilter.Length > 0)
                {
                    m_searchFilter = string.Empty;
                    ApplyFilter();
                    GUI.FocusControl(null);
                }

                GUILayout.Space(4);

                var countToShow = m_filteredTileData.Count;
                var totalCount = m_allTileData.Count;
                var countLabel = m_searchFilter.Length > 0 ? $"{countToShow} / {totalCount} assets" : $"{totalCount} assets";
                GUILayout.Label(countLabel, EditorStyles.toolbarButton, GUILayout.Width(100));
            }
        }

        private void DrawSidebar(Rect rect)
        {
            GUI.Box(rect, GUIContent.none, "CN Box");

            var headerRect = new Rect(rect.x, rect.y, rect.width, SidebarItemHeight);
            GUI.Label(headerRect, "  TileData Assets", m_sidebarHeaderStyle);

            var scrollRect = new Rect(rect.x, rect.y + SidebarItemHeight, rect.width, rect.height - SidebarItemHeight);

            var contentHeight = 0f;
            foreach (var groupKey in m_sortedGroupKeys)
            {
                contentHeight += SidebarGroupHeaderHeight;
                if (m_groupFoldouts.TryGetValue(groupKey, out var isExpanded) && isExpanded)
                    contentHeight += m_GroupedTileData[groupKey].Count * SidebarItemHeight;
            }
            if (m_sortedGroupKeys.Count == 0)
                contentHeight = 40f;

            var contentRect = new Rect(0, 0, scrollRect.width - 14f, contentHeight);

            using var scrollView = new GUI.ScrollViewScope(scrollRect, m_sidebarScrollPosition, contentRect);
            m_sidebarScrollPosition = scrollView.scrollPosition;

            var cursor = 0f;
            var globalItemIndex = 0;

            foreach (var groupKey in m_sortedGroupKeys)
            {
                var groupItems = m_GroupedTileData[groupKey];

                var groupHeaderRect = new Rect(0, cursor, contentRect.width, SidebarGroupHeaderHeight);
                var isExpanded = m_groupFoldouts.GetValueOrDefault(groupKey, true);

                EditorGUI.DrawRect(groupHeaderRect, new Color(0.15f, 0.15f, 0.15f, 1f));

                var foldoutRect = new Rect(groupHeaderRect.x + 2, groupHeaderRect.y, groupHeaderRect.width - 2, groupHeaderRect.height);
                var newExpanded = EditorGUI.Foldout(foldoutRect, isExpanded,
                    $"  {groupKey}  ({groupItems.Count})", true, m_sidebarGroupHeaderStyle);

                if (newExpanded != isExpanded)
                {
                    m_groupFoldouts[groupKey] = newExpanded;
                    Repaint();
                }

                cursor += SidebarGroupHeaderHeight;

                if (!newExpanded) continue;

                for (var itemIndex = 0; itemIndex < groupItems.Count; itemIndex++, globalItemIndex++)
                {
                    var tileData = groupItems[itemIndex];
                    if (tileData == null) continue;

                    var itemRect = new Rect(0, cursor, contentRect.width, SidebarItemHeight);
                    var isSelected = tileData == m_selectedTileData;
                    var itemStyle = isSelected ? m_sidebarItemSelectedStyle : m_sidebarItemStyle;

                    if (!isSelected && globalItemIndex % 2 == 1)
                        EditorGUI.DrawRect(itemRect, new Color(0, 0, 0, 0.04f));

                    Texture assetIcon = AssetPreview.GetMiniThumbnail(tileData);
                    var itemContent = assetIcon != null
                        ? new GUIContent($"    {tileData.name}", assetIcon)
                        : new GUIContent($"    {tileData.name}");

                    if (GUI.Button(itemRect, itemContent, itemStyle))
                        SelectTileData(tileData);

                    cursor += SidebarItemHeight;
                }
            }

            if (m_sortedGroupKeys.Count == 0)
            {
                var emptyRect = new Rect(0, 0, contentRect.width, 40);
                var labelText = m_searchFilter.Length > 0 ? "No results found." : "No TileData assets found.\nClick Refresh to search.";
                GUI.Label(emptyRect, labelText, EditorStyles.centeredGreyMiniLabel);
            }
        }

        private void DrawInspector(Rect rect)
        {
            if (m_selectedTileData == null)
            {
                EditorGUI.DrawRect(rect, new Color(0.18f, 0.18f, 0.18f, 1f));
                GUI.Label(rect, "← Select a TileData asset", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            var headerRect = new Rect(rect.x, rect.y, rect.width, SidebarItemHeight);
            EditorGUI.DrawRect(headerRect, new Color(0.24f, 0.24f, 0.24f, 1f));

            if (m_isRenaming)
            {
                DrawInspectorRenameHeader(headerRect);
            }
            else
            {
                DrawInspectorNormalHeader(headerRect);
            }

            var inspectorScrollRect = new Rect(rect.x, rect.y + SidebarItemHeight, rect.width, rect.height - SidebarItemHeight);
            var screenRect = new Rect(0, 0, inspectorScrollRect.width - 14f, 10000f);

            using var scrollView = new GUI.ScrollViewScope(inspectorScrollRect, m_inspectorScrollPosition, screenRect);
            m_inspectorScrollPosition = scrollView.scrollPosition;

            using (new GUILayout.AreaScope(screenRect))
            {
                GUILayout.Space(4);
                if (m_cachedEditor != null) m_cachedEditor.OnInspectorGUI();
                GUILayout.Space(8);
            }
        }

        private void DrawInspectorNormalHeader(Rect headerRect)
        {
            var labelRect  = new Rect(headerRect.x + 6,      headerRect.y, headerRect.width - 114, headerRect.height);
            var renameRect = new Rect(headerRect.xMax - 110,  headerRect.y + 3, 52, 20);
            var pingRect   = new Rect(headerRect.xMax - 56,   headerRect.y + 3, 52, 20);

            GUI.Label(labelRect, m_selectedTileData.name, EditorStyles.boldLabel);

            if (GUI.Button(renameRect, "Rename", EditorStyles.miniButton))
                BeginRename();

            if (GUI.Button(pingRect, "Ping", EditorStyles.miniButton))
            {
                EditorGUIUtility.PingObject(m_selectedTileData);
                Selection.activeObject = m_selectedTileData;
            }
        }

        private void DrawInspectorRenameHeader(Rect headerRect)
        {
            var fieldRect   = new Rect(headerRect.x + 4,       headerRect.y + 3, headerRect.width - 130, headerRect.height - 6);
            var confirmRect = new Rect(headerRect.xMax - 126,  headerRect.y + 3, 52, 20);
            var cancelRect  = new Rect(headerRect.xMax - 70,   headerRect.y + 3, 66, 20);

            GUI.SetNextControlName(k_RenameControlName);
            m_renameBuffer = GUI.TextField(fieldRect, m_renameBuffer);

            if (GUI.GetNameOfFocusedControl() != k_RenameControlName)
                EditorGUI.FocusTextInControl(k_RenameControlName);

            // Keyboard shortcuts
            var e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                {
                    CommitRename();
                    e.Use();
                }
                else if (e.keyCode == KeyCode.Escape)
                {
                    CancelRename();
                    e.Use();
                }
            }

            if (GUI.Button(confirmRect, "✓ OK", EditorStyles.miniButton))
                CommitRename();

            if (GUI.Button(cancelRect, "✕ Cancel", EditorStyles.miniButton))
                CancelRename();
        }

        private void BeginRename()
        {
            m_isRenaming = true;
            m_renameBuffer = m_selectedTileData.name;
            Repaint();
        }

        private void CommitRename()
        {
            var trimmed = m_renameBuffer.Trim();

            if (!string.IsNullOrEmpty(trimmed) && trimmed != m_selectedTileData.name)
            {
                var assetPath = AssetDatabase.GetAssetPath(m_selectedTileData);
                AssetDatabase.RenameAsset(assetPath, trimmed);
                AssetDatabase.SaveAssets();
                RefreshAssets();
                SelectTileData(m_selectedTileData);
            }

            m_isRenaming = false;
            m_renameBuffer = string.Empty;
            GUI.FocusControl(null);
            Repaint();
        }

        private void CancelRename()
        {
            m_isRenaming = false;
            m_renameBuffer = string.Empty;
            GUI.FocusControl(null);
            Repaint();
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
                Repaint();
                currentEvent.Use();
            }

            if (currentEvent.type != EventType.MouseUp) return;
            m_isDraggingSplitter = false;
            currentEvent.Use();
        }

        private void RefreshAssets()
        {
            m_allTileData.Clear();

            var guids = AssetDatabase.FindAssets($"t:{nameof(TileData)}");

            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var tileData = AssetDatabase.LoadAssetAtPath<TileData>(assetPath);
                if (tileData == null) continue;
                m_allTileData.Add(tileData);
            }

            m_allTileData.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase));

            ApplyFilter();

            if (m_selectedTileData != null && !m_allTileData.Contains(m_selectedTileData))
                SelectTileData(null);

            Repaint();
        }

        private void ApplyFilter()
        {
            m_filteredTileData.Clear();

            if (string.IsNullOrWhiteSpace(m_searchFilter))
            {
                m_filteredTileData.AddRange(m_allTileData);
            }
            else
            {
                var lowerFilter = m_searchFilter.ToLowerInvariant();
                foreach (var tileData in m_allTileData)
                {
                    if (tileData.name.ToLowerInvariant().Contains(lowerFilter))
                        m_filteredTileData.Add(tileData);
                }
            }

            RebuildGroups();
        }

        private void RebuildGroups()
        {
            m_GroupedTileData.Clear();
            m_sortedGroupKeys.Clear();

            foreach (var tileData in m_filteredTileData)
            {
                var typeName = tileData.GetType().Name;

                if (!m_GroupedTileData.ContainsKey(typeName))
                    m_GroupedTileData[typeName] = new List<TileData>();

                m_GroupedTileData[typeName].Add(tileData);
            }

            m_sortedGroupKeys.AddRange(m_GroupedTileData.Keys);
            m_sortedGroupKeys.Sort(System.StringComparer.OrdinalIgnoreCase);

            foreach (var key in m_sortedGroupKeys)
            {
                if (!m_groupFoldouts.ContainsKey(key))
                    m_groupFoldouts[key] = true;
            }
        }

        private void SelectTileData(TileData tileData)
        {
            if (m_selectedTileData == tileData) return;

            m_isRenaming = false;
            m_renameBuffer = string.Empty;
            m_selectedTileData = tileData;
            m_inspectorScrollPosition = Vector2.zero;

            DestroyEditorIfExists();

            if (m_selectedTileData != null)
                m_cachedEditor = UnityEditor.Editor.CreateEditor(m_selectedTileData);

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

            m_sidebarGroupHeaderStyle = new GUIStyle(EditorStyles.foldout)
            {
                fixedHeight = SidebarGroupHeaderHeight,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(16, 4, 0, 0),
                fontSize = 11,
                fontStyle = FontStyle.Bold,
            };
            m_sidebarGroupHeaderStyle.normal.textColor = new Color(0.75f, 0.85f, 1f, 1f);
            m_sidebarGroupHeaderStyle.onNormal.textColor = new Color(0.75f, 0.85f, 1f, 1f);

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