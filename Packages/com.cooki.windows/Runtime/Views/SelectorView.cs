using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Profiling;
using Utilities.General;

namespace Windows.View
{
    public class SelectorView : UIBehaviour, IWindowView<IEnumerable<Object>>
    {
        [SerializeReference, Reference] protected IDateInterpreter m_currentDisplay = null;
        [Space]
        [SerializeReference, Reference] protected IDateInterpreter m_previousDisplay = null;
        [SerializeReference, Reference] protected IDateInterpreter m_nextDisplay = null;
        
        private readonly List<Object> m_definitions = new List<Object>(30);
        private Index m_index = null;
        
        public Object Current => m_definitions[m_index];
        
        [Space]
        public UnityEvent<Object> OnSelect =  new UnityEvent<Object>();
        
        public void Initialize(IEnumerable<Object> data)
        {
            m_index ??= new Index(m_definitions);
            m_definitions.AddRange(data);
            UpdateDisplay();
            OnSelect.Invoke(m_definitions[m_index]);
        }

        public void Clear()
        {
            if (m_index != null)
                m_index.Current = 0;
            m_definitions.Clear();
            m_currentDisplay.Clear();
            m_previousDisplay?.Clear();
            m_nextDisplay?.Clear();
        }

        [ContextMenu("Next")]
        public void GoToNext()
        {
            OnSelect.Invoke(m_definitions[m_index++]);
            UpdateDisplay();
        }
        
        [ContextMenu("Previous")]
        public void GoToPrevious()
        {
            OnSelect.Invoke(m_definitions[m_index--]);
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            Profiler.BeginSample("Updating display");
            m_currentDisplay.Display(m_definitions[m_index]);
            m_previousDisplay?.Display(m_definitions[m_index.Previous]);
            m_nextDisplay?.Display(m_definitions[m_index.Previous]);
            Profiler.EndSample();
        }
    }
}