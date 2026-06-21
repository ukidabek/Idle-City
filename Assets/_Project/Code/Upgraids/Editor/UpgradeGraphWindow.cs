using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Code.Upgrades
{
    public class UpgradeGraphWindow : EditorWindow
    {
        private const float NodeWidth = 160f;
        private const float NodeHeight = 52f;
        private const float RowHeight = 110f;
        private const float ToolbarHeight = 26f;
        private const float MinZoom = 0.4f;
        private const float MaxZoom = 2f;
        private const string WindowTitle = "Upgrade Graph";
        private const string MenuPath = "Tools/Upgrade Graph";

        private static readonly Color NodeLockedColor = new Color(0.22f, 0.22f, 0.22f, 1f);
        private static readonly Color NodeCanLevelColor = new Color(0.22f, 0.55f, 0.32f, 1f);
        private static readonly Color NodeMaxedColor = new Color(0.20f, 0.38f, 0.42f, 1f);
        private static readonly Color EdgeSatisfiedColor = new Color(0.3f, 0.7f, 0.3f, 0.9f);
        private static readonly Color EdgeUnsatisfiedColor = new Color(0.7f, 0.3f, 0.3f, 0.9f);
        private static readonly Color SelectedBorderColor = new Color(1f, 1f, 1f, 0.9f);
        private static readonly Color CanvasBackground = new Color(0.15f, 0.15f, 0.15f, 1f);
        private static readonly Color GridLineColor = new Color(1f, 1f, 1f, 0.04f);

        private readonly List<Upgrade> m_allUpgrades = new(32);
        private readonly Dictionary<Upgrade, Vector2> m_nodePositions = new(32);
        private readonly Dictionary<Upgrade, List<(Upgrade, int)>> m_edges = new(32);

        private Upgrade m_selectedNode = null;
        private Vector2 m_scrollOffset = Vector2.zero;
        private float m_zoom = 1f;
        private bool m_isPanning = false;
        private Vector2 m_lastMousePos;

        private GUIStyle m_nodeLabelStyle;
        private GUIStyle m_nodeSublabelStyle;
        private GUIStyle m_edgeLabelStyle;
        private bool m_stylesInitialised;

        [MenuItem(MenuPath)]
        public static void OpenWindow()
        {
            var window = GetWindow<UpgradeGraphWindow>(WindowTitle);
            window.minSize = new Vector2(600f, 400f);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshGraph();
            wantsMouseMove = true;
        }

        private void OnGUI()
        {
            InitStylesIfNeeded();
            HandleInput();

            var canvasRect = new Rect(0, ToolbarHeight, position.width, position.height - ToolbarHeight);
            DrawCanvas(canvasRect);
            DrawToolbar();
        }
        
        private void DrawToolbar()
        {
            using (new GUILayout.AreaScope(new Rect(0, 0, position.width, ToolbarHeight)))
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
                    RefreshGraph();

                if (GUILayout.Button("Reset View", EditorStyles.toolbarButton, GUILayout.Width(72)))
                    ResetView();

                GUILayout.Space(8);
                GUILayout.Label($"{m_allUpgrades.Count} upgrades", EditorStyles.toolbarButton, GUILayout.Width(90));
                GUILayout.FlexibleSpace();
                GUILayout.Label($"Zoom {m_zoom:P0}", EditorStyles.toolbarButton, GUILayout.Width(72));
            }
        }
        

        private void DrawCanvas(Rect canvasRect)
        {
            GUI.BeginClip(canvasRect);

            EditorGUI.DrawRect(new Rect(0, 0, canvasRect.width, canvasRect.height), CanvasBackground);
            DrawGrid(canvasRect);

            var oldMatrix = GUI.matrix;
            var pivot = new Vector2(canvasRect.width * 0.5f, canvasRect.height * 0.5f);
            GUIUtility.ScaleAroundPivot(Vector2.one * m_zoom, pivot);
            GUI.matrix = Matrix4x4.TRS(m_scrollOffset, Quaternion.identity, Vector3.one) * GUI.matrix;

            if (m_allUpgrades.Count == 0)
            {
                DrawEmptyMessage(canvasRect);
            }
            else
            {
                DrawEdges();
                DrawNodes();
            }

            GUI.matrix = oldMatrix;
            GUI.EndClip();
        }

        private void DrawGrid(Rect canvasRect)
        {
            var gridSpacing = 40f * m_zoom;
            var offsetX = m_scrollOffset.x % gridSpacing;
            var offsetY = m_scrollOffset.y % gridSpacing;

            for (var x = offsetX; x < canvasRect.width; x += gridSpacing)
                EditorGUI.DrawRect(new Rect(x, 0, 1, canvasRect.height), GridLineColor);
            for (var y = offsetY; y < canvasRect.height; y += gridSpacing)
                EditorGUI.DrawRect(new Rect(0, y, canvasRect.width, 1), GridLineColor);
        }

        private void DrawEmptyMessage(Rect canvasRect)
        {
            var centreRect = new Rect(canvasRect.width * 0.5f - 150f, canvasRect.height * 0.5f - 20f, 300f, 40f);
            GUI.Label(centreRect, "No Upgrade assets found. Click Refresh.", EditorStyles.centeredGreyMiniLabel);
        }
        
        private void DrawEdges()
        {
            foreach (var (dependent, deps) in m_edges)
            {
                if (!m_nodePositions.TryGetValue(dependent, out var depPos)) continue;
                var depTop = new Vector3(depPos.x + NodeWidth * 0.5f, depPos.y, 0);

                foreach (var (required, minLevel) in deps)
                {
                    if (!m_nodePositions.TryGetValue(required, out var reqPos)) continue;

                    var reqBottom = new Vector3(reqPos.x + NodeWidth * 0.5f, reqPos.y + NodeHeight, 0);
                    var satisfied = required.CurrentLevel >= minLevel;
                    var edgeColor = satisfied ? EdgeSatisfiedColor : EdgeUnsatisfiedColor;

                    var tangentStrength = Mathf.Abs(depTop.y - reqBottom.y) * 0.5f;
                    var t0 = new Vector3(reqBottom.x, reqBottom.y + tangentStrength, 0);
                    var t1 = new Vector3(depTop.x,    depTop.y    - tangentStrength, 0);

                    Handles.DrawBezier(reqBottom, depTop, t0, t1, edgeColor, null, 2f);
                }
            }
        }

        private void DrawNodes()
        {
            var clickConsumed = false;

            foreach (var upgrade in m_allUpgrades)
            {
                if (!m_nodePositions.TryGetValue(upgrade, out var pos)) continue;

                var nodeRect = new Rect(pos.x, pos.y, NodeWidth, NodeHeight);
                var isSelected = upgrade == m_selectedNode;

                DrawNodeBox(nodeRect, upgrade, isSelected);

                if (clickConsumed || Event.current.type != EventType.MouseDown || !nodeRect.Contains(Event.current.mousePosition)) continue;
                SelectNode(upgrade);
                clickConsumed = true;
                Event.current.Use();
            }
        }

        private void DrawNodeBox(Rect rect, Upgrade upgrade, bool isSelected)
        {
            var fillColor = upgrade switch
            {
                _ when upgrade.CanLevelUp => NodeCanLevelColor,
                _ when upgrade.IsUnlocked && !upgrade.CanLevelUp => NodeMaxedColor,
                _ => NodeLockedColor,
            };

            EditorGUI.DrawRect(rect, fillColor);

            if (isSelected)
            {
                EditorGUI.DrawRect(new Rect(rect.x - 2, rect.y - 2, rect.width + 4, 2), SelectedBorderColor);
                EditorGUI.DrawRect(new Rect(rect.x - 2, rect.yMax, rect.width + 4, 2), SelectedBorderColor);
                EditorGUI.DrawRect(new Rect(rect.x - 2, rect.y - 2, 2, rect.height + 4), SelectedBorderColor);
                EditorGUI.DrawRect(new Rect(rect.xMax, rect.y - 2, 2, rect.height + 4), SelectedBorderColor);
            }

            var nameRect = new Rect(rect.x + 6, rect.y + 6, rect.width - 12, 22f);
            var levelRect = new Rect(rect.x + 6, rect.yMax - 22f, rect.width - 12, 18f);

            GUI.Label(nameRect, upgrade.name, m_nodeLabelStyle);
            GUI.Label(levelRect, $"Level  {upgrade.CurrentLevel} / {upgrade.Count}", m_nodeSublabelStyle);
        }

        private void HandleInput()
        {
            var e = Event.current;

            if (e.type == EventType.ScrollWheel)
            {
                var zoomDelta = -e.delta.y * 0.05f;
                m_zoom = Mathf.Clamp(m_zoom + zoomDelta, MinZoom, MaxZoom);
                e.Use();
                Repaint();
                return;
            }

            var isPanButton = e.button == 2 || e.button == 1;

            if (e.type == EventType.MouseDown && isPanButton)
            {
                m_isPanning = true;
                m_lastMousePos = e.mousePosition;
                e.Use();
                return;
            }

            if (m_isPanning && e.type == EventType.MouseDrag)
            {
                m_scrollOffset += (e.mousePosition - m_lastMousePos) / m_zoom;
                m_lastMousePos = e.mousePosition;
                e.Use();
                Repaint();
                return;
            }

            if (m_isPanning && e.type == EventType.MouseUp)
            {
                m_isPanning = false;
                e.Use();
            }
        }

        private void SelectNode(Upgrade upgrade)
        {
            m_selectedNode = upgrade;
            Selection.activeObject = upgrade;
            EditorGUIUtility.PingObject(upgrade);
            Repaint();
        }

        private void ResetView()
        {
            m_scrollOffset = Vector2.zero;
            m_zoom = 1f;
            Repaint();
        }

        private void RefreshGraph()
        {
            m_allUpgrades.Clear();
            m_edges.Clear();
            m_nodePositions.Clear();

            var guids = AssetDatabase.FindAssets($"t:{nameof(Upgrade)}");
            foreach (var guid in guids)
            {
                var upgrade = AssetDatabase.LoadAssetAtPath<Upgrade>(AssetDatabase.GUIDToAssetPath(guid));
                if (upgrade == null) continue;
                m_allUpgrades.Add(upgrade);
                m_edges[upgrade] = new List<(Upgrade, int)>();
            }

            foreach (var upgrade in m_allUpgrades)
                ReadDependencies(upgrade);

            ComputeLayout();
            Repaint();
        }

        private void ReadDependencies(Upgrade upgrade)
        {
            var dependencies = upgrade.Dependencies;
            var lenght = dependencies.Count;
            
            for (var i = 0; i < lenght; i++)
            {
                var element = dependencies[i];
                var required = element.Upgrade;
                var minLevel = element.MinimalLevel;

                if (required == null || !m_edges.ContainsKey(required)) continue;
                m_edges[upgrade].Add((required, minLevel));
            }
        }

        private void ComputeLayout()
        {
            var depthMap = new Dictionary<Upgrade, int>(m_allUpgrades.Count);

            int ComputeDepth(Upgrade upgrade)
            {
                if (depthMap.TryGetValue(upgrade, out var cached)) return cached;
                depthMap[upgrade] = 0; // guard against cycles
                var maxChildDepth = -1;
                foreach (var (dep, _) in m_edges[upgrade])
                    maxChildDepth = Mathf.Max(maxChildDepth, ComputeDepth(dep));
                var depth = maxChildDepth + 1;
                depthMap[upgrade] = depth;
                return depth;
            }

            foreach (var upgrade in m_allUpgrades)
                ComputeDepth(upgrade);

            var columns = new Dictionary<int, List<Upgrade>>();
            foreach (var upgrade in m_allUpgrades)
            {
                var depth = depthMap[upgrade];
                if (!columns.ContainsKey(depth)) columns[depth] = new List<Upgrade>();
                columns[depth].Add(upgrade);
            }

            const float NodePadding = 16f;

            float IdealX(Upgrade u)
            {
                if (!m_edges.TryGetValue(u, out var deps) || deps.Count == 0) return float.MaxValue;
                var sum = 0f; var count = 0;
                foreach (var (dep, _) in deps)
                    if (m_nodePositions.TryGetValue(dep, out var p)) { sum += p.x + NodeWidth * 0.5f; count++; }
                return count > 0 ? sum / count - NodeWidth * 0.5f : float.MaxValue;
            }

            var sortedDepths = new List<int>(columns.Keys);
            sortedDepths.Sort();

            foreach (var depth in sortedDepths)
            {
                var rowUpgrades = columns[depth];
                rowUpgrades.Sort((a, b) =>
                {
                    var ia = IdealX(a); var ib = IdealX(b);
                    if (ia == float.MaxValue && ib == float.MaxValue)
                        return string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase);
                    return ia.CompareTo(ib);
                });

                var cursor = 20f;
                foreach (var u in rowUpgrades)
                {
                    var ideal = IdealX(u);
                    var x = ideal == float.MaxValue ? cursor : Mathf.Max(cursor, ideal);
                    m_nodePositions[u] = new Vector2(x, depth * RowHeight + 20f);
                    cursor = x + NodeWidth + NodePadding;
                }
            }
        }
        
        private void InitStylesIfNeeded()
        {
            if (m_stylesInitialised) return;

            m_nodeLabelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 11,
                alignment = TextAnchor.UpperLeft,
                wordWrap = true,
            };
            m_nodeLabelStyle.normal.textColor = Color.white;

            m_nodeSublabelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleLeft,
            };
            m_nodeSublabelStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 1f);

            m_edgeLabelStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                fontSize = 9,
                alignment = TextAnchor.MiddleCenter,
            };
            m_edgeLabelStyle.normal.textColor = new Color(0.9f, 0.9f, 0.6f, 1f);

            m_stylesInitialised = true;
        }
    }
}