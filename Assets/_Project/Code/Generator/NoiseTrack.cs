using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Generator
{
    [Serializable]
    public class NoiseTrack
    {
        [SerializeReference] private INoiseGeneratorStep[] m_steps = Array.Empty<INoiseGeneratorStep>();
        public IReadOnlyList<INoiseGeneratorStep> Steps => m_steps;
        public Noise Noise { get; private set; }

        public Noise Generate(int size, int seed)
        {
            Noise = new Noise(size, seed);
            foreach (var step in m_steps)
                step?.Process(Noise);
            return Noise;
        }

        public Task<Noise> GenerateAsync(int size, int seed) => !Steps.Any() ? 
            Task.FromResult<Noise>(null) : 
            Task.Run(() => Generate(size, seed));
    }
}
