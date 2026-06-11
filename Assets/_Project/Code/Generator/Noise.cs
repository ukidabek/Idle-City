using UnityEngine;

namespace Code.Generator
{
    public class Noise
    {
        private readonly float[,] m_values;
        public float MinValue = float.MaxValue;
        public float MaxValue  = float.MinValue;

        public readonly Vector2Int Size;
        
        public float this[int x, int y]
        {
            get => m_values[x, y];
            set => m_values[x, y] = value;
        }


        public Noise(int size) : this(size, size)
        {
        }
        
        
        public Noise(int xSize, int ySize)
        {
            Size =  new Vector2Int(xSize, ySize);
            m_values = new float[xSize, ySize];
        }
    }
}