using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using Utilities.SceneManagement;

namespace Cooki.Flow.General
{
    [Serializable, Preserve]
    public class UnloadSceneFlowAction : IFlowAction
    {
        [SerializeField] private SceneProvider m_sceneProvider = null;
        
        public IEnumerator Perform(FlowManager manager)
        {
            var operation = m_sceneProvider.UnloadSceneAsync();
            while (!operation.isDone)
                yield return null;
        }
    }
}