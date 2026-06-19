using UnityEditor;
using UnityEngine;

namespace Project.Map
{
    [CustomPropertyDrawer(typeof(ClientResourceInfo))]
    public class ClientResourceInfoPropertyDrawer : PropertyDrawer
    {
        private const float AmountWidth = 60f;
        private const float Gap = 4f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var resourceProp = property.FindPropertyRelative("<Resource>k__BackingField");
            var amountProp   = property.FindPropertyRelative("<Amount>k__BackingField");

            EditorGUI.BeginProperty(position, label, property);

            var contentRect = EditorGUI.PrefixLabel(position, label);
            var oldIndent   = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var resourceRect = new Rect(contentRect.x, contentRect.y, contentRect.width - AmountWidth - Gap, contentRect.height);
            var amountRect   = new Rect(contentRect.xMax - AmountWidth, contentRect.y, AmountWidth, contentRect.height);

            EditorGUI.PropertyField(resourceRect, resourceProp, GUIContent.none);
            EditorGUI.PropertyField(amountRect,   amountProp,   GUIContent.none);

            EditorGUI.indentLevel = oldIndent;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight;
    }
}
