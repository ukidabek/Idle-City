using UnityEditor;
using UnityEngine;

namespace Values.Editor
{
    [CustomPropertyDrawer(typeof(SmartValue<>), useForChildren: true)]
    public class SmartValuePropertyDrawer : PropertyDrawer
    {
        private const float ModeDropdownWidth   = 70f;
        private const float PingButtonWidth     = 26f;
        private const float RemoteValueWidth    = 80f;
        private const float Spacing             = 4f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var modeProp       = property.FindPropertyRelative("m_mode");
            var localValueProp = property.FindPropertyRelative("m_localValue");
            var remoteProp     = property.FindPropertyRelative("m_remoteValue");

            var isLocal = modeProp.enumValueIndex == (int)SmartValueMode.Local;

            EditorGUI.LabelField(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height), label);

            var contentX     = position.x + EditorGUIUtility.labelWidth + Spacing;
            var contentWidth = position.width - EditorGUIUtility.labelWidth - Spacing;

            if (isLocal)
            {
                // [value field] [dropdown]
                var dropdownX  = position.x + position.width - ModeDropdownWidth;
                var fieldWidth = contentWidth - ModeDropdownWidth - Spacing;

                EditorGUI.PropertyField(
                    new Rect(contentX, position.y, fieldWidth, position.height),
                    localValueProp,
                    GUIContent.none);

                EditorGUI.PropertyField(
                    new Rect(dropdownX, position.y, ModeDropdownWidth, position.height),
                    modeProp,
                    GUIContent.none);
            }
            else
            {
                // [value field] [remote asset ref] [dropdown] [ping]
                var pingX      = position.x + position.width - PingButtonWidth;
                var dropdownX  = pingX - Spacing - ModeDropdownWidth;
                var refX       = dropdownX - Spacing - RemoteValueWidth;
                var fieldWidth = refX - contentX - Spacing;

                EditorGUI.PropertyField(
                    new Rect(contentX, position.y, fieldWidth, position.height),
                    localValueProp,
                    GUIContent.none);

                EditorGUI.PropertyField(
                    new Rect(refX, position.y, RemoteValueWidth, position.height),
                    remoteProp,
                    GUIContent.none);

                EditorGUI.PropertyField(
                    new Rect(dropdownX, position.y, ModeDropdownWidth, position.height),
                    modeProp,
                    GUIContent.none);

                EditorGUI.BeginDisabledGroup(remoteProp.objectReferenceValue == null);
                if (GUI.Button(
                    new Rect(pingX, position.y, PingButtonWidth, position.height),
                    new GUIContent("◎", "Ping asset in Project window"),
                    EditorStyles.miniButton))
                {
                    EditorGUIUtility.PingObject(remoteProp.objectReferenceValue);
                }
                EditorGUI.EndDisabledGroup();

                if (remoteProp.objectReferenceValue != null)
                {
                    var remoteObj = new SerializedObject(remoteProp.objectReferenceValue);
                    var valueProp = remoteObj.FindProperty("m_value");

                    if (valueProp != null)
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUI.PropertyField(
                            new Rect(contentX, position.y, fieldWidth, position.height),
                            valueProp,
                            GUIContent.none);
                        if (EditorGUI.EndChangeCheck())
                            remoteObj.ApplyModifiedProperties();
                    }
                }
                else
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUI.TextField(new Rect(contentX, position.y, fieldWidth, position.height), "—");
                    EditorGUI.EndDisabledGroup();
                }
            }

            EditorGUI.EndProperty();
        }
    }
}