using UnityEditor;
using UnityEngine;

namespace Code.Generator
{
    [CustomEditor(typeof(GeneratorEngine))]
    public class GeneratorEngineEditor : Editor
    {
        private SerializedProperty m_textureProperty;
        private readonly TextureChannelPreview m_channelPreview = new();

        private void OnEnable()
        {
            m_textureProperty = serializedObject.FindProperty("m_texture");
            m_channelPreview.Rebuild(m_textureProperty.objectReferenceValue as Texture2D);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var freshBake = m_textureProperty.objectReferenceValue as Texture2D;
            if (freshBake == null) return;


            if (m_channelPreview.PreviewTexture != null)
            {
                var previewRect = GUILayoutUtility.GetAspectRect((float)freshBake.width / freshBake.height);
                GUI.DrawTexture(previewRect, m_channelPreview.PreviewTexture, ScaleMode.ScaleToFit);
            }

            m_channelPreview.DrawToggles(freshBake, Repaint);
            EditorGUILayout.LabelField($"{freshBake.width} × {freshBake.height}  ({freshBake.format})", EditorStyles.centeredGreyMiniLabel);
        }
    }
}
