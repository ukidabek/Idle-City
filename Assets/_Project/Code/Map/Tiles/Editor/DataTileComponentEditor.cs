using UnityEditor;
using UnityEngine;

namespace Project.Map
{
    [CustomEditor(typeof(DataTileComponent<>), true)]
    public class DataTileComponentEditor : Editor
    {
        private SerializedProperty m_dataProperty = null;

        private void OnEnable() => m_dataProperty = serializedObject.FindProperty("m_data");

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (m_dataProperty == null || m_dataProperty.objectReferenceValue == null) return;

            var data = m_dataProperty.objectReferenceValue as ScriptableObject;
            if (data == null) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(data.GetType().Name + " Inspector", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var dataEditor = CreateEditor(data);
            var oldIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 1;
            dataEditor.OnInspectorGUI();
            DestroyImmediate(dataEditor);
            EditorGUI.indentLevel = oldIndentLevel;
            EditorGUILayout.EndVertical();
        }
    }
}