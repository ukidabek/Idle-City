using UnityEngine;

namespace Code.Generator
{
    public interface INoiseToTextureConverter
    {
        Texture2D Convert(Noise noise);
    }
}