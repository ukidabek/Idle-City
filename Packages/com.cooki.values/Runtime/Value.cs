using System;
using UnityEngine;

namespace Values
{
	public abstract class BaseValue<T> : ScriptableObject
	{
		private BulkEditScope<T> m_bulkEditScope = null;
		
		private class BulkEditScope<BulkEditT> : IDisposable
		{
			private readonly BaseValue<BulkEditT>  m_baseValue;
			private BulkEditT m_value;
			
			public BulkEditScope(BaseValue<BulkEditT> baseValue) => m_baseValue = baseValue;

			public void Set(BulkEditT value)
			{
				if (m_baseValue.m_isSilent) return;
				m_baseValue.m_isSilent = true;
				m_value = value;
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

		public IDisposable BulkEdit()
		{
			m_bulkEditScope ??= new BulkEditScope<T>(this);
			m_bulkEditScope.Set(Value);
			return m_bulkEditScope;
		}
	}
}