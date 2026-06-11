using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Code.Generator
{
    [Serializable, Preserve]
    public class PerlinNoiseNoiseGeneratorStep : INoiseGeneratorStep
    {
        [SerializeField, Randomize(-100000, 100000)] private Vector2 m_offset;
        [SerializeField, Min(0f)] private Vector2 m_center = new Vector2(0.5f, 0.5f);
        [SerializeField, Min(float.Epsilon)] private float m_scale = 1f;
        [SerializeField, Min(float.Epsilon)] private float m_frequency = 1f;

        public void Process(Noise noise)
        {
            var size = noise.Size;
            var height = size.y;
            var width = size.x;
            var halfHeight = height * m_center.y;
            var halfWidth = width * m_center.x; 
            
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var sampleX = (x - halfWidth) / m_scale * m_frequency + m_offset.x;
                    var sampleY = (y - halfHeight) / m_scale * m_frequency + m_offset.y;
                    noise[x, y] += Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f;

                    if (noise[x, y] > noise.MaxValue)
                        noise.MaxValue = noise[x, y];
                    else if (noise[x, y] < noise.MinValue)
                        noise.MinValue = noise[x, y];
                }
            }
        }
    }
}