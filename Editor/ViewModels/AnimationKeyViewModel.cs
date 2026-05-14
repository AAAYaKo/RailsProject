using System.Collections.Generic;
using Rails.Editor.Context;
using Rails.Runtime.Tracks;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.ViewModel
{
	public class AnimationKeyViewModel : BaseKeyViewModel<IAnimationKey>
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
		public IAnimationTrack.ValueType ValueType
		{
			get => valueType ?? IAnimationTrack.ValueType.Single;
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
		[CreateProperty]
		public UnityEngine.Object Reference
		{
			private get => reference;
			set
			{
				if (SetProperty(ref reference, value))
					NotifyPropertyChanged(nameof(TrackName));
			}
		}
		[CreateProperty]
		public bool HasDriver
		{
			get => hasDriver ?? false;
			set => SetProperty(ref hasDriver, value);
		}
		[CreateProperty]
		public SerializedProperty DriverProperty { get; private set; }

		public override string TrackName => Reference == null ? "No Reference" : Reference.name;
		public string TrackProperty
		{
			private get => trackProperty;
			set
			{
				if (trackProperty == value)
					return;
				trackProperty = value;

				string path = $"{trackProperty}.animationKeys.Array.data[{KeyIndex}].driver";

				DriverProperty = EditorContext.Instance.ViewModel.SerializedObject.FindProperty(path);
				NotifyPropertyChanged(nameof(DriverProperty));
			}
		}

		private ICommand<ValueEditArgs> valueEditCommand;
		private ICommand<bool> constrainedProportionsChangeCommand;
		private float? singleValue;
		private Vector2? vector2Value;
		private Vector3? vector3Value;
		private EaseViewModel ease = new();
		private bool constrainedProportions;
		private IAnimationTrack.ValueType? valueType;
		private UnityEngine.Object reference;
		private bool? hasDriver;
		private string trackProperty;

		public AnimationKeyViewModel(int keyIndex, ICommand<AnimationTime> moveKeyCommand) : base(keyIndex, moveKeyCommand)
		{
			ValueEditCommand = new RelayCommand<ValueEditArgs>(args =>
			{
				EditorContext.Instance.Record("Key Value Changed");
				if (ValueType is IAnimationTrack.ValueType.Single)
					model.Value = args.SingleValue;
				else if (ValueType is IAnimationTrack.ValueType.Vector2)
					model.Value = args.Vector2Value;
				else if (ValueType is IAnimationTrack.ValueType.Vector3)
					model.Value = args.Vector3Value;
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

			if (ValueType is IAnimationTrack.ValueType.Single)
				SingleValue = (float)model.Value;
			else if (ValueType is IAnimationTrack.ValueType.Vector2)
				Vector2Value = (Vector2)model.Value;
			else if (ValueType is IAnimationTrack.ValueType.Vector3)
				Vector3Value = (Vector3)model.Value;
			ConstrainedProportions = model.ConstrainedProportions;

			HasDriver = model.HasDriver;
		}

		protected override void OnModelPropertyChanged(PropertyChanged evt)
		{
			if (model?.Driver != null && model.Driver == evt.Sender)
				model.Driver.UpdateValue(Reference);
			if (!EqualityComparer<object>.Default.Equals(model, evt.Sender))
				return;
			OnModelPropertyChanged(evt.Sender, evt.PropertyName);
		}

		protected override void OnModelPropertyChanged(object sender, string propertyName)
		{
			base.OnModelPropertyChanged(sender, propertyName);
			if (propertyName == nameof(IAnimationKey.Value))
			{
				if (model.Value is float valueSingle)
					SingleValue = valueSingle;
				else if (model.Value is Vector2 valueVector2)
					Vector2Value = valueVector2;
				else if (model.Value is Vector3 valueVector3)
					Vector3Value = valueVector3;
			}
			else if (propertyName == nameof(IAnimationKey.ConstrainedProportions))
			{
				ConstrainedProportions = model.ConstrainedProportions;
			}
			else if (propertyName == nameof(IAnimationKey.HasDriver))
			{
				HasDriver = model.HasDriver;
			}
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