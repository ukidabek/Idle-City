using System.Collections.Generic;
using UnityEngine;

namespace Code.Generator
{
    public interface INoiseToTextureConverter
    {
        Texture2D Convert(IReadOnlyList<Noise> noises);
    }
}
