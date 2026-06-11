using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;

namespace Code.Generator
{
    [Serializable, Preserve]
    public class PerlinNoiseNoiseGeneratorStep : INoiseGeneratorStep
    {
        [SerializeField, Min(0f)] private Vector2 m_center = new Vector2(0.5f, 0.5f);
        [SerializeField, Min(float.Epsilon)] private float m_scale = 1f;
        [SerializeField, Min(float.Epsilon)] private float m_frequency = 1f;
        [SerializeField, Min(1)] private int m_octaves = 1;
        [SerializeField, Range(0f, 1f)] private float m_persistence = 0.5f;
        [SerializeField, Min(1f)] private float m_lacunarity = 2f;

        public void Process(Noise noise)
        {
            var size = noise.Size;
            var height = size.y;
            var width = size.x;
            var halfHeight = height * m_center.y;
            var halfWidth = width * m_center.x;

            var random = new System.Random(noise.Seed);
            var octaveOffsets = Enumerable.Range(0, m_octaves)
                .Select(_ => new Vector2(random.Next(-100000, 100000), random.Next(-100000, 100000)))
                .ToArray();
            
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var amplitude = 1f;
                    var frequency = 1f;
                    var value = 0f;
                    var normalization = 0f;

                    for (var o = 0; o < m_octaves; o++)
                    {
                        var sampleX = (x - halfWidth) / m_scale * m_frequency * frequency + octaveOffsets[o].x;
                        var sampleY = (y - halfHeight) / m_scale * m_frequency * frequency + octaveOffsets[o].y;
                        value += (Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f) * amplitude;
                        normalization += amplitude;
                        amplitude *= m_persistence;
                        frequency *= m_lacunarity;
                    }

                    noise[x, y] += value / normalization;

                    if (noise[x, y] > noise.MaxValue)
                        noise.MaxValue = noise[x, y];
                    else if (noise[x, y] < noise.MinValue)
                        noise.MinValue = noise[x, y];
                }
            }
        }
    }
}