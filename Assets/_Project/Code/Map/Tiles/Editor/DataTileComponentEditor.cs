using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Project.Map
{
    [CustomEditor(typeof(TileComponent), true)]
    public class DataTileComponentEditor : Editor
    {
        private SerializedProperty m_dataProperty = null;

        private void OnEnable() => m_dataProperty = FindDataSerializedProperty();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DrawCreateDataButton();

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

        private void DrawCreateDataButton()
        {
            if (m_dataProperty == null) return;

            var dataType = GetDataTypeViaReflection();
            if (dataType == null) return;

            var hasData = m_dataProperty.objectReferenceValue != null;

            using (new EditorGUI.DisabledScope(hasData))
            {
                if (GUILayout.Button($"Create {dataType.Name}"))
                    CreateAndAssignData(dataType);
            }
        }

        private SerializedProperty FindDataSerializedProperty()
        {
            var dataType = GetDataTypeViaReflection();
            if (dataType == null) return null;

            var currentType = target.GetType();
            while (currentType != null && currentType != typeof(MonoBehaviour))
            {
                var fields = currentType.GetFields(
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );

                foreach (var field in fields)
                {
                    if (field.FieldType != dataType) continue;
                    if (!field.IsDefined(typeof(SerializeField), inherit: true) && !field.IsPublic) continue;

                    var property = serializedObject.FindProperty(field.Name);
                    if (property != null) return property;
                }

                currentType = currentType.BaseType;
            }

            return null;
        }

        private Type GetDataTypeViaReflection()
        {
            var componentType = target.GetType();

            foreach (var interfaceType in componentType.GetInterfaces())
            {
                if (!interfaceType.IsGenericType) continue;
                if (interfaceType.GetGenericTypeDefinition() != typeof(IDataTileComponent<>)) continue;

                var genericArguments = interfaceType.GetGenericArguments();
                if (genericArguments.Length == 1)
                    return genericArguments[0];
            }

            return null;
        }

        private void CreateAndAssignData(Type dataType)
        {
            var targetObject = target as MonoBehaviour;
            if (targetObject == null) return;

            var prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(targetObject);
            var folder = string.IsNullOrEmpty(prefabPath)
                ? "Assets"
                : System.IO.Path.GetDirectoryName(prefabPath);

            var assetName = target.name + "_Data";
            var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{assetName}.asset");

            var newData = ScriptableObject.CreateInstance(dataType);
            AssetDatabase.CreateAsset(newData, assetPath);
            AssetDatabase.SaveAssets();

            serializedObject.Update();
            m_dataProperty.objectReferenceValue = newData;
            serializedObject.ApplyModifiedProperties();

            EditorGUIUtility.PingObject(newData);
        }
    }
}