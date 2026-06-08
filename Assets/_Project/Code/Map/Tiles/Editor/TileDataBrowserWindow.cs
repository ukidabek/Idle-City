using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Project.Map
{
    public class TileDataBrowserWindow : EditorWindow
    {
        private const float k_SidebarWidth = 220f;
        private const float k_SidebarItemHeight = 28f;
        private const float k_SplitterWidth = 4f;
        private const float k_ToolbarHeight = 26f;
        private const float k_MinWindowWidth = 600f;
        private const float k_MinWindowHeight = 400f;
        private const string k_WindowTitle = "Tile Data Browser";
        private const string k_MenuPath = "Tools/Tile Data Browser";

        private List<TileData> m_AllTileData = new();
        private List<TileData> m_FilteredTileData = new();
        private TileData m_SelectedTileData;
        private Editor m_CachedEditor;

        private Vector2 m_SidebarScrollPos;
        private Vector2 m_InspectorScrollPos;

        private string m_SearchFilter = string.Empty;
        
        private float m_SidebarCurrentWidth = k_SidebarWidth;
        private bool m_IsDraggingSplitter;

        private GUIStyle m_SidebarItemStyle;
        private GUIStyle m_SidebarItemSelectedStyle;
        private GUIStyle m_SidebarHeaderStyle;
        private bool m_StylesInitialised;

        [MenuItem(k_MenuPath)]
        public static void OpenWindow()
        {
            var window = GetWindow<TileDataBrowserWindow>(k_WindowTitle);
            window.minSize = new Vector2(k_MinWindowWidth, k_MinWindowHeight);
            window.Show();
        }

        
        private void OnEnable() => RefreshAssets();

        private void OnDisable() => DestroyEditorIfExists();
        
        private void OnGUI()
        {
            InitialiseStylesIfNeeded();
            DrawToolbar();

            var mainRect = new Rect(0, k_ToolbarHeight, position.width, position.height - k_ToolbarHeight);

            var sidebarRect = new Rect(mainRect.x, mainRect.y, m_SidebarCurrentWidth, mainRect.height);

            var splitterRect = new Rect(sidebarRect.xMax, mainRect.y, k_SplitterWidth, mainRect.height);

            var inspectorRect = new Rect(splitterRect.xMax, mainRect.y, mainRect.width - m_SidebarCurrentWidth - k_SplitterWidth, mainRect.height);

            DrawSidebar(sidebarRect);
            DrawSplitter(splitterRect);
            DrawInspector(inspectorRect);
            HandleSplitterDrag(splitterRect);
        }

        private void DrawToolbar()
        {
            var toolbarRect = new Rect(0, 0, position.width, k_ToolbarHeight);

            using (new GUILayout.AreaScope(toolbarRect))
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
                {
                    RefreshAssets();
                }

                GUILayout.Space(4);

                EditorGUI.BeginChangeCheck();
                m_SearchFilter = EditorGUILayout.TextField(m_SearchFilter, EditorStyles.toolbarSearchField, GUILayout.MinWidth(120), GUILayout.ExpandWidth(true));
                
                if (EditorGUI.EndChangeCheck())
                {
                    ApplyFilter();
                    if (m_SelectedTileData != null && !m_FilteredTileData.Contains(m_SelectedTileData)) 
                        SelectTileData(null);
                }

                if (GUILayout.Button("✕", EditorStyles.toolbarButton, GUILayout.Width(20)) && m_SearchFilter.Length > 0)
                {
                    m_SearchFilter = string.Empty;
                    ApplyFilter();
                    GUI.FocusControl(null);
                }

                GUILayout.Space(4);

                var countToShow = m_FilteredTileData.Count;
                var totalCount = m_AllTileData.Count;
                var countLabel = m_SearchFilter.Length > 0 ? $"{countToShow} / {totalCount} assets" : $"{totalCount} assets";
                GUILayout.Label(countLabel, EditorStyles.toolbarButton, GUILayout.Width(100));
            }
        }

        private void DrawSidebar(Rect rect)
        {
            GUI.Box(rect, GUIContent.none, "CN Box");

            var headerRect = new Rect(rect.x, rect.y, rect.width, k_SidebarItemHeight);
            GUI.Label(headerRect, "  TileData Assets", m_SidebarHeaderStyle);

            var scrollRect = new Rect(rect.x, rect.y + k_SidebarItemHeight, rect.width, rect.height - k_SidebarItemHeight);

            var contentHeight = m_FilteredTileData.Count * k_SidebarItemHeight;
            var contentRect = new Rect(0, 0, scrollRect.width - 14f, contentHeight);

            using var scrollView = new GUI.ScrollViewScope(scrollRect, m_SidebarScrollPos, contentRect);
            m_SidebarScrollPos = scrollView.scrollPosition;

            for (var itemIndex = 0; itemIndex < m_FilteredTileData.Count; itemIndex++)
            {
                var tileData = m_FilteredTileData[itemIndex];
                if (tileData == null) continue;

                var itemRect = new Rect(0, itemIndex * k_SidebarItemHeight,
                    contentRect.width, k_SidebarItemHeight);

                var isSelected = tileData == m_SelectedTileData;
                var itemStyle = isSelected ? m_SidebarItemSelectedStyle : m_SidebarItemStyle;

                if (!isSelected && itemIndex % 2 == 1) 
                    EditorGUI.DrawRect(itemRect, new Color(0, 0, 0, 0.04f));

                Texture assetIcon = AssetPreview.GetMiniThumbnail(tileData);
                var itemContent = assetIcon != null ? new GUIContent($"  {tileData.name}", assetIcon) : new GUIContent($"  {tileData.name}");

                if (GUI.Button(itemRect, itemContent, itemStyle))
                {
                    SelectTileData(tileData);

                    if (Event.current.clickCount == 2)
                    {
                        EditorGUIUtility.PingObject(tileData);
                    }
                }

                if (Event.current.type != EventType.ContextClick || !itemRect.Contains(Event.current.mousePosition)) continue;
                ShowContextMenu(tileData);
                Event.current.Use();
            }

            if (m_FilteredTileData.Count != 0) return;
            
            var emptyRect = new Rect(0, 0, contentRect.width, 40);
            var labelText = m_SearchFilter.Length > 0 ? "No results found." : "No TileData assets found.\nClick Refresh to search.";
            GUI.Label(emptyRect, labelText, EditorStyles.centeredGreyMiniLabel);
        }

        
        private void DrawInspector(Rect rect)
        {
            if (m_SelectedTileData == null)
            {
                EditorGUI.DrawRect(rect, new Color(0.18f, 0.18f, 0.18f, 1f));
                GUI.Label(rect, "← Select a TileData asset", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            var headerRect = new Rect(rect.x, rect.y, rect.width, k_SidebarItemHeight);
            EditorGUI.DrawRect(headerRect, new Color(0.24f, 0.24f, 0.24f, 1f));

            var labelRect = new Rect(headerRect.x + 6, headerRect.y,
                headerRect.width - 60, headerRect.height);
            var pingRect = new Rect(headerRect.xMax - 56, headerRect.y + 3, 52, 20);

            GUI.Label(labelRect, m_SelectedTileData.name, EditorStyles.boldLabel);

            if (GUI.Button(pingRect, "Ping", EditorStyles.miniButton))
            {
                EditorGUIUtility.PingObject(m_SelectedTileData);
                Selection.activeObject = m_SelectedTileData;
            }

            var inspectorScrollRect = new Rect(rect.x, rect.y + k_SidebarItemHeight,
                rect.width, rect.height - k_SidebarItemHeight);

            var screenRect = new Rect(0, 0, inspectorScrollRect.width - 14f, 10000f);
            using var scrollView = new GUI.ScrollViewScope(inspectorScrollRect, m_InspectorScrollPos, screenRect);
            m_InspectorScrollPos = scrollView.scrollPosition;

            using (new GUILayout.AreaScope(screenRect))
            {
                GUILayout.Space(4);

                if (m_CachedEditor != null) m_CachedEditor.OnInspectorGUI();

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
                m_IsDraggingSplitter = true;
                currentEvent.Use();
            }

            if (!m_IsDraggingSplitter) return;
            
            if (currentEvent.type == EventType.MouseDrag)
            {
                m_SidebarCurrentWidth = Mathf.Clamp(currentEvent.mousePosition.x, 120f, position.width - 300f);
                Repaint();
                currentEvent.Use();
            }

            
            if (currentEvent.type != EventType.MouseUp) return;
            m_IsDraggingSplitter = false;
            currentEvent.Use();
        }

        private void ShowContextMenu(TileData tileData)
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Select in Project"), false, () =>
            {
                Selection.activeObject = tileData;
                EditorGUIUtility.PingObject(tileData);
            });

            menu.AddItem(new GUIContent("Copy Asset Path"), false, () => { EditorGUIUtility.systemCopyBuffer = AssetDatabase.GetAssetPath(tileData); });
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Delete Asset"), false, () =>
            {
                if (!EditorUtility.DisplayDialog("Delete TileData", $"Are you sure you want to delete '{tileData.name}'?", "Delete", "Cancel")) return;
                
                var assetPath = AssetDatabase.GetAssetPath(tileData);
                AssetDatabase.DeleteAsset(assetPath);
                RefreshAssets();
            });

            menu.ShowAsContext();
        }

        private void RefreshAssets()
        {
            m_AllTileData.Clear();

            var guids = AssetDatabase.FindAssets($"t:{nameof(TileData)}");

            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var tileData = AssetDatabase.LoadAssetAtPath<TileData>(assetPath);

                if (tileData == null) continue;
                m_AllTileData.Add(tileData);
            }

            m_AllTileData.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase));

            ApplyFilter();

            // Re-validate selection
            if (m_SelectedTileData != null && !m_AllTileData.Contains(m_SelectedTileData)) 
                SelectTileData(null);

            Repaint();
        }

        private void ApplyFilter()
        {
            m_FilteredTileData.Clear();

            if (string.IsNullOrWhiteSpace(m_SearchFilter))
            {
                m_FilteredTileData.AddRange(m_AllTileData);
                return;
            }

            var lowerFilter = m_SearchFilter.ToLowerInvariant();

            foreach (var tileData in m_AllTileData)
            {
                if (!tileData.name.ToLowerInvariant().Contains(lowerFilter)) continue;
                m_FilteredTileData.Add(tileData);
            }
        }

        private void SelectTileData(TileData tileData)
        {
            if (m_SelectedTileData == tileData) return;

            m_SelectedTileData = tileData;
            m_InspectorScrollPos = Vector2.zero;

            DestroyEditorIfExists();

            if (m_SelectedTileData != null)
            {
                m_CachedEditor = UnityEditor.Editor.CreateEditor(m_SelectedTileData);
            }

            Repaint();
        }

        private void DestroyEditorIfExists()
        {
            if (m_CachedEditor == null) return;
            DestroyImmediate(m_CachedEditor);
            m_CachedEditor = null;
        }
        
        private void InitialiseStylesIfNeeded()
        {
            if (m_StylesInitialised) return;

            m_SidebarItemStyle = new GUIStyle(EditorStyles.label)
            {
                fixedHeight = k_SidebarItemHeight,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(4, 4, 0, 0),
                fontSize = 12,
            };

            m_SidebarItemSelectedStyle = new GUIStyle(m_SidebarItemStyle);
            m_SidebarItemSelectedStyle.normal.background = MakeSolidTexture(new Color(0.17f, 0.36f, 0.53f, 1f));
            m_SidebarItemSelectedStyle.normal.textColor = Color.white;
            m_SidebarItemSelectedStyle.hover.background = m_SidebarItemSelectedStyle.normal.background;
            m_SidebarItemSelectedStyle.hover.textColor = Color.white;

            m_SidebarHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fixedHeight = k_SidebarItemHeight,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(6, 0, 0, 0),
                fontSize = 11,
            };

            m_StylesInitialised = true;
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