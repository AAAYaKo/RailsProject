using System;
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
		[SerializeField] private bool constrainedProportions;

		public RailsEase Ease
		{
			get => ease;
			set => SetProperty(ref ease, value);
		}
		public float SingleValue
		{
			get => singleValue;
			set => SetProperty(ref singleValue, value);
		}
		public Vector3 Vector3Value
		{
			get => vector3Value;
			set => SetProperty(ref vector3Value, value);
		}
		public Vector2 Vector2Value
		{
			get => vector2Value;
			set => SetProperty(ref vector2Value, value);
		}
		public bool ConstrainedProportions
		{
			get => constrainedProportions;
			set => SetProperty(ref constrainedProportions, value);
		}

#if UNITY_EDITOR
		private RailsEase easeCopy;
		private float singleValueCopy;
		private Vector3 vector3ValueCopy;
		private Vector2 vector2ValueCopy;
		private bool constrainedProportionsCopy;
#endif

		public override void OnBeforeSerialize()
		{
#if UNITY_EDITOR
			base.OnBeforeSerialize();
			easeCopy = Ease;
			singleValueCopy = SingleValue;
			vector2ValueCopy = Vector2Value;
			vector3ValueCopy = Vector3Value;
			constrainedProportionsCopy = ConstrainedProportions;
#endif
		}

		public override void OnAfterDeserialize()
		{
#if UNITY_EDITOR
			base.OnAfterDeserialize();
			if (NotifyIfChanged(Ease, easeCopy, nameof(Ease)))
				easeCopy = Ease;
			if (NotifyIfChanged(SingleValue, singleValueCopy, nameof(SingleValue)))
				singleValueCopy = SingleValue;
			if (NotifyIfChanged(Vector3Value, vector3ValueCopy, nameof(Vector3Value)))
				vector3ValueCopy = Vector3Value;
			if (NotifyIfChanged(Vector2Value, vector2ValueCopy, nameof(Vector2Value)))
				vector2ValueCopy = Vector2Value;
			if (NotifyIfChanged(ConstrainedProportions, constrainedProportionsCopy, nameof(ConstrainedProportions)))
				constrainedProportionsCopy = ConstrainedProportions;
#endif
		}
	}
}
