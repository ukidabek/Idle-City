using System;
using UnityEngine;

namespace Values
{
	public abstract class BaseValue<T> : ScriptableObject
	{
		private class BulkEditScope<T> : IDisposable
		{
			private readonly BaseValue<T>  m_baseValue;
			private readonly T m_value;
			
			public BulkEditScope(BaseValue<T> baseValue)
			{
				m_baseValue = baseValue;
				m_value = baseValue.Value;
				m_baseValue.m_isSilent = true;
			}

			public void Dispose()
			{
				m_baseValue.m_isSilent = false;
				if (m_baseValue.m_value.Equals(m_value)) return;
				m_baseValue.OnValueChanged.Invoke(m_baseValue.Value);
			}
		}
		
		private bool m_isSilent = false;
		public event Action<T> OnValueChanged;

		[SerializeField] protected T m_value;
		public virtual T Value
		{
			get => m_value;
			set
			{
				if (m_value.Equals(value)) return;
				m_value = value;
				if(m_isSilent) return;
				OnValueChanged?.Invoke(m_value);
			}
		}

		public IDisposable BulkEdit() => new BulkEditScope<T>(this);
	}
}