using System;
using UnityEditor;
using UnityEngine;

namespace Code.Generator
{
    public class TextureChannelPreview
    {
        private ChannelMask m_channelMask = ChannelMask.All;
        private Texture2D m_previewTexture;
        public Texture2D PreviewTexture => m_previewTexture;

        public void Rebuild(Texture2D source)
        {
            if (source == null) return;

            if (m_previewTexture == null || m_previewTexture.width != source.width || m_previewTexture.height != source.height)
                m_previewTexture = new Texture2D(source.width, source.height, source.format, false);

            var pixels = source.GetPixels();
            for (var i = 0; i < pixels.Length; i++)
            {
                var p = pixels[i];
                pixels[i] = new Color(
                    m_channelMask.HasFlag(ChannelMask.R) ? p.r : 0f,
                    m_channelMask.HasFlag(ChannelMask.G) ? p.g : 0f,
                    m_channelMask.HasFlag(ChannelMask.B) ? p.b : 0f,
                    m_channelMask.HasFlag(ChannelMask.A) ? p.a : 1f
                );
            }

            m_previewTexture.SetPixels(pixels);
            m_previewTexture.Apply();
        }

        public void DrawToggles(Texture2D source, Action onChange)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                DrawToggle("R", ChannelMask.R, new Color(1f, 0.4f, 0.4f), source, onChange);
                DrawToggle("G", ChannelMask.G, new Color(0.4f, 1f, 0.4f), source, onChange);
                DrawToggle("B", ChannelMask.B, new Color(0.4f, 0.6f, 1f), source, onChange);
                DrawToggle("A", ChannelMask.A, Color.white,                source, onChange);
                if (GUILayout.Button("All", GUILayout.Width(36f)))
                {
                    m_channelMask = m_channelMask == ChannelMask.All ? ChannelMask.None : ChannelMask.All;
                    Rebuild(source);
                    onChange?.Invoke();
                }
                GUILayout.FlexibleSpace();
            }
        }

        private void DrawToggle(string label, ChannelMask channel, Color color, Texture2D source, Action onChange)
        {
            var prev = GUI.contentColor;
            GUI.contentColor = m_channelMask.HasFlag(channel) ? color : Color.grey;
            if (GUILayout.Button(label, GUILayout.Width(28f)))
            {
                m_channelMask ^= channel;
                Rebuild(source);
                onChange?.Invoke();
            }
            GUI.contentColor = prev;
        }
    }
}
