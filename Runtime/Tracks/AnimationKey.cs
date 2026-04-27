using System;
using System.ComponentModel;
using UnityEngine;

namespace Rails.Runtime.Tracks
{
	[Serializable]
	public class AnimationKey<T> : BaseKey, IAnimationKey
	{
		[SerializeField] private RailsEase ease = new();
		[SerializeField] private T animatedValue;

		[SerializeField] private bool constrainedProportions;

		public RailsEase Ease
		{
			get => ease;
			set
			{
				if (ease != null)
					ease.PropertyChanged -= OnEaseChanged;
				SetProperty(ref ease, value);
				if (ease != null)
					ease.PropertyChanged += OnEaseChanged;
			}
		}
		public object Value
		{
			get => animatedValue;
			set
			{
				if (value is T newValue)
					SetProperty(ref animatedValue, newValue);
				else
					throw new InvalidCastException($"Cannot cast {value} to the {typeof(T).Name}");
			}
		}
		public bool ConstrainedProportions
		{
			get => constrainedProportions;
			set => SetProperty(ref constrainedProportions, value);
		}

#if UNITY_EDITOR
		[NonSerialized] private RailsEase easeCopy;
		[NonSerialized] private T animatedValueCopy;
		[NonSerialized] private bool constrainedProportionsCopy;
#endif


		public AnimationKey()
		{
			ease.PropertyChanged += OnEaseChanged;
		}

		public override void OnBeforeSerialize()
		{
#if UNITY_EDITOR
			base.OnBeforeSerialize();
			easeCopy = Ease;
			animatedValueCopy = animatedValue;
			constrainedProportionsCopy = ConstrainedProportions;
#endif
		}

		public override void OnAfterDeserialize()
		{
#if UNITY_EDITOR
			base.OnAfterDeserialize();
			if (NotifyIfChanged(Ease, easeCopy, nameof(Ease)))
				easeCopy = Ease;
			if (NotifyIfChanged(animatedValue, animatedValueCopy, nameof(animatedValue)))
				animatedValueCopy = animatedValue;
			if (NotifyIfChanged(ConstrainedProportions, constrainedProportionsCopy, nameof(ConstrainedProportions)))
				constrainedProportionsCopy = ConstrainedProportions;
#endif
		}

		private void OnEaseChanged(object sender, PropertyChangedEventArgs e)
		{
			NotifyPropertyChanged(nameof(Ease));
		}
	}

	public interface IAnimationKey : IKey
	{
		public RailsEase Ease { get; set; }
		public object Value { get; set; }
		public bool ConstrainedProportions { get; set; }
	}
}
