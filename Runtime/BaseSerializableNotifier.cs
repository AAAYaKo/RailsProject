using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Rails.Runtime
{
	public abstract class BaseSerializableNotifier : INotifyPropertyChanged, ISerializationCallbackReceiver
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public abstract void OnAfterDeserialize();
		public abstract void OnBeforeSerialize();

#nullable enable
		protected bool SetProperty<T>(T oldValue, T newValue, IEqualityComparer<T> comparer, Action<T> setter, [CallerMemberName] string? propertyName = "")
		{
			comparer ??= EqualityComparer<T>.Default;
			if (comparer.Equals(oldValue, newValue))
				return false;
			setter.Invoke(newValue);
			if (!string.IsNullOrEmpty(propertyName))
				NotifyPropertyChanged(propertyName);
			return true;
		}

		protected bool SetProperty<T>(T oldValue, T newValue, Action<T> setter, [CallerMemberName] string? propertyName = "")
		{
			return SetProperty<T>(oldValue, newValue, EqualityComparer<T>.Default, setter, propertyName);
		}

		protected bool SetProperty<T>(ref T field, T value, IEqualityComparer<T> comparer, [CallerMemberName] string? propertyName = "")
		{
			comparer ??= EqualityComparer<T>.Default;
			if (comparer.Equals(field, value))
				return false;
			field = value;
			if (!string.IsNullOrEmpty(propertyName))
				NotifyPropertyChanged(propertyName);
			return true;
		}

		protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = "")
		{
			return SetProperty(ref field, value, EqualityComparer<T>.Default, propertyName);
		}
#nullable disable

		protected void CopyList<T>(List<T> original, List<T> copy)
		{
			copy.Clear();
			copy.AddRange(original);
		}

		protected void CopyArray<T>(T[] original, ref T[] copy)
		{
			if (original == null)
			{
				copy = new T[0];
				return;
			}
			if (copy == null || copy.Length != original.Length)
				copy = new T[original.Length];
			for (int i = 0; i < copy.Length; i++)
				copy[i] = original[i];
		}
		protected bool NotifyIfChanged<T>(in T original, in T copy, string propertyName, IEqualityComparer<T> comparer = null)
		{
			comparer ??= EqualityComparer<T>.Default;
			if (comparer.Equals(copy, original))
				return false;
			NotifyPropertyChanged(propertyName);
			return true;
		}

		protected void NotifyPropertyChanged([CallerMemberName] string property = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
	}
}
