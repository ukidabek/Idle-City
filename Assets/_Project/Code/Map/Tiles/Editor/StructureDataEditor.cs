using System.Linq;
using Project.Resources;
using UnityEditor;
using UnityEngine;

namespace Project.Map
{
    [CustomEditor(typeof(StructureData))]
    public class StructureDataEditor : Editor
    {
        private const int SampleCount = 20;
        private const int AxisSteps = 10;

        private static readonly Color[] m_curveColors =
        {
            Color.cyan, 
            Color.yellow, 
            Color.magenta, 
            Color.green, 
            Color.red
        };

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var data = (StructureData)target;
            DrawCostGraph(data);
        }

        private void DrawCostGraph(StructureData data)
        {
            var costs = data.Costs;
            if (costs == null || costs.Count == 0) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Cost Curve", EditorStyles.boldLabel);

            var canvasRect = GUILayoutUtility.GetRect(0, 160, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(canvasRect, new Color(0.15f, 0.15f, 0.15f));

            var curveSamples = costs.Select(cost => data.SampleCostCurve(SampleCount, cost.Amount)).ToArray();
            var allValues = curveSamples.SelectMany(samples => samples);
            var minCost = allValues.Min();
            var maxCost = Mathf.Max(allValues.Max(), minCost + 0.0001f);

            Handles.BeginGUI();

            Handles.color = new Color(1f, 1f, 1f, 0.1f);
            var axisSteps = AxisSteps - 1;
            for (var i = 1; i <= axisSteps; i++)
            {
                var gridY = canvasRect.yMax - i * (canvasRect.height / AxisSteps);
                Handles.DrawLine(new Vector3(canvasRect.x, gridY), new Vector3(canvasRect.xMax, gridY));
            }

            var values = curveSamples.Select((samples, index) => (samples, index));
            foreach (var samples in values)
            {
                var polylinePoints = new Vector3[SampleCount];
                var delta = maxCost - minCost;
                for (var n = 0; n < SampleCount; n++)
                {
                    var t = n / (float)(SampleCount - 1);
                    var x = canvasRect.x + t * canvasRect.width;
                    var y = canvasRect.yMax - (samples.samples[n] - minCost) / delta * canvasRect.height;
                    polylinePoints[n] = new Vector3(x, y);
                }

                Handles.color = m_curveColors[samples.index % m_curveColors.Length];
                Handles.DrawAAPolyLine(3f, polylinePoints);
            }

            Handles.EndGUI();

            for (var i = 1; i <= axisSteps; i++)
            {
                var value = minCost + i * (maxCost - minCost) / AxisSteps;
                var labelY = canvasRect.yMax - i * (canvasRect.height / AxisSteps);
                var labelRect = new Rect(canvasRect.x + 2, labelY - 8, 60, 16);
                GUI.Label(labelRect, value.Abbreviate(), EditorStyles.miniLabel);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                for (var c = 0; c < costs.Count; c++)
                {
                    var resourceName = costs[c].Resource != null ? costs[c].Resource.name : "Resource";
                    var previousColor = GUI.color;
                    GUI.color = m_curveColors[c % m_curveColors.Length];
                    GUILayout.Label("■", GUILayout.Width(14));
                    GUI.color = previousColor;
                    GUILayout.Label(resourceName, GUILayout.ExpandWidth(false));
                    GUILayout.Space(10);
                }
            }

            EditorGUILayout.LabelField($"n: 0–{SampleCount - 1}", EditorStyles.miniLabel);
        }
    }
}
