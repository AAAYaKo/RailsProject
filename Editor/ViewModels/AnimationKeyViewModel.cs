using System.ComponentModel;
using Rails.Editor.Context;
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
		[CreateProperty]
		public bool ConstrainedProportions
		{
			get => constrainedProportions;
			set => SetProperty(ref constrainedProportions, value);
		}
		[CreateProperty]
		public AnimationTrack.ValueType ValueType
		{
			get => valueType ?? AnimationTrack.ValueType.Single;
			set => SetProperty(ref valueType, value);
		}
		[CreateProperty]
		public ICommand<ValueEditArgs> ValueEditCommand
		{
			get => valueEditCommand;
			set => SetProperty(ref valueEditCommand, value);
		}
		[CreateProperty]
		public ICommand<bool> ConstrainedProportionsChangeCommand
		{
			get => constrainedProportionsChangeCommand;
			set => SetProperty(ref constrainedProportionsChangeCommand, value);
		}
		private ICommand<ValueEditArgs> valueEditCommand;
		private ICommand<bool> constrainedProportionsChangeCommand;

		public override string TrackName => Reference == null ? "No Reference" : Reference.name;
		public Object Reference { private get; set; }

		private float? singleValue;
		private Vector2? vector2Value;
		private Vector3? vector3Value;
		private EaseViewModel ease;
		private bool constrainedProportions;
		private AnimationTrack.ValueType? valueType;

		public AnimationKeyViewModel(string trackClass, int keyIndex, ICommand<AnimationTime> moveKeyCommand) : base(trackClass, keyIndex, moveKeyCommand)
		{
			ValueEditCommand = new RelayCommand<ValueEditArgs>(args =>
			{
				EditorContext.Instance.Record("Key Value Changed");
				model.SingleValue = args.SingleValue;
				model.Vector2Value = args.Vector2Value;
				model.Vector3Value = args.Vector3Value;
			});

			ConstrainedProportionsChangeCommand = new RelayCommand<bool>(x =>
			{
				string recordName = x ? "Key Constrained Proportions Enabled" : "Key Constrained Proportions Disabled";
				EditorContext.Instance.Record(recordName);
				model.ConstrainedProportions = x;
			});
		}

		protected override void OnModelChanged()
		{
			if (model == null)
				return;

			TimePosition = new AnimationTime() { Frames = model.TimePosition };
			SingleValue = model.SingleValue;
			Vector2Value = model.Vector2Value;
			Vector3Value = model.Vector3Value;
			ConstrainedProportions = model.ConstrainedProportions;
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnModelPropertyChanged(sender, e);
			bool changed = false;
			if (e.PropertyName == nameof(AnimationKey.SingleValue))
			{
				SingleValue = model.SingleValue;
				changed = true;
			}
			else if (e.PropertyName == nameof(AnimationKey.Vector2Value))
			{
				Vector2Value = model.Vector2Value;
				changed = true;
			}
			else if (e.PropertyName == nameof(AnimationKey.Vector3Value))
			{
				Vector3Value = model.Vector3Value;
				changed = true;
			}
			else if (e.PropertyName == nameof(AnimationKey.ConstrainedProportions))
			{
				ConstrainedProportions = model.ConstrainedProportions;
				changed = true;
			}
			if (changed)
				EventBus.Publish(new ClipChangedEvent());
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