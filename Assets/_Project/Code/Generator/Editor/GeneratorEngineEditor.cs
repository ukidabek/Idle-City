using UnityEditor;
using UnityEngine;

namespace Code.Generator
{
    [CustomEditor(typeof(GeneratorEngine))]
    public class GeneratorEngineEditor : Editor
    {
        private SerializedProperty m_textureProperty;

        private void OnEnable() =>
            m_textureProperty = serializedObject.FindProperty("m_texture");

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Editor"))
                GeneratorEngineWindow.Open(target as GeneratorEngine);

            serializedObject.Update();
            var freshBake = m_textureProperty.objectReferenceValue as Texture2D;
            if (freshBake == null) return;

            var previewRect = GUILayoutUtility.GetAspectRect((float)freshBake.width / freshBake.height);
            GUI.DrawTexture(previewRect, freshBake, ScaleMode.ScaleToFit);
            EditorGUILayout.LabelField(
                $"{freshBake.width} × {freshBake.height}  ({freshBake.format})",
                EditorStyles.centeredGreyMiniLabel);
        }
    }
}
