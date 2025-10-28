using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine;


//Based on https://gist.github.com/adammyhre/e5318c8c9811264f0cabdd793b796529
namespace Rails.Runtime.Callback
{
	[Serializable]
	public class SerializableEvent : INotifyPropertyChanged, IEquatable<SerializableEvent>
#if UNITY_EDITOR
		, ISerializationCallbackReceiver
#endif
	{
		private static readonly CollectionComparer<SerializableCallback> comparer = new();

		[SerializeField] private List<SerializableCallback> callbacks;

		[CreateProperty]
		public List<SerializableCallback> Callbacks
		{
			get => callbacks;
			set
			{
				if (comparer.Equals(value, callbacks))
					return;
				callbacks = value;
				NotifyPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

#if UNITY_EDITOR
		private List<SerializableCallback> callbacksCopy = new();
#endif


		public void Invoke()
		{
			callbacks.ForEach(x => x.Invoke());
		}

#if UNITY_EDITOR
		public void OnBeforeSerialize()
		{
			callbacksCopy.Clear();
			callbacksCopy.AddRange(callbacks);
		}

		public void OnAfterDeserialize()
		{
			if (!comparer.Equals(callbacks, callbacksCopy))
				NotifyPropertyChanged(nameof(Callbacks));
			callbacksCopy.Clear();
			callbacksCopy.AddRange(callbacks);
		}

		public void Copy(in SerializableEvent other)
		{
			Callbacks.Clear();
			Callbacks.AddRange(other.Callbacks);
		}
#endif
		public override bool Equals(object obj)
		{
			return Equals(obj as SerializableEvent);
		}

		public bool Equals(SerializableEvent other)
		{
			return other is not null && comparer.Equals(Callbacks, other.Callbacks);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Callbacks);
		}

		public static bool operator ==(SerializableEvent left, SerializableEvent right)
		{
			return EqualityComparer<SerializableEvent>.Default.Equals(left, right);
		}

		public static bool operator !=(SerializableEvent left, SerializableEvent right)
		{
			return !(left == right);
		}

		private void NotifyPropertyChanged([CallerMemberName] string property = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
	}
}