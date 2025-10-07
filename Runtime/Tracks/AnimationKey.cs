using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Rails.Runtime.Tracks
{
	[Serializable]
	public class AnimationKey : BaseKey
	{
		[SerializeField] private RailsEase ease = new();
		[SerializeField] private float singleValue;
		[SerializeField] private Vector3 vector3Value;
		[SerializeField] private Vector2 vector2Value;

		public RailsEase Ease
		{
			get => ease;
			set
			{
				if (ease == value)
					return;
				ease = value;
				NotifyPropertyChanged();
			}
		}
		public float SingleValue
		{
			get => singleValue;
			set
			{
				if (singleValue == value)
					return;
				singleValue = value;
				NotifyPropertyChanged();
			}
		}
		public Vector3 Vector3Value
		{
			get => vector3Value;
			set
			{
				if (vector3Value == value)
					return;
				vector3Value = value;
				NotifyPropertyChanged();
			}
		}
		public Vector2 Vector2Value
		{
			get => vector2Value;
			set
			{
				if (vector2Value == value)
					return;
				vector2Value = value;
				NotifyPropertyChanged();
			}
		}

#if UNITY_EDITOR
		private RailsEase easeCopy;
		private float singleValueCopy;
		private Vector3 vector3ValueCopy;
		private Vector2 vector2ValueCopy;

		public override void OnBeforeSerialize()
		{
			base.OnBeforeSerialize();
			easeCopy = Ease;
			singleValueCopy = SingleValue;
			vector2ValueCopy = Vector2Value;
			vector3ValueCopy = Vector3Value;
		}

		public override void OnAfterDeserialize()
		{
			base.OnAfterDeserialize();
			if (easeCopy != Ease)
				NotifyPropertyChanged(nameof(Ease));
			if (singleValueCopy != SingleValue)
				NotifyPropertyChanged(nameof(SingleValue));
			if (vector2ValueCopy != Vector2Value)
				NotifyPropertyChanged(nameof(Vector2Value));
			if (vector3ValueCopy != Vector3Value)
				NotifyPropertyChanged(nameof(Vector3Value));

			easeCopy = Ease;
			singleValueCopy = SingleValue;
			vector2ValueCopy = Vector2Value;
			vector3ValueCopy = Vector3Value;
		}
#endif
	}
}
