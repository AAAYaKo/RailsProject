using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;


//Based on https://gist.github.com/adammyhre/e5318c8c9811264f0cabdd793b796529
namespace Rails.Runtime.Callback
{
	[Serializable]
	public class SerializableEvent : BaseSerializableNotifier, IEquatable<SerializableEvent>

	{
		private static readonly CollectionComparer<SerializableCallback> comparer = new();

		[SerializeField] private List<SerializableCallback> callbacks = new();

		[CreateProperty]
		public List<SerializableCallback> Callbacks
		{
			get => callbacks;
			set => SetProperty(ref callbacks, value, comparer);
		}

#if UNITY_EDITOR
		[NonSerialized] private readonly List<SerializableCallback> callbacksCopy = new();
#endif


		public void Invoke()
		{
			callbacks.ForEach(x => x.Invoke());
		}

#if UNITY_EDITOR
		public override void OnBeforeSerialize()
		{
			CopyList(Callbacks, callbacksCopy);
		}

		public override void OnAfterDeserialize()
		{
			if (NotifyIfChanged(Callbacks, callbacksCopy, nameof(Callbacks), comparer))
				CopyList(Callbacks, callbacksCopy);
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
	}
}