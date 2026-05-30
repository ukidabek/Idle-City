using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using Utilities.SceneManagement;

namespace Cooki.Flow.General
{
    [Serializable, Preserve]
    public class LoadSceneFlowAction : IFlowAction
    {
        [SerializeField] private SceneProvider m_sceneToLoad = new SceneProvider();
        [SerializeField] private LoadSceneMode m_mode = LoadSceneMode.Additive;
        [SerializeField] private bool m_setSceneAsActive = false;

        public IEnumerator Perform(FlowManager manager)
        {
            var operation = m_sceneToLoad.LoadSceneAsync(m_mode);
            
            while (!operation.isDone)
                yield return null;

            if (!m_setSceneAsActive) yield break;
            
            m_sceneToLoad.SetSceneActive();
        }
    }
}