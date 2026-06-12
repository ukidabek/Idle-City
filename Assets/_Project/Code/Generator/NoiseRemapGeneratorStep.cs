using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Code.Generator
{
    [Serializable, Preserve]
    public class NoiseRemapGeneratorStep : INoiseGeneratorStep
    {
        [SerializeField] private AnimationCurve m_map = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        public void Process(Noise noise)
        {
            var size = noise.Size;
            var height = size.y;
            var width = size.x;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var value = Mathf.InverseLerp(noise.MinValue, noise.MaxValue, noise[x, y]);
                    value = m_map.Evaluate(value);
                    noise[x, y] =  Mathf.Lerp(noise.MinValue, noise.MaxValue, value);
                }
            }
        }
    }
}