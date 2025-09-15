using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Rails.Runtime.Tracks
{
	[Serializable]
	public class AnimationKey : INotifyPropertyChanged
	{
		[SerializeField] private int timePosition;
		[SerializeField] private RailsEase ease = new();
		[SerializeField] private float singleValue;
		[SerializeField] private Vector3 vector3Value;
		[SerializeField] private Vector2 vector2Value;

		/// <summary>
		/// Time position in frames
		/// </summary>
		public int TimePosition
		{
			get => timePosition;
			set
			{
				if (timePosition == value)
					return;
				timePosition = value;
				NotifyPropertyChanged();
			}
		}
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

		public event PropertyChangedEventHandler PropertyChanged;


		protected void NotifyPropertyChanged([CallerMemberName] string property = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
	}
}
