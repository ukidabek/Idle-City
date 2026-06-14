using System;
using System.Collections.Generic;
using Code.Generator;
using UnityEngine;
using UnityEngine.Scripting;

namespace Project.Map.Generation
{
    [Serializable, Preserve]
    public class WaterDistancePostProcessor : ITexturePostProcessor
    {
        [SerializeField, Range(0f, 1f)] private float m_waterThreshold = 0.5f;
        [SerializeField] private NoiseChannel m_sourceChannel = NoiseChannel.R;
        [SerializeField] private NoiseChannel m_outputChannel = NoiseChannel.B;

        private readonly Queue<Vector2Int> m_waterTiles = new Queue<Vector2Int>();
        private readonly Vector2Int[] m_offsets = new[]
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
        };

        public void PostProcess(Texture2D texture)
        {
            var width = texture.width;
            var height = texture.height;
            var pixels = texture.GetPixels();
            var distance = new int[width, height];
            var srcIdx = (int)m_sourceChannel;
            var outIdx = (int)m_outputChannel;

            m_waterTiles.Clear();
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    if (pixels[y * width + x][srcIdx] > m_waterThreshold)
                    {
                        distance[x, y] = int.MaxValue;
                        continue;
                    }
                    m_waterTiles.Enqueue(new Vector2Int(x, y));
                    distance[x, y] = 0;
                }
            }

            var maxValue = int.MinValue;
            while (m_waterTiles.Count > 0)
            {
                var cell = m_waterTiles.Dequeue();
                var heare = distance[cell.x, cell.y];
                foreach (var offset in m_offsets)
                {
                    var next = cell + offset;
                    if (next.x < 0 || next.x >= width || next.y < 0 || next.y >= height)
                        continue;
                    var nextValue = heare + 1;
                    if (distance[next.x, next.y] <= nextValue) continue;

                    distance[next.x, next.y] = nextValue;
                    m_waterTiles.Enqueue(next);
                    if (nextValue < maxValue) continue;
                    maxValue = nextValue;
                }
            }

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var i = y * width + x;
                    var c = pixels[i];
                    c[outIdx] = Mathf.InverseLerp(0, maxValue, distance[x, y]);
                    pixels[i] = c;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
        }
    }
}