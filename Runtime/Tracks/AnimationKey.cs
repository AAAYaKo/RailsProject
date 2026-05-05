using System;
using System.Collections.Generic;
using Rails.Runtime.Drivers;
using Unity.Properties;
using UnityEngine;

namespace Rails.Runtime.Tracks
{
	[Serializable]
	public class AnimationKey<T> : BaseKey, IAnimationKey
		where T : struct
	{
		[SerializeField, DontCreateProperty] private RailsEase ease = new();
		[SerializeField, DontCreateProperty] private T animatedValue;
		[SerializeReference, DontCreateProperty] private IRailsDriver<T> driver;

		[SerializeField, DontCreateProperty] private bool constrainedProportions;

		[CreateProperty]
		public RailsEase Ease
		{
			get => ease;
			set
			{
				//if (ease != null)
				//	ease.PropertyChanged -= OnEaseChanged;
				ease = value;
				//if (ease != null)
				//	ease.PropertyChanged += OnEaseChanged;
			}
		}
		[CreateProperty]
		public object Value
		{
			get
			{
				if (driver != null)
					return driver.Value();
				return animatedValue;
			}
			set
			{
				if (value is T newValue)
				{
					if (!EqualityComparer<T>.Default.Equals(animatedValue, newValue) && driver == null)
							animatedValue = newValue;
				}
				else
					throw new InvalidCastException($"Cannot cast {value} to the {typeof(T).Name}");
			}
		}
		[CreateProperty]
		public bool ConstrainedProportions
		{
			get => constrainedProportions;
			set => constrainedProportions = value;
		}
		[CreateProperty]
		public bool HasDriver => driver != null;

		[CreateProperty]
		public IDriver Driver => driver;

		public AnimationKey()
		{
			//ease.PropertyChanged += OnEaseChanged;
		}

		//private void OnEaseChanged(object sender, PropertyChangedEventArgs e)
		//{
		//	NotifyPropertyChanged(nameof(Ease));
		//}
	}

	public interface IAnimationKey : IKey
	{
		public RailsEase Ease { get; set; }
		public object Value { get; set; }
		public bool ConstrainedProportions { get; set; }
		public bool HasDriver { get; }
		public IDriver Driver { get; }
	}
}
