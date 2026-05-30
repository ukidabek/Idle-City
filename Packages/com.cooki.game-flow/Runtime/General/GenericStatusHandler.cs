using System;
using UnityEngine;
using UnityEngine.Scripting;
using Object = System.Object;

namespace Cooki.Flow.General
{
    [Serializable, Preserve]
    public class GenericStatusHandler : IStatusHandler
    {
        [SerializeField] private UnityEngine.Object m_object = null;
        
        public void SetStatus(bool status)
        {
            switch (m_object)
            {
                case GameObject gameObject:
                    gameObject.SetActive(status);
                    break;
                case Behaviour component:
                    component.enabled = status;
                    break;
                case Rigidbody rigidbody:
                    rigidbody.isKinematic = !status;
                    break;
            }
        }
    }
}