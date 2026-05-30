using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Windows.View
{
    public abstract class ObjectView<T> : Selectable, IWindowView<T>, ISubmitHandler
    {
        public UnityEvent OnSelected = new UnityEvent();
        public UnityEvent OnDeselected = new UnityEvent();
        public UnityEvent OnClick = new UnityEvent();
        [field: SerializeField] public T Data { get; protected set; }
        
        public virtual void Initialize(T data) => Data = data;

        public virtual void Clear() => Data = default;

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            OnSelected.Invoke();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            OnDeselected.Invoke();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (!IsActive() || !IsInteractable()) return;
            DoStateTransition(SelectionState.Pressed, false);
            OnClick.Invoke();
            StartCoroutine(OnFinishSubmit());
        }
        
        private IEnumerator OnFinishSubmit()
        {
            var fadeTime = colors.fadeDuration;
            var elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            DoStateTransition(currentSelectionState, false);
        }
    }
}