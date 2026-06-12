using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Code.Generator
{
    public enum SelectionMode { NoiseGenerator, Converter, Postprocessor }

    public class GeneratorEngineWindow : EditorWindow, ISearchWindowProvider
    {
        private const int MaxTracks = 4;
        private const float StepBlockWidth = 320f;
        private const float TrackHeaderWidth = 80f;
        private const float TrackRowHeight = 230f;
        private const float ColumnWidth = 300f;
        private const float TextureColumnWidth = 512f;
        private const float ButtonWidth = 22f;
        private const float ArrowWidth = 22f;

        [SerializeField] private GeneratorEngine m_engine;
        [SerializeField] private bool m_locked = false;
        private bool m_generating = false;

        private SerializedObject m_serializedObject;
        private SerializedProperty m_seedProperty;
        private SerializedProperty m_sizeProperty;
        private SerializedProperty m_tracksProperty;
        private SerializedProperty m_textureProperty;
        private SerializedProperty m_converterProperty;
        private SerializedProperty m_postProcessorsProperty;

        private List<SearchTreeEntry> m_noiseGeneratorEntries = new List<SearchTreeEntry>(30);
        private List<SearchTreeEntry> m_converterSearchEntries = new List<SearchTreeEntry>(10);
        private List<SearchTreeEntry> m_postprocessorSearchEntries = new List<SearchTreeEntry>(10);

        private SelectionMode m_selectionMode = SelectionMode.NoiseGenerator;
        private int m_activeTrackIndex = -1;

        private GUIStyle m_arrowStyle;
        private GUIStyle m_headerStyle;
        private Vector2 m_canvasScroll = Vector2.zero;
        private Vector2 m_converterScroll = Vector2.zero;
        private Vector2 m_postProcessorScroll = Vector2.zero;
        private readonly List<Vector2> m_trackScrolls = new List<Vector2>(MaxTracks);
        private float SectionMinHeight = 200;

        public static void Open(GeneratorEngine engine)
        {
            if (engine == null) return;
            var window = GetWindow<GeneratorEngineWindow>("Generator Engine");
            window.Bind(engine);
            window.Show();
        }

        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceId, int line)
        {
            if (EditorUtility.InstanceIDToObject(instanceId) is not GeneratorEngine engine) return false;
            Open(engine);
            return true;
        }

        private void Bind(GeneratorEngine engine)
        {
            m_engine = engine;
            m_serializedObject = new SerializedObject(engine);
            m_seedProperty = m_serializedObject.FindProperty("m_seed");
            m_sizeProperty = m_serializedObject.FindProperty("m_size");
            m_tracksProperty = m_serializedObject.FindProperty("m_tracks");
            m_textureProperty = m_serializedObject.FindProperty("m_texture");
            m_converterProperty = m_serializedObject.FindProperty("m_converter");
            m_postProcessorsProperty = m_serializedObject.FindProperty("m_postProcessors");
            BuildSearchTrees();
        }

        private void EnsureBound()
        {
            if (m_serializedObject != null && m_serializedObject.targetObject == m_engine) return;
            Bind(m_engine);
        }

        private void ShowButton(Rect rect)
        {
            var icon = EditorGUIUtility.IconContent(m_locked ? "IN LockButton on" : "IN LockButton");
            if (!GUI.Button(rect, icon, GUIStyle.none)) return;
            m_locked = !m_locked;
            if (!m_locked && Selection.activeObject is GeneratorEngine freshPick)
            {
                Bind(freshPick);
                Repaint();
            }
        }

        private void OnSelectionChange()
        {
            if (m_locked) return;
            if (Selection.activeObject is not GeneratorEngine freshPick) return;
            Bind(freshPick);
            Repaint();
        }

        private void BuildSearchTrees()
        {
            m_noiseGeneratorEntries = BuildTree<INoiseGeneratorStep>("Generator Steps");
            m_converterSearchEntries = BuildTree<INoiseToTextureConverter>("Noise To Texture Converter");
            m_postprocessorSearchEntries = BuildTree<ITexturePostProcessor>("Postprocessor");
        }

        private static List<SearchTreeEntry> BuildTree<T>(string header)
        {
            var entries = new List<SearchTreeEntry> { new SearchTreeGroupEntry(new GUIContent(header), 0) };
            entries.AddRange(TypeCache.GetTypesDerivedFrom<T>()
                .Where(t => !t.IsInterface && !t.IsAbstract)
                .Select(t => new SearchTreeEntry(new GUIContent(t.Name)) { level = 1, userData = t }));
            return entries;
        }

        private void OnGUI()
        {
            if (m_engine == null)
            {
                EditorGUILayout.HelpBox("No GeneratorEngine bound. Select one and click Open Editor.", MessageType.Info);
                return;
            }

            EnsureBound();
            m_arrowStyle ??= new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
            m_headerStyle ??= new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft };

            m_serializedObject.Update();

            DrawHeader();

            m_canvasScroll = EditorGUILayout.BeginScrollView(m_canvasScroll);
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawTracksPanel();
                DrawArrowV();
                DrawConverterSection();
                DrawArrowV();
                DrawPostProcessorSection();
                DrawArrowV();
                DrawTextureSection();
            }
            EditorGUILayout.EndScrollView();

            m_serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope(GUI.skin.box, GUILayout.ExpandWidth(true)))
            {
                EditorGUILayout.PropertyField(m_seedProperty, GUILayout.Width(220f));
                EditorGUILayout.PropertyField(m_sizeProperty, GUILayout.Width(220f));
            }
            EditorGUILayout.Space(4f);
        }

        private void DrawArrowV() => EditorGUILayout.LabelField("→", m_arrowStyle, GUILayout.Width(20f), GUILayout.ExpandHeight(true));


        private void DrawTracksPanel()
        {
            m_converterScroll = EditorGUILayout.BeginScrollView(m_converterScroll);
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("Tracks", EditorStyles.boldLabel);

                var trackCount = m_tracksProperty.arraySize;
                while (m_trackScrolls.Count < trackCount) m_trackScrolls.Add(Vector2.zero);

                var toDelete = -1;
                for (var i = 0; i < trackCount; i++)
                {
                    using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("-", GUILayout.Width(ButtonWidth))) 
                                toDelete = i;
                            EditorGUILayout.LabelField($"{(NoiseChannel)i}", m_headerStyle, GUILayout.Width(TrackHeaderWidth));
                        }
                        
                        m_trackScrolls[i] = EditorGUILayout.BeginScrollView(m_trackScrolls[i], GUILayout.Height(TrackRowHeight));
                        DrawStepBlocks(m_tracksProperty.GetArrayElementAtIndex(i).FindPropertyRelative("m_steps"), i);
                        
                        EditorGUILayout.EndScrollView();
                    }
                }

                if (toDelete >= 0)
                    m_tracksProperty.DeleteArrayElementAtIndex(toDelete);

                if (trackCount < MaxTracks && GUILayout.Button("+ Track"))
                    AddTrack(trackCount);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawStepBlocks(SerializedProperty stepsProperty, int trackIndex)
        {
            if (stepsProperty == null) return;
            var count = stepsProperty.arraySize;
            if (count == 0) return;

            using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandHeight(true)))
            {
                for (var i = 0; i < count; i++)
                {
                    var step = stepsProperty.GetArrayElementAtIndex(i);
                    using (new EditorGUILayout.VerticalScope(GUI.skin.box, GUILayout.Width(StepBlockWidth), GUILayout.MinHeight(TrackRowHeight - 20)))
                    {
                        DrawManagedReferenceBody(step, false);
                        
                        GUILayout.FlexibleSpace();
                        
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            var layout = GUILayout.Width(ButtonWidth);
                            if (GUILayout.Button("←", layout) && i > 0)
                                stepsProperty.MoveArrayElement(i, i - 1);
                            if (GUILayout.Button("→", layout) && i < count - 1)
                                stepsProperty.MoveArrayElement(i, i + 1);
                            if (GUILayout.Button("✕", layout))
                            {
                                stepsProperty.DeleteArrayElementAtIndex(i);
                                break;
                            }
                        }
                    }

                    if (i < count - 1)
                    {
                        EditorGUILayout.LabelField("→", m_arrowStyle, GUILayout.Width(ArrowWidth), GUILayout.ExpandHeight(true));
                    }
                    else
                    {
                        if (GUILayout.Button("+", GUILayout.Width(ButtonWidth), GUILayout.ExpandHeight(true)))
                            OpenSearch(SelectionMode.NoiseGenerator, trackIndex);
                    }
                }
            }
        }

        private void AddTrack(int index)
        {
            m_tracksProperty.InsertArrayElementAtIndex(index);
            m_tracksProperty.GetArrayElementAtIndex(index).managedReferenceValue = new NoiseTrack();
        }
        
        private void DrawConverterSection()
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.label, GUILayout.Width(ColumnWidth)))
            {
                GUILayout.FlexibleSpace();
                using (new EditorGUILayout.VerticalScope(GUI.skin.box, GUILayout.MinHeight(SectionMinHeight)))
                {
                    EditorGUILayout.LabelField("Noise To Texture Converter", EditorStyles.boldLabel);
                    
                    if (m_converterProperty.managedReferenceValue != null)
                    {
                        using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                using (new EditorGUILayout.VerticalScope())
                                    DrawManagedReferenceBody(m_converterProperty, true);
                            }
                        }
                    }
                    
                    GUILayout.FlexibleSpace();
                    
                    if (GUILayout.Button("Set Converter"))
                        OpenSearch(SelectionMode.Converter, -1);
                }

                GUILayout.FlexibleSpace();
            }
        }
        
        private void DrawPostProcessorSection()
        {
            m_postProcessorScroll = EditorGUILayout.BeginScrollView(m_postProcessorScroll, GUILayout.Width(ColumnWidth));
            using (new EditorGUILayout.VerticalScope(GUI.skin.label))
            {
                GUILayout.FlexibleSpace();
                using (new EditorGUILayout.VerticalScope(GUI.skin.box, GUILayout.MinHeight(SectionMinHeight)))
                {
                    EditorGUILayout.LabelField("Post Processors", EditorStyles.boldLabel);
                    DisplayVerticalList(m_postProcessorsProperty);
                    if (GUILayout.Button("+"))
                        OpenSearch(SelectionMode.Postprocessor, -1);
                }

                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawTextureSection()
        {
            using (new EditorGUILayout.VerticalScope(GUI.skin.label, GUILayout.Width(TextureColumnWidth)))
            {
                GUILayout.FlexibleSpace();
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField("Final Texture", EditorStyles.boldLabel);

                    var texture2D = m_textureProperty.objectReferenceValue as Texture2D;

                    if (texture2D == null) return;
                    var claimedRect = GUILayoutUtility.GetRect(0, TextureColumnWidth, 0, TextureColumnWidth);
                    GUI.DrawTexture(claimedRect, texture2D, ScaleMode.ScaleToFit);
                    EditorGUILayout.LabelField($"{texture2D.width} × {texture2D.height}  ({texture2D.format})", EditorStyles.centeredGreyMiniLabel);

                    using (new EditorGUI.DisabledScope(m_generating))
                    {
                        if (GUILayout.Button("Generate"))
                            _ = RunGenerateAsync();
                    }
                }

                GUILayout.FlexibleSpace();
            }
        }
        

        private async Awaitable RunGenerateAsync()
        {
            m_generating = true;
            Repaint();
            await m_engine.Generate();
            m_serializedObject.Update();
            SaveTextureAsSubAsset();
            m_serializedObject.Update();
            m_generating = false;
            Repaint();
        }

        private void DisplayVerticalList(SerializedProperty listProperty)
        {
            var count = listProperty.arraySize;
            if (count == 0) return;

            for (var i = 0; i < count; i++)
            {
                var element = listProperty.GetArrayElementAtIndex(i);
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUILayout.VerticalScope())
                            DrawManagedReferenceBody(element, false);

                        var layout = GUILayout.Width(ButtonWidth);
                        using (new EditorGUILayout.VerticalScope(layout))
                        {
                            if (GUILayout.Button("↑", layout) && i > 0)
                                listProperty.MoveArrayElement(i, i - 1);
                            if (GUILayout.Button("↓", layout) && i < count - 1)
                                listProperty.MoveArrayElement(i, i + 1);
                            if (GUILayout.Button("✕", layout))
                            {
                                listProperty.DeleteArrayElementAtIndex(i);
                                break;
                            }
                        }
                    }
                }

                if (i < count - 1)
                    EditorGUILayout.LabelField("↓", m_arrowStyle);
            }
        }

        private void DrawManagedReferenceBody(SerializedProperty property, bool showClearButton)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(property.managedReferenceValue?.GetType().Name ?? "null", EditorStyles.boldLabel);
                if (showClearButton && GUILayout.Button("✕", GUILayout.Width(ButtonWidth)))
                {
                    m_converterProperty.managedReferenceValue = null;
                    return;
                }
            }

            var child = property.Copy();
            var end = property.GetEndProperty();
            if (!child.NextVisible(true)) return;

            var prevWideMode = EditorGUIUtility.wideMode;
            EditorGUIUtility.wideMode = true;
            
            while (!SerializedProperty.EqualContents(child, end))
            {
                EditorGUILayout.PropertyField(child, true, GUILayout.ExpandWidth(true));
                if (!child.NextVisible(false)) break;
            }
            EditorGUIUtility.wideMode = prevWideMode;
        }

        private void SaveTextureAsSubAsset()
        {
            var assetPath = AssetDatabase.GetAssetPath(m_engine);
            if (string.IsNullOrEmpty(assetPath)) return;

            var freshBake = m_textureProperty.objectReferenceValue as Texture2D;
            if (freshBake == null) return;

            var stale = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<Texture2D>().FirstOrDefault();
            if (stale != null)
            {
                AssetDatabase.RemoveObjectFromAsset(stale);
                DestroyImmediate(stale, true);
            }

            freshBake.name = "GeneratedTexture";
            AssetDatabase.AddObjectToAsset(freshBake, m_engine);
            EditorUtility.SetDirty(m_engine);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.Default);
        }

        private void OpenSearch(SelectionMode mode, int trackIndex)
        {
            m_selectionMode = mode;
            m_activeTrackIndex = trackIndex;
            SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), this);
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) => m_selectionMode switch
        {
            SelectionMode.NoiseGenerator => m_noiseGeneratorEntries,
            SelectionMode.Converter      => m_converterSearchEntries,
            SelectionMode.Postprocessor  => m_postprocessorSearchEntries,
            _                            => m_noiseGeneratorEntries
        };

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            var type = (Type)entry.userData;
            switch (m_selectionMode)
            {
                case SelectionMode.NoiseGenerator:
                    if (m_activeTrackIndex < 0 || m_activeTrackIndex >= m_tracksProperty.arraySize) return false;
                    var stepsProperty = m_tracksProperty.GetArrayElementAtIndex(m_activeTrackIndex).FindPropertyRelative("m_steps");
                    AddToList(stepsProperty, type);
                    break;
                case SelectionMode.Converter:
                    m_converterProperty.managedReferenceValue = Activator.CreateInstance(type);
                    m_serializedObject.ApplyModifiedProperties();
                    break;
                case SelectionMode.Postprocessor:
                    AddToList(m_postProcessorsProperty, type);
                    break;
            }
            return true;
        }

        private void AddToList(SerializedProperty listProperty, Type type)
        {
            var insertIndex = listProperty.arraySize;
            listProperty.InsertArrayElementAtIndex(insertIndex);
            listProperty.GetArrayElementAtIndex(insertIndex).managedReferenceValue = Activator.CreateInstance(type);
            m_serializedObject.ApplyModifiedProperties();
        }
    }
}
