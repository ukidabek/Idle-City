using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Code.Generator
{
    [Serializable, Preserve]
    public class SmoothFadeOutNoiseGeneratorStep : INoiseGeneratorStep
    {
        [SerializeField] private AnimationCurve m_fadeOutCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        
        public void Process(Noise noise)
        {
            var size = noise.Size;
            var radiolus = Mathf.Max(size.x, size.y) / 2f;
            var center = size / 2;
            var height = size.y;
            var width = size.x;
            var target = Vector2Int.zero;
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    target.Set(x, y);
                    var distance = Vector2Int.Distance(center, target);
                    var e = Mathf.InverseLerp(0, radiolus, distance);
                    noise[x, y] = Mathf.Lerp(noise.MinValue, noise[x, y], m_fadeOutCurve.Evaluate(e)); 
                }
            }
        }
    }
}