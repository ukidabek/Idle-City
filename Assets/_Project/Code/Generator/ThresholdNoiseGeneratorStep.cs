using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Code.Generator
{
    [Serializable, Preserve]
    public class ThresholdNoiseGeneratorStep : INoiseGeneratorStep
    {
        [SerializeField, Range(0f,1f)] private float m_radius = 0.5f;
        public void Process(Noise noise)
        {
            var radiolus = Mathf.Max(noise.Size.x, noise.Size.y) * m_radius;
            var center = noise.Size / 2;

            var target = Vector2Int.zero;
            var size = noise.Size;
            var height = size.y;
            var width = size.x;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    target.Set(x, y);
                    var distance = Vector2Int.Distance(center, target);
                    if(distance < radiolus) continue;
                    noise[x, y] = noise.MinValue;
                }
            }
        }
    }
}