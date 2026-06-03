using System;
using UnityEngine;

namespace Values
{
	public abstract class BaseValue<T> : ScriptableObject
	{
		public event Action<T> OnValueChanged;

		[SerializeField] protected T m_value;
		public virtual T Value
		{
			get => m_value;
			set
			{
				if (m_value.Equals(value)) return;
				m_value = value;
				OnValueChanged?.Invoke(m_value);
			}
		}
	}
}