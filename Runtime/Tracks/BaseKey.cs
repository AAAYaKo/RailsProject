using System;
using UnityEngine;

namespace Rails.Runtime
{
	[Serializable]
	public abstract class BaseKey : BaseSerializableNotifier
	{
		[SerializeField] private int timePosition;

		/// <summary>
		/// Time position in frames
		/// </summary>
		public int TimePosition
		{
			get => timePosition;
			set => SetProperty(ref timePosition, value);
		}

#if UNITY_EDITOR
		[NonSerialized] private int timePositionCopy;
#endif


		public void SetTimePositionWithoutNotify(int value)
		{
			timePosition = value;
		}

		public override void OnBeforeSerialize()
		{
#if UNITY_EDITOR
			timePositionCopy = TimePosition;
#endif
		}

		public override void OnAfterDeserialize()
		{
#if UNITY_EDITOR
			if (NotifyIfChanged(TimePosition, timePositionCopy, nameof(TimePosition)))
				timePositionCopy = TimePosition;
#endif
		}
	}
}
