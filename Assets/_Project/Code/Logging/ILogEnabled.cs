using UnityEngine;

namespace cookie.Logging
{
    public interface ILogEnabled
    {
        Color Color { get; }
        LogMode Mode { get; }
    }
}