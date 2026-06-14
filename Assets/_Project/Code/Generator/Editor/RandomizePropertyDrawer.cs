using UnityEditor;
using UnityEngine;

namespace Code.Generator
{
    [CustomPropertyDrawer(typeof(RandomizeAttribute))]
    public class RandomizePropertyDrawer : PropertyDrawer
    {
        private const float ButtonWidth = 24f;
        private const float ButtonGap   = 4f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var singleLineHeight = EditorGUIUtility.singleLineHeight;
            var fieldRect = new Rect(position.x, position.y, position.width - ButtonWidth - ButtonGap, singleLineHeight);
            var btnRect   = new Rect(position.xMax - ButtonWidth, position.y, ButtonWidth, singleLineHeight);

            EditorGUI.PropertyField(fieldRect, property, label, true);

            if (GUI.Button(btnRect, "⟳"))
            {
                Randomize(property);
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUI.GetPropertyHeight(property, label, true);

        private void Randomize(SerializedProperty property)
        {
            var attr = (RandomizeAttribute)attribute;
            var min  = attr.Min;
            var max  = attr.Max;
            var iMin = (int)min;
            var iMax = (int)max;
            
            switch (property.propertyType)
            {
                case SerializedPropertyType.Float:
                    property.floatValue = Random.Range(min, max); 
                    break;
                case SerializedPropertyType.Integer:
                    property.intValue = Random.Range(iMin, iMax); 
                    break;
                case SerializedPropertyType.Vector2:
                    property.vector2Value = new Vector2(Random.Range(min, max), Random.Range(min, max)); 
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = new Vector3(Random.Range(min, max), Random.Range(min, max), Random.Range(min, max)); 
                    break;
                case SerializedPropertyType.Vector2Int:
                    property.vector2IntValue = new Vector2Int(Random.Range(iMin, iMax), Random.Range(iMin, iMax)); 
                    break;
                case SerializedPropertyType.Vector3Int:
                    property.vector3IntValue = new Vector3Int(Random.Range(iMin, iMax), Random.Range(iMin, iMax), Random.Range(iMin, iMax)); 
                    break;
                case SerializedPropertyType.Color:
                    property.colorValue = new Color(Random.Range(min, max), Random.Range(min, max), Random.Range(min, max)); 
                    break;
            }
        }
    }
}
