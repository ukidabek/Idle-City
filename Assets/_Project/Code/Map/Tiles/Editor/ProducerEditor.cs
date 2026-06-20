using System.Reflection;
using Project.Resources;
using UnityEditor;
using UnityEngine;

namespace Project.Map
{
    [CustomEditor(typeof(Producer))]
    public class ProducerEditor : DataTileComponentEditor
    {
        private static readonly Color ProduceColor = new Color(0.2f, 0.8f, 0.3f, 0.15f);
        private static readonly Color ConsumeColor = new Color(0.9f, 0.3f, 0.3f, 0.15f);

        private static readonly FieldInfo ProduceHandlerField = typeof(Producer)
            .GetField("m_produceHandler", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo ConsumeHandlerField = typeof(Producer)
            .GetField("m_consumeHandler", BindingFlags.Instance | BindingFlags.NonPublic);

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var producer = (Producer)target;
            if (producer.Data == null) return;

            EditorGUILayout.Space();
            DrawResourceSection("Produces", producer, producer.Data.ResourcesToProduce, ModifierTarget.Production, ProduceColor);
            DrawResourceSection("Consumes", producer, producer.Data.ResourcesToConsume, ModifierTarget.Consumption, ConsumeColor);

            if (Application.isPlaying) Repaint();
        }

        private void DrawResourceSection(string label, Producer producer, ClientResourceInfo[] resources, ModifierTarget target, Color backgroundColor)
        {
            if (resources.Length == 0) return;

            var headerRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight + 4);
            EditorGUI.DrawRect(headerRect, backgroundColor);
            EditorGUI.LabelField(headerRect, label, EditorStyles.boldLabel);

            foreach (var info in resources)
            {
                var rowRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
                EditorGUI.DrawRect(rowRect, backgroundColor * 0.5f);

                var iconRect   = new Rect(rowRect.x + 4,      rowRect.y, 16,                  rowRect.height);
                var nameRect   = new Rect(rowRect.x + 24,     rowRect.y, rowRect.width * 0.5f, rowRect.height);
                var amountRect = new Rect(rowRect.xMax - 120, rowRect.y, 120,                  rowRect.height);

                if (info.Resource?.Image != null)
                    GUI.DrawTexture(iconRect, info.Resource.Image.texture, ScaleMode.ScaleToFit);

                EditorGUI.LabelField(nameRect, info.Resource?.name ?? "None");

                var rightAligned = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight };
                var amountLabel  = Application.isPlaying
                    ? $"{info.Amount:F2}  →  {CalculateEffective(producer, info.Amount, target):F2}"
                    : info.Amount.ToString("F2");

                EditorGUI.LabelField(amountRect, amountLabel, rightAligned);
            }
        }

        private float CalculateEffective(Producer producer, float baseAmount, ModifierTarget target)
        {
            var handlerField  = target == ModifierTarget.Production ? ProduceHandlerField : ConsumeHandlerField;
            var handler       = handlerField?.GetValue(producer) as ModiferHandler;
            var dataEffective = producer.Data.GetEffectiveAmount(baseAmount, target);
            return handler?.Calculate(dataEffective) ?? dataEffective;
        }
    }
}
