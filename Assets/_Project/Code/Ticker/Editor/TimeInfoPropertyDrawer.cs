using UnityEditor;
using UnityEngine;

namespace Code.Ticker
{
    [CustomPropertyDrawer(typeof(TimeInfo))]
    public class TimeInfoPropertyDrawer : PropertyDrawer
    {
        private static readonly string[] FieldNames =
            { "LastUpdate", "NextUpdate", "DeltaUpdate", "DeltaTime", "TimeScale" };

        private static readonly Color AccentColor     = new Color(0.2f, 0.8f, 1f);
        private static readonly Color BackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.4f);
        private static readonly Color BarTrackColor   = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        private const int AccentBarWidth = 3;
        private const int PanelPadding   = 6;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var rowHeight = EditorGUIUtility.singleLineHeight;
            var spacing   = EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.LabelField(new Rect(position.x, position.y, position.width, rowHeight), label);

            var panelTop    = position.y + rowHeight + spacing;
            var panelHeight = FieldNames.Length * (rowHeight + spacing);
            var panelRect   = new Rect(position.x, panelTop, position.width, panelHeight);
            EditorGUI.DrawRect(panelRect, BackgroundColor);

            var accentRect = new Rect(position.x, panelTop, AccentBarWidth, panelHeight);
            EditorGUI.DrawRect(accentRect, AccentColor);

            var contentX     = position.x + AccentBarWidth + PanelPadding;
            var contentWidth = position.width - AccentBarWidth - PanelPadding * 2;
            var currentY     = panelTop + spacing;

            var valueStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight,
                normal    = { textColor = Color.white }
            };

            foreach (var fieldName in FieldNames)
            {
                var rowRect  = new Rect(contentX, currentY, contentWidth, rowHeight);
                var rawValue = property.FindPropertyRelative(fieldName).floatValue;

                GUI.Label(rowRect, fieldName, EditorStyles.miniLabel);

                if (fieldName == "TimeScale")
                    DrawBar(rowRect, rawValue);
                else
                    GUI.Label(rowRect, rawValue.ToString("F3"), valueStyle);

                currentY += rowHeight + spacing;
            }
        }

        private static void DrawBar(Rect rowRect, float value)
        {
            var trackRect = new Rect(rowRect.x + rowRect.width * 0.45f, rowRect.y + 3f, rowRect.width * 0.55f, rowRect.height - 6f);
            EditorGUI.DrawRect(trackRect, BarTrackColor);

            var fillRect = new Rect(trackRect.x, trackRect.y, trackRect.width * Mathf.Clamp01(value), trackRect.height);
            EditorGUI.DrawRect(fillRect, AccentColor);

            var overlayStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = Color.white }
            };
            GUI.Label(trackRect, value.ToString("F2"), overlayStyle);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var rowHeight = EditorGUIUtility.singleLineHeight;
            var spacing   = EditorGUIUtility.standardVerticalSpacing;
            return rowHeight + spacing + FieldNames.Length * (rowHeight + spacing);
        }
    }
}
