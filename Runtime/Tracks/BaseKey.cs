using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Rails.Runtime
{
	[Serializable]
	public abstract class BaseKey : INotifyPropertyChanged
#if UNITY_EDITOR
		, ISerializationCallbackReceiver
#endif
	{
		[SerializeField] private int timePosition;

		/// <summary>
		/// Time position in frames
		/// </summary>
		public int TimePosition
		{
			get => timePosition;
			set
			{
				SetTimePositionWithoutNotify(value);
				NotifyPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

#if UNITY_EDITOR
		private int timePositionCopy;
#endif


		public void SetTimePositionWithoutNotify(int value)
		{
			if (timePosition == value)
				return;
			timePosition = value;
		}

		protected void NotifyPropertyChanged([CallerMemberName] string property = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}


#if UNITY_EDITOR
		public virtual void OnBeforeSerialize()
		{
			timePositionCopy = TimePosition;
		}

		public virtual void OnAfterDeserialize()
		{
			if (timePositionCopy != TimePosition)
				NotifyPropertyChanged(nameof(TimePosition));

			timePositionCopy = TimePosition;
		}
#endif
	}
}
