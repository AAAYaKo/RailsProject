using System.ComponentModel;
using Rails.Runtime.Tracks;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.ViewModel
{
	public class AnimationKeyViewModel : BaseNotifyPropertyViewModel<AnimationKey>
	{
		[CreateProperty]
		public int TimePosition
		{
			get => timePosition ?? 0;
			set => SetProperty(ref timePosition, value);
		}
		[CreateProperty]
		public float SingleValue
		{
			get => singleValue ?? 0;
			set => SetProperty(ref singleValue, value);
		}
		[CreateProperty]
		public Vector2 Vector2Value
		{
			get => vector2Value ?? Vector2.zero;
			set => SetProperty(ref vector2Value, value);
		}
		[CreateProperty]
		public Vector3 Vector3Value
		{
			get => vector3Value ?? Vector3.zero;
			set => SetProperty(ref vector3Value, value);
		}
		[CreateProperty]
		public EaseViewModel Ease
		{
			get => ease;
			set => SetProperty(ease, value, SetEaseWithoutNotify);
		}

		private int? timePosition;
		private float? singleValue;
		private Vector2? vector2Value;
		private Vector3? vector3Value;
		private EaseViewModel ease;

		protected override void OnModelChanged()
		{
			if (model == null)
				return;

			TimePosition = model.TimePosition;
			SingleValue = model.SingleValue;
			Vector2Value = model.Vector2Value;
			Vector3Value = model.Vector3Value;
			if (Ease == null)
				SetEaseWithoutNotify(new EaseViewModel());
			Ease.BindModel(model.Ease);
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(AnimationKey.TimePosition))
				TimePosition = model.TimePosition;
			if (e.PropertyName == nameof(AnimationKey.SingleValue))
				SingleValue = model.SingleValue;
			if (e.PropertyName == nameof(AnimationKey.Vector2Value))
				Vector2Value = model.Vector2Value;
			if (e.PropertyName == nameof(AnimationKey.Vector3Value))
				Vector3Value = model.Vector3Value;
		}

		protected override void OnUnbind()
		{
			base.OnUnbind();
			Ease.UnbindModel();
		}

		private void OnEasePropertyChanged(object sender, BindablePropertyChangedEventArgs e)
		{
			NotifyPropertyChanged(nameof(Ease));
		}

		private void SetEaseWithoutNotify(EaseViewModel value)
		{
			if (ease != null)
				ease.propertyChanged -= OnEasePropertyChanged;
			ease = value;
			ease.propertyChanged += OnEasePropertyChanged;
		}
	}
}