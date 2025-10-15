using System.ComponentModel;
using Rails.Runtime.Tracks;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.ViewModel
{
	public class AnimationKeyViewModel : BaseKeyViewModel<AnimationKey>
	{
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

		public override string TrackName => Reference == null ? "No Reference" : Reference.name;
		public Object Reference { private get; set; }

		private float? singleValue;
		private Vector2? vector2Value;
		private Vector3? vector3Value;
		private EaseViewModel ease;


		public AnimationKeyViewModel(string trackClass, int keyIndex) : base(trackClass, keyIndex)
		{
		}

		protected override void OnModelChanged()
		{
			if (model == null)
				return;

			TimePosition = new AnimationTime() { Frames = model.TimePosition };
			SingleValue = model.SingleValue;
			Vector2Value = model.Vector2Value;
			Vector3Value = model.Vector3Value;
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnModelPropertyChanged(sender, e);
			if (e.PropertyName == nameof(AnimationKey.SingleValue))
				SingleValue = model.SingleValue;
			else if (e.PropertyName == nameof(AnimationKey.Vector2Value))
				Vector2Value = model.Vector2Value;
			else if (e.PropertyName == nameof(AnimationKey.Vector3Value))
				Vector3Value = model.Vector3Value;
		}

		protected override void OnBind()
		{
			base.OnBind();
			if (Ease == null)
				SetEaseWithoutNotify(new EaseViewModel());
			Ease.BindModel(model.Ease);
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