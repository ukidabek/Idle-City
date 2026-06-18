using System;
using Project.Resources;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Map
{
    [Serializable]
    public struct Cost
    {
        [field: SerializeField] public Resource Resource { get; set; }

        [field: SerializeField] public float Amount { get; set; }
    }
} 