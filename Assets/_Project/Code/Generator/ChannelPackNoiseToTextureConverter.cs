using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Code.Generator
{
    [Serializable, Preserve]
    public class ChannelPackNoiseToTextureConverter : INoiseToTextureConverter
    {
        [SerializeField] private string m_textureName = "GeneratedNoise";
        [SerializeField] private FilterMode m_filterMode = FilterMode.Point;
        [SerializeField] private TextureWrapMode m_wrapMode = TextureWrapMode.Clamp;
        [SerializeField] private TextureFormat m_textureFormat = TextureFormat.RGBA32;
        [SerializeField] private bool m_generateMipMaps = false;

        public Texture2D Convert(IReadOnlyList<Noise> noises)
        {
            var firstBorn = FirstNonNull(noises);
            if (firstBorn == null) return null;

            var width = firstBorn.Size.x;
            var height = firstBorn.Size.y;
            var freshCanvas = new Texture2D(width, height, m_textureFormat, m_generateMipMaps)
            {
                name = m_textureName,
                filterMode = m_filterMode,
                wrapMode = m_wrapMode
            };

            var channelCount = noises?.Count ?? 0;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    // Alpha defaults to opaque, the colour channels to zero.
                    var packed = new Color(0f, 0f, 0f, 1f);
                    for (var channel = 0; channel < channelCount && channel < 4; channel++)
                    {
                        var noise = noises[channel];
                        if (noise == null) continue;
                        packed[channel] = Mathf.InverseLerp(noise.MinValue, noise.MaxValue, noise[x, y]);
                    }
                    freshCanvas.SetPixel(x, y, packed);
                }
            }

            freshCanvas.Apply(m_generateMipMaps);
            return freshCanvas;
        }

        private static Noise FirstNonNull(IReadOnlyList<Noise> noises)
        {
            if (noises == null) return null;
            foreach (var noise in noises)
                if (noise != null) return noise;
            return null;
        }
    }
}
