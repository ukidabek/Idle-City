using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Code.Generator
{
    [Serializable, Preserve]
    public class GradientNoiseToTextureConverter : INoiseToTextureConverter
    {
        [SerializeField] private Color m_colorA = Color.black;
        [SerializeField] private Color m_colorB = Color.white;
        [SerializeField] private string m_textureName = "GeneratedNoise";
        [SerializeField] private FilterMode m_filterMode = FilterMode.Point;
        [SerializeField] private TextureWrapMode m_wrapMode = TextureWrapMode.Clamp;
        [SerializeField] private TextureFormat m_textureFormat = TextureFormat.RGBA32;
        [SerializeField] private bool m_generateMipMaps = false;

        public Texture2D Convert(Noise noise)
        {
            var width = noise.Size.x;
            var height = noise.Size.y;
            var freshCanvas = new Texture2D(width, height, m_textureFormat, m_generateMipMaps)
            {
                name = m_textureName,
                filterMode = m_filterMode,
                wrapMode = m_wrapMode
            };

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var normalized = Mathf.InverseLerp(noise.MinValue, noise.MaxValue, noise[x, y]);
                    freshCanvas.SetPixel(x, y, Color.Lerp(m_colorA, m_colorB, normalized));
                }
            }

            freshCanvas.Apply(m_generateMipMaps);
            return freshCanvas;
        }
    }
}
