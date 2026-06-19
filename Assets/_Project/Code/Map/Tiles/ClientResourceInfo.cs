using System;
using Project.Resources;
using UnityEngine;

namespace Project.Map
{
    [Serializable]
    public class ClientResourceInfo
    {
        [field: SerializeField] public Resource Resource { get; private set; }
        [field: SerializeField] public float Amount { get; private set; }
    }
}