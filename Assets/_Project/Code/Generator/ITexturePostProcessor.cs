using UnityEngine;

namespace Code.Generator
{
    public interface ITexturePostProcessor
    {
        void PostProcess(Texture2D texture);
    }
}