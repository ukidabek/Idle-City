using UnityEngine;
using UnityEngine.Events;

namespace Project.Map
{
    public class SimpleOnCoveredEffect : MonoBehaviour, IOnCoveredEffect
    {
        public UnityEvent OnApply =  new UnityEvent();
        public UnityEvent OnUndo =  new UnityEvent();
        
        public void Apply() => OnApply.Invoke();

        public void Undo() => OnUndo.Invoke();
    }
}