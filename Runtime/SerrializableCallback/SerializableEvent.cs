using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;


//Based on https://gist.github.com/adammyhre/e5318c8c9811264f0cabdd793b796529
namespace Rails.Runtime.Callback
{
	[Serializable]
	public class SerializableEvent : IEquatable<SerializableEvent>

	{
		private static readonly CollectionComparer<SerializableCallback> comparer = new();

		[SerializeField, DontCreateProperty] private List<SerializableCallback> callbacks = new();

		[CreateProperty]
		public List<SerializableCallback> Callbacks
		{
			get => callbacks;
			set
			{
				if (!comparer.Equals(callbacks, value))
					callbacks = value;
			}
		}


		public void Invoke()
		{
			callbacks.ForEach(x => x.Invoke());
		}

#if UNITY_EDITOR
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