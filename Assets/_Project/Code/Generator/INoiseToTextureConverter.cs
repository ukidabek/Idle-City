using System.Collections.Generic;
using UnityEngine;

namespace Code.Generator
{
    public interface INoiseToTextureConverter
    {
        // Index of each noise is its texture channel (0 -> R, 1 -> G, 2 -> B, 3 -> A); entries may be null.
        Texture2D Convert(IReadOnlyList<Noise> noises);
    }
}
