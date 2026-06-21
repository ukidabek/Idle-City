using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GameObjectScreenshotWindow : EditorWindow
{
    private int    m_Resolution  = 512;
    private float  m_Padding     = 0.2f;
    private float  m_CameraYaw   = 45f;
    private float  m_CameraPitch = 30f;
    private float  m_FieldOfView = 30f;
    private string m_SavePath    = "Assets/Screenshots";

    private Texture2D m_PreviewTexture = null;
    private bool      m_IsCapturing    = false;

    private const int k_CaptureLayer = 31;

    private static readonly int[]    k_ResolutionOptions = { 128, 256, 512, 1024, 2048 };
    private static readonly string[] k_ResolutionLabels  = { "128", "256", "512", "1024", "2048" };

    [MenuItem("Tools/GameObject Screenshot")]
    public static void Open() =>
        GetWindow<GameObjectScreenshotWindow>("GO Screenshot").minSize = new Vector2(320, 540);

    private void OnGUI()
    {
        EditorGUILayout.Space(8);
        DrawHeader();
        EditorGUILayout.Space(4);
        DrawSettings();
        EditorGUILayout.Space(8);
        DrawCaptureButton();
        EditorGUILayout.Space(8);
        DrawPreview();
    }

    private void OnSelectionChange() => Repaint();

    private void DrawHeader()
    {
        EditorGUILayout.LabelField("GameObject Screenshot", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Captures selected prefabs or scene objects to transparent PNGs.", EditorStyles.miniLabel);
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Flow", EditorStyles.boldLabel);

        var selectedObjects = GetSelectedObjects();
        var helpBoxStyle    = new GUIStyle(EditorStyles.helpBox);

        if (selectedObjects.Count == 0)
        {
            EditorGUILayout.LabelField("— select a prefab or scene GameObject —", helpBoxStyle);
        }
        else if (selectedObjects.Count == 1)
        {
            EditorGUILayout.LabelField(selectedObjects[0].name, helpBoxStyle);
        }
        else
        {
            EditorGUILayout.LabelField($"{selectedObjects[0].name}  (+{selectedObjects.Count - 1} more)", helpBoxStyle);
        }
    }

    private void DrawSettings()
    {
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

        var resolutionIndex = System.Array.IndexOf(k_ResolutionOptions, m_Resolution);
        resolutionIndex = resolutionIndex < 0 ? 2 : resolutionIndex;
        resolutionIndex = EditorGUILayout.Popup("Resolution", resolutionIndex, k_ResolutionLabels);
        m_Resolution    = k_ResolutionOptions[resolutionIndex];

        m_Padding     = EditorGUILayout.Slider("Padding",       m_Padding,     0f,  1f);
        m_CameraYaw   = EditorGUILayout.Slider("Yaw",           m_CameraYaw,   0f,  360f);
        m_CameraPitch = EditorGUILayout.Slider("Pitch",         m_CameraPitch, 0f,  89f);
        m_FieldOfView = EditorGUILayout.Slider("Field Of View", m_FieldOfView, 5f,  90f);

        EditorGUILayout.BeginHorizontal();
        m_SavePath = EditorGUILayout.TextField("Save Path", m_SavePath);
        if (GUILayout.Button("…", GUILayout.Width(28)))
        {
            var pickedPath = EditorUtility.OpenFolderPanel("Save folder", m_SavePath, "");
            if (!string.IsNullOrEmpty(pickedPath))
                m_SavePath = FileUtil.GetProjectRelativePath(pickedPath);
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawCaptureButton()
    {
        var selectedObjects = GetSelectedObjects();

        GUI.enabled = selectedObjects.Count > 0 && !m_IsCapturing;
        if (GUILayout.Button("Preview First Selected", GUILayout.Height(36)))
        {
            var capturedTexture = CaptureToTexture(selectedObjects[0]);
            if (capturedTexture != null)
            {
                if (m_PreviewTexture != null) DestroyImmediate(m_PreviewTexture);
                m_PreviewTexture = capturedTexture;
                Repaint();
            }
        }
        GUI.enabled = true;

        GUI.enabled = selectedObjects.Count > 0 && !m_IsCapturing;
        var saveLabel = selectedObjects.Count > 1 ? $"Save {selectedObjects.Count} PNGs" : "Save PNG";
        if (m_PreviewTexture != null && GUILayout.Button(saveLabel, GUILayout.Height(28)))
            SaveAllPngs(selectedObjects);
        GUI.enabled = true;
    }

    private void DrawPreview()
    {
        if (m_PreviewTexture == null) return;

        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

        var previewRect = GUILayoutUtility.GetAspectRect(1f);
        DrawCheckerboard(previewRect);
        GUI.DrawTexture(previewRect, m_PreviewTexture);
    }

    private static List<GameObject> GetSelectedObjects() =>
        Selection.objects
            .OfType<GameObject>()
            .ToList();

    private Texture2D CaptureToTexture(GameObject sourceGameObject)
    {
        m_IsCapturing = true;

        var isPrefabAsset      = AssetDatabase.Contains(sourceGameObject);
        var instanceGameObject = isPrefabAsset
            ? (GameObject)PrefabUtility.InstantiatePrefab(sourceGameObject)
            : sourceGameObject;

        var originalLayers = SetLayerRecursive(instanceGameObject, k_CaptureLayer);

        try
        {
            var worldBounds = GetWorldBounds(instanceGameObject);
            if (worldBounds.size == Vector3.zero)
            {
                Debug.LogWarning($"[GO Screenshot] '{sourceGameObject.name}' has no renderers — skipped.");
                return null;
            }

            var cameraGameObject = new GameObject("_ScreenshotCam") { hideFlags = HideFlags.HideAndDontSave };
            var screenshotCamera = cameraGameObject.AddComponent<Camera>();

            screenshotCamera.orthographic    = false;
            screenshotCamera.fieldOfView     = m_FieldOfView;
            screenshotCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            screenshotCamera.clearFlags      = CameraClearFlags.SolidColor;
            screenshotCamera.cullingMask     = 1 << k_CaptureLayer;
            screenshotCamera.nearClipPlane   = 0.01f;

            var boundingSphereRadius = worldBounds.extents.magnitude;
            var halfFovRadians       = screenshotCamera.fieldOfView * 0.5f * Mathf.Deg2Rad;
            var cameraDistance       = boundingSphereRadius * (1f + m_Padding) / Mathf.Tan(halfFovRadians);

            var yawRadians      = m_CameraYaw   * Mathf.Deg2Rad;
            var pitchRadians    = m_CameraPitch * Mathf.Deg2Rad;
            var cameraDirection = new Vector3(
                Mathf.Sin(yawRadians) * Mathf.Cos(pitchRadians),
                Mathf.Sin(pitchRadians),
                Mathf.Cos(yawRadians) * Mathf.Cos(pitchRadians));

            cameraGameObject.transform.position = worldBounds.center + cameraDirection * cameraDistance;
            cameraGameObject.transform.LookAt(worldBounds.center, Vector3.up);

            var renderTexture          = new RenderTexture(m_Resolution, m_Resolution, 32, RenderTextureFormat.ARGB32);
            renderTexture.antiAliasing = 4;
            screenshotCamera.targetTexture = renderTexture;
            screenshotCamera.Render();

            RenderTexture.active = renderTexture;
            var capturedTexture  = new Texture2D(m_Resolution, m_Resolution, TextureFormat.ARGB32, false);
            capturedTexture.ReadPixels(new Rect(0, 0, m_Resolution, m_Resolution), 0, 0);
            capturedTexture.Apply();

            RenderTexture.active           = null;
            screenshotCamera.targetTexture = null;
            DestroyImmediate(renderTexture);
            DestroyImmediate(cameraGameObject);

            return capturedTexture;
        }
        finally
        {
            if (isPrefabAsset)
                DestroyImmediate(instanceGameObject);
            else
                RestoreLayersRecursive(instanceGameObject, originalLayers);

            m_IsCapturing = false;
        }
    }

    private void SaveAllPngs(List<GameObject> sourceGameObjects)
    {
        if (!Directory.Exists(m_SavePath))
            Directory.CreateDirectory(m_SavePath);

        var timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");

        foreach (var sourceGameObject in sourceGameObjects)
        {
            var capturedTexture = CaptureToTexture(sourceGameObject);
            if (capturedTexture == null) continue;

            var outputPath = $"{m_SavePath}/{sourceGameObject.name}_{timestamp}.png";
            var pngBytes   = capturedTexture.EncodeToPNG();

            File.WriteAllBytes(outputPath, pngBytes);
            DestroyImmediate(capturedTexture);
            Debug.Log($"[GO Screenshot] Saved → {outputPath}");
        }

        AssetDatabase.Refresh();

        if (sourceGameObjects.Count == 1)
        {
            var savedAsset = AssetDatabase.LoadAssetAtPath<Texture2D>($"{m_SavePath}/{sourceGameObjects[0].name}_{timestamp}.png");
            if (savedAsset != null) EditorGUIUtility.PingObject(savedAsset);
        }
    }

    private static Dictionary<Transform, int> SetLayerRecursive(GameObject gameObject, int layer)
    {
        var originalLayers = new Dictionary<Transform, int>();
        foreach (var transform in gameObject.GetComponentsInChildren<Transform>(true))
        {
            originalLayers[transform]  = transform.gameObject.layer;
            transform.gameObject.layer = layer;
        }
        return originalLayers;
    }

    private static void RestoreLayersRecursive(GameObject gameObject, Dictionary<Transform, int> originalLayers)
    {
        foreach (var transform in gameObject.GetComponentsInChildren<Transform>(true))
        {
            if (originalLayers.TryGetValue(transform, out var originalLayer))
                transform.gameObject.layer = originalLayer;
        }
    }

    private static Bounds GetWorldBounds(GameObject gameObject)
    {
        var renderers = gameObject.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return default;

        var bounds = renderers[0].bounds;
        for (var index = 1; index < renderers.Length; index++)
            bounds.Encapsulate(renderers[index].bounds);
        return bounds;
    }

    private static void DrawCheckerboard(Rect rect)
    {
        var tileSize    = 12;
        var columnCount = Mathf.CeilToInt(rect.width  / tileSize);
        var rowCount    = Mathf.CeilToInt(rect.height / tileSize);

        for (var row = 0; row < rowCount; row++)
        for (var column = 0; column < columnCount; column++)
        {
            var isLight   = (column + row) % 2 == 0;
            var tileColor = isLight ? new Color(0.75f, 0.75f, 0.75f) : new Color(0.55f, 0.55f, 0.55f);
            EditorGUI.DrawRect(
                new Rect(
                    rect.x + column * tileSize,
                    rect.y + row    * tileSize,
                    Mathf.Min(tileSize, rect.xMax - rect.x - column * tileSize),
                    Mathf.Min(tileSize, rect.yMax - rect.y - row    * tileSize)),
                tileColor);
        }
    }
}