using System;

namespace Code.Ticker
{
    [Serializable]
    public struct TimeInfo
    {
        public float LastUpdate;
        public float NextUpdate;
        public float DeltaUpdate;
        public float DeltaTime;
        public float TimeScale;
    }
}