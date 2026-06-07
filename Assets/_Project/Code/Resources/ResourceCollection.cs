using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Project.Resources
{
    [CreateAssetMenu(fileName = "ResourceCollection", menuName = "Resource/ResourceCollection")]
    public class ResourceCollection : ScriptableObject, IReadOnlyList<Resource>
    {
        [SerializeField] private Resource[] m_resources;
        public IEnumerator<Resource> GetEnumerator() => m_resources.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => m_resources.Length;

        public Resource this[int index] => m_resources[index];
    }
}