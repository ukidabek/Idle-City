using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Code.Generator
{
    public enum SelectionMode { NoiseGenerator, Converter, Postprocessor }

    [CustomEditor(typeof(GeneratorEngine))]
    public class GeneratorEngineEditor : Editor, ISearchWindowProvider
    {
        private SerializedProperty m_seedProperty;
        private SerializedProperty m_sizeProperty;
        private SerializedProperty m_generatorStepsProperty;
        private SerializedProperty m_textureProperty;
        private SerializedProperty m_converterProperty;
        private SerializedProperty m_postProcessorsProperty;
        private List<SearchTreeEntry> m_noiseGeneratorEntries = new List<SearchTreeEntry>(30);
        private List<SearchTreeEntry> m_converterSearchEntries = new List<SearchTreeEntry>(10);
        private List<SearchTreeEntry> m_postprocessorSearchEntries = new List<SearchTreeEntry>(10);
        private SelectionMode m_selectionMode = SelectionMode.NoiseGenerator;
        private GUIStyle m_arrowStyle = null;
        private Vector2 m_scrollPosition = Vector2.zero;
        private Vector2 m_postProcessorScrollPosition = Vector2.zero;

        private void OnEnable()
        {
            m_seedProperty = serializedObject.FindProperty("m_seed");
            m_sizeProperty = serializedObject.FindProperty("m_size");
            m_generatorStepsProperty = serializedObject.FindProperty("m_generatorSteps");
            m_textureProperty = serializedObject.FindProperty("m_texture");
            m_converterProperty = serializedObject.FindProperty("m_converter");
            m_postProcessorsProperty = serializedObject.FindProperty("m_postProcessors");


            m_noiseGeneratorEntries = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Generator Steps"), 0)
            };

            var types = TypeCache.GetTypesDerivedFrom<INoiseGeneratorStep>().Where(type => !type.IsInterface && !type.IsAbstract);
            m_noiseGeneratorEntries.AddRange(types.Select(type => new SearchTreeEntry(new GUIContent(type.Name))
            {
                level = 1,
                userData = type
            }));

            m_converterSearchEntries = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Noise To Texture Converter"), 0)
            };

            var converterTypes = TypeCache.GetTypesDerivedFrom<INoiseToTextureConverter>().Where(type => !type.IsInterface && !type.IsAbstract);
            m_converterSearchEntries.AddRange(converterTypes.Select(type => new SearchTreeEntry(new GUIContent(type.Name))
            {
                level = 1,
                userData = type
            }));

            m_postprocessorSearchEntries = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Postprocessor"), 0)
            };

            var postProcessorTypes = TypeCache.GetTypesDerivedFrom<ITexturePostProcessor>().Where(type => !type.IsInterface && !type.IsAbstract);
            m_postprocessorSearchEntries.AddRange(postProcessorTypes.Select(type => new SearchTreeEntry(new GUIContent(type.Name))
            {
                level = 1,
                userData = type
            }));
        }

        public override void OnInspectorGUI()
        {
            m_arrowStyle ??= new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
            serializedObject.Update();
            DrawStepsList();
            DrawAddStepSection();
            EditorGUILayout.LabelField("↓", m_arrowStyle);
            DrawConverterSection();
            EditorGUILayout.LabelField("↓", m_arrowStyle);
            DrawPostProcessorSection();
            EditorGUILayout.LabelField("↓", m_arrowStyle);
            DrawGenerateSection();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawStepsList()
        {
            EditorGUILayout.PropertyField(m_seedProperty);
            EditorGUILayout.PropertyField(m_sizeProperty);
            EditorGUILayout.LabelField("Generator Steps", EditorStyles.boldLabel);

            m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
            DisplayList(m_generatorStepsProperty);
            EditorGUILayout.EndScrollView();
        }

        private void DisplayList(SerializedProperty serializedProperty)
        {
            var stepCount = serializedProperty.arraySize;
            if (stepCount == 0) return;
            for (var i = 0; i < stepCount; i++)
            {
                var elementProperty = serializedProperty.GetArrayElementAtIndex(i);

                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        var childProperty = elementProperty.Copy();
                        var endProperty = elementProperty.GetEndProperty();
                        childProperty.NextVisible(true);

                        using (new EditorGUILayout.VerticalScope())
                        {
                            var typeName = elementProperty.managedReferenceValue?.GetType().Name ?? "null";
                            EditorGUILayout.LabelField(typeName, EditorStyles.boldLabel);
                            EditorGUI.indentLevel++;
                            while (!SerializedProperty.EqualContents(childProperty, endProperty))
                            {
                                EditorGUILayout.PropertyField(childProperty, true);
                                childProperty.NextVisible(false);
                            }
                            EditorGUI.indentLevel--;
                        }

                        var layout = GUILayout.Width(24f);
                        using (new EditorGUILayout.VerticalScope(layout))
                        {
                            if (GUILayout.Button("↑", layout) && i > 0)
                                serializedProperty.MoveArrayElement(i, i - 1);

                            if (GUILayout.Button("↓", layout) && i < stepCount - 1)
                                serializedProperty.MoveArrayElement(i, i + 1);

                            if (GUILayout.Button("✕", layout))
                            {
                                serializedProperty.DeleteArrayElementAtIndex(i);
                                break;
                            }
                        }
                    }
                }

                if (i == stepCount - 1) continue;
                EditorGUILayout.LabelField("↓", m_arrowStyle);
            }
        }

        private void DrawAddStepSection()
        {
            EditorGUILayout.Space(4f);

            if (!GUILayout.Button("+")) return;
            m_selectionMode = SelectionMode.NoiseGenerator;
            var context = new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition));
            SearchWindow.Open(context, this);
        }

        private void DrawConverterSection()
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Noise To Texture Converter", EditorStyles.boldLabel);

            if (m_converterProperty.managedReferenceValue != null)
            {
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        var childProperty = m_converterProperty.Copy();
                        var endProperty = m_converterProperty.GetEndProperty();
                        childProperty.NextVisible(true);

                        using (new EditorGUILayout.VerticalScope())
                        {
                            EditorGUILayout.LabelField(m_converterProperty.managedReferenceValue.GetType().Name, EditorStyles.boldLabel);
                            EditorGUI.indentLevel++;
                            while (!SerializedProperty.EqualContents(childProperty, endProperty))
                            {
                                EditorGUILayout.PropertyField(childProperty, true);
                                childProperty.NextVisible(false);
                            }
                            EditorGUI.indentLevel--;
                        }

                        if (GUILayout.Button("✕", GUILayout.Width(24f)))
                            m_converterProperty.managedReferenceValue = null;
                    }
                }
            }

            if (!GUILayout.Button("Set Converter")) return;
            m_selectionMode = SelectionMode.Converter;
            var screenPos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            SearchWindow.Open(new SearchWindowContext(screenPos), this);
        }

        private void DrawPostProcessorSection()
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Post Processors", EditorStyles.boldLabel);

            m_postProcessorScrollPosition = EditorGUILayout.BeginScrollView(m_postProcessorScrollPosition);
            DisplayList(m_postProcessorsProperty);
            EditorGUILayout.EndScrollView();

            if (!GUILayout.Button("+")) return;
            m_selectionMode = SelectionMode.Postprocessor;
            var context = new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition));
            SearchWindow.Open(context, this);
        }

        private void DrawGenerateSection()
        {
            EditorGUILayout.Space(4f);

            var freshBake = m_textureProperty.objectReferenceValue as Texture2D;
            if (freshBake != null)
            {
                EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
                var previewRect = GUILayoutUtility.GetAspectRect((float)freshBake.width / freshBake.height);
                GUI.DrawTexture(previewRect, freshBake, ScaleMode.ScaleToFit);
                EditorGUILayout.LabelField(
                    $"{freshBake.width} × {freshBake.height}  ({freshBake.format})",
                    EditorStyles.centeredGreyMiniLabel);
            }

            if (!GUILayout.Button("Generate")) return;

            var engine = target as GeneratorEngine;
            engine.Generate();
            serializedObject.Update();
            SaveTextureAsSubAsset();
            serializedObject.Update();
            Repaint();
        }

        private void SaveTextureAsSubAsset()
        {
            var assetFilePath = AssetDatabase.GetAssetPath(target);
            if (string.IsNullOrEmpty(assetFilePath)) return;

            var freshBake = m_textureProperty.objectReferenceValue as Texture2D;
            if (freshBake == null) return;

            var stale = AssetDatabase.LoadAllAssetsAtPath(assetFilePath).OfType<Texture2D>().FirstOrDefault();
            if (stale != null)
            {
                AssetDatabase.RemoveObjectFromAsset(stale);
                DestroyImmediate(stale, true);
            }

            freshBake.name = "GeneratedTexture";
            AssetDatabase.AddObjectToAsset(freshBake, target);
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(assetFilePath, ImportAssetOptions.Default);
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) => m_selectionMode switch
        {
            SelectionMode.NoiseGenerator  => m_noiseGeneratorEntries,
            SelectionMode.Converter       => m_converterSearchEntries,
            SelectionMode.Postprocessor   => m_postprocessorSearchEntries
        };

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            var type = (Type)entry.userData;
            switch (m_selectionMode)
            {
                case SelectionMode.NoiseGenerator:
                    AddToList(m_generatorStepsProperty, type);
                    break;
                case SelectionMode.Converter:
                    m_converterProperty.managedReferenceValue = Activator.CreateInstance(type);
                    serializedObject.ApplyModifiedProperties();
                    break;
                case SelectionMode.Postprocessor:
                    AddToList(m_postProcessorsProperty, type);
                    break;
            }
            return true;
        }

        private void AddToList(SerializedProperty listProperty, Type type)
        {
            var freshling = Activator.CreateInstance(type);
            var insertIndex = listProperty.arraySize;
            listProperty.InsertArrayElementAtIndex(insertIndex);
            listProperty.GetArrayElementAtIndex(insertIndex).managedReferenceValue = freshling;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
