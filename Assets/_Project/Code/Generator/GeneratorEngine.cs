using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Code.Generator
{
    [CreateAssetMenu]
    public class GeneratorEngine : ScriptableObject
    {
        [SerializeField] private int m_seed = 2137;
        [SerializeField] private Size m_size = Size._32;
        [SerializeField] private Texture2D m_texture;
        public Texture2D Texture => m_texture;

        [SerializeReference] private NoiseTrack[] m_tracks = { new NoiseTrack() };
        [SerializeReference] private INoiseToTextureConverter m_converter;
        [SerializeReference] private ITexturePostProcessor[] m_postProcessors;

        public async Awaitable Generate()
        {
            await Awaitable.BackgroundThreadAsync();
            
            var activeTracks = m_tracks.Select(track => track.GenerateAsync((int)m_size, m_seed));
            var channelNoises = await Task.WhenAll(activeTracks);
            
            await Awaitable.MainThreadAsync();
            
            if (m_converter != null)
                m_texture = m_converter.Convert(channelNoises);
            
            if (m_texture == null || m_postProcessors == null) return;
            foreach (var postProcessor in m_postProcessors)
                postProcessor?.PostProcess(m_texture);
        }
    }
    
    public enum NoiseChannel { R = 0, G = 1, B = 2, A = 3 }

    public enum Size
    {
        [InspectorName("32")]    _32    = 1 << 5,
        [InspectorName("64")]    _64    = 1 << 6,
        [InspectorName("128")]   _128   = 1 << 7,
        [InspectorName("256")]   _256   = 1 << 8,
        [InspectorName("512")]   _512   = 1 << 9,
        [InspectorName("1024")]  _1024  = 1 << 10,
        [InspectorName("2048")]  _2048  = 1 << 11,
        [InspectorName("4096")]  _4096  = 1 << 12,
        [InspectorName("8192")]  _8192  = 1 << 13,
    }
}