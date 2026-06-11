using UnityEngine;

namespace Code.Generator
{
    public class RandomizeAttribute : PropertyAttribute
    {
        public readonly float Min;
        public readonly float Max;

        public RandomizeAttribute(float min = -9999f, float max = 9999f)
        {
            Min = min;
            Max = max;
        }
    }
}
