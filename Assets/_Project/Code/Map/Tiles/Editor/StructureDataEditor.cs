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
            Color.red,
            new Color(1f, 0.5f, 0f),       // orange
            new Color(0.5f, 0f, 1f),       // violet
            new Color(0f, 1f, 0.5f),       // mint
            new Color(1f, 0f, 0.5f),       // rose
            new Color(0f, 0.5f, 1f),       // sky blue
            new Color(1f, 1f, 0.5f),       // light yellow
            new Color(0.5f, 1f, 0f),       // lime
            new Color(1f, 0.5f, 0.5f),     // salmon
            new Color(0.5f, 0.5f, 1f),     // lavender
            new Color(0f, 1f, 1f),         // aqua
            new Color(1f, 0.75f, 0f),      // amber
            new Color(0.75f, 0f, 1f),      // purple
            new Color(0f, 0.75f, 0.5f),    // teal
            new Color(1f, 0.25f, 0.75f),   // hot pink
            new Color(0.25f, 0.75f, 1f),   // cornflower
            new Color(0.75f, 1f, 0.25f),   // yellow-green
            new Color(1f, 0.6f, 0.2f),     // peach
            new Color(0.2f, 0.6f, 1f),     // dodger blue
            new Color(0.6f, 1f, 0.6f),     // pale green
            new Color(1f, 0.4f, 0.4f),     // light red
            new Color(0.4f, 0.4f, 1f),     // periwinkle
            new Color(1f, 0.9f, 0.3f),     // gold
            new Color(0.3f, 1f, 0.9f),     // turquoise
            new Color(0.9f, 0.3f, 1f),     // orchid
            new Color(0.6f, 0.8f, 0.2f)    // olive green
        };

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var data = (StructureData)target;
            DrawCostGraph(data);
        }

        private static float[] SampleCostCurve(StructureData data, float baseValue)
        {
            var samples = new float[SampleCount];
            for (var n = 0; n < SampleCount; n++)
                samples[n] = data.CalculateCost(baseValue, n);
            return samples;
        }

        private void DrawCostGraph(StructureData data)
        {
            var costs = data.Costs;
            var costsLength = costs.Length;
            if (costs == null || costsLength == 0) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Cost Curve", EditorStyles.boldLabel);

            var canvasRect = GUILayoutUtility.GetRect(0, 160, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(canvasRect, new Color(0.15f, 0.15f, 0.15f));

            var curveSamples = costs.Select(cost => SampleCostCurve(data, cost.Amount)).ToArray();
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

            var allPolylinePoints = new Vector3[curveSamples.Length][];
            var delta = maxCost - minCost;

            for (var c = 0; c < curveSamples.Length; c++)
            {
                var polylinePoints = new Vector3[SampleCount];
                for (var n = 0; n < SampleCount; n++)
                {
                    var t = n / (float)(SampleCount - 1);
                    var x = canvasRect.x + t * canvasRect.width;
                    var y = canvasRect.yMax - (curveSamples[c][n] - minCost) / delta * canvasRect.height;
                    polylinePoints[n] = new Vector3(x, y);
                }

                allPolylinePoints[c] = polylinePoints;
                Handles.color = m_curveColors[c % m_curveColors.Length];
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

            var xCallouts = new[] { 0, 5, 10, 15, SampleCount - 1 };
            for (var c = 0; c < curveSamples.Length; c++)
            {
                var previousColor = GUI.color;
                GUI.color = m_curveColors[c % m_curveColors.Length];
                foreach (var n in xCallouts)
                {
                    var point = allPolylinePoints[c][n];
                    var labelRect = new Rect(point.x + 2, point.y - 14, 50, 14);
                    GUI.Label(labelRect, curveSamples[c][n].Abbreviate(), EditorStyles.miniLabel);
                }
                GUI.color = previousColor;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                for (var c = 0; c < costsLength; c++)
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

            foreach (var n in xCallouts)
            {
                var t = n / (float)(SampleCount - 1);
                var x = canvasRect.x + t * canvasRect.width;
                var labelRect = new Rect(x - 10, canvasRect.yMax + 2, 20, 14);
                GUI.Label(labelRect, n.ToString(), EditorStyles.miniLabel);
            }
        }
    }
}
