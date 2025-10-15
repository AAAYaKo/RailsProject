using System.Collections.Generic;
using Rails.Editor.ViewModel;
using Rails.Runtime.Tracks;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class AnimationTrackView : BaseView
	{
		public const string KeyFrameClass = "key_frame";
		public static readonly BindingId IsKeyFrameProperty = nameof(IsKeyFrame);
		public static readonly BindingId FloatValueProperty = nameof(FloatValue);
		public static readonly BindingId Vector2ValueProperty = nameof(Vector2Value);
		public static readonly BindingId Vector3ValueProperty = nameof(Vector3Value);
		public static readonly BindingId RemoveCommandProperty = nameof(RemoveCommand);
		public static readonly BindingId KeyFrameAddCommandProperty = nameof(KeyFrameAddCommand);
		public static readonly BindingId KeyFrameRemoveCommandProperty = nameof(KeyFrameRemoveCommand);
		public static readonly BindingId ValueEditCommandProperty = nameof(ValueEditCommand);
		public static readonly BindingId TrackClassProperty = nameof(TrackClass);
		public static readonly BindingId ValueTypeProperty = nameof(ValueType);

		[UxmlAttribute("type"), CreateProperty]
		public AnimationTrack.ValueType ValueType
		{
			get => type ?? AnimationTrack.ValueType.Single;
			set
			{
				if (type == value)
					return;

				type = value;
				foreach (var pair in valueViews)
					pair.Value.style.display = (pair.Key == type).ToDisplay();
			}
		}
		[UxmlAttribute("trackClass"), CreateProperty]
		public string TrackClass
		{
			get => trackClass;
			set
			{
				if (trackClass == value)
					return;
				if (!trackClass.IsNullOrEmpty())
					RemoveFromClassList(trackClass);
				trackClass = value;
				AddToClassList(trackClass);
			}
		}
		[UxmlAttribute("isKeyFrame"), CreateProperty]
		public bool IsKeyFrame
		{
			get => isKeyFrame ?? false;
			set
			{
				if (isKeyFrame == value)
					return;
				isKeyFrame = value;
				if (value)
					keyToggle.AddToClassList(KeyFrameClass);
				else
					keyToggle.RemoveFromClassList(KeyFrameClass);
				NotifyPropertyChanged(IsKeyFrameProperty);
			}
		}
		[UxmlAttribute("floatValue"), CreateProperty]
		public float FloatValue
		{
			get => floatValue ?? 0;
			set
			{
				if (floatValue == value)
					return;
				floatValue = value;
				floatField.SetValueWithoutNotify(floatValue.Value);
				NotifyPropertyChanged(FloatValueProperty);
			}
		}
		[UxmlAttribute("vector2Value"), CreateProperty]
		public Vector2 Vector2Value
		{
			get => vector2Value ?? Vector2.zero;
			set
			{
				if (vector2Value == value)
					return;
				vector2Value = value;
				vector2Field.SetValueWithoutNotify(vector2Value.Value);
				NotifyPropertyChanged(Vector2ValueProperty);
			}
		}
		[UxmlAttribute("vector3Value"), CreateProperty]
		public Vector3 Vector3Value
		{
			get => vector3Value ?? Vector3.zero;
			set
			{
				if (vector3Value == value)
					return;
				vector3Value = value;
				vector3Field.SetValueWithoutNotify(vector3Value.Value);
				NotifyPropertyChanged(Vector3ValueProperty);
			}
		}
		[CreateProperty]
		public ICommand RemoveCommand { get; set; }
		[CreateProperty]
		public ICommand KeyFrameAddCommand { get; set; }
		[CreateProperty]
		public ICommand KeyFrameRemoveCommand { get; set; }
		[CreateProperty]
		public ICommand<ValueEditArgs> ValueEditCommand { get; set; }

		private static VisualTreeAsset templateMain;
		private AnimationTrack.ValueType? type;
		private Dictionary<AnimationTrack.ValueType, VisualElement> valueViews = new();
		private FloatField floatField;
		private Vector2Field vector2Field;
		private Vector3Field vector3Field;
		private VisualElement keyToggle;
		private string trackClass;
		private bool? isKeyFrame;
		private float? floatValue;
		private Vector2? vector2Value;
		private Vector3? vector3Value;


		static AnimationTrackView()
		{
			templateMain = Resources.Load<VisualTreeAsset>("RailsTrack");
		}

		public AnimationTrackView()
		{
			templateMain.CloneTree(this);

			floatField = this.Q<FloatField>("float-value");
			vector2Field = this.Q<Vector2Field>("vector2-value");
			vector3Field = this.Q<Vector3Field>("vector3-value");

			valueViews.Add(AnimationTrack.ValueType.Single, floatField);
			valueViews.Add(AnimationTrack.ValueType.Vector2, vector2Field);
			valueViews.Add(AnimationTrack.ValueType.Vector3, vector3Field);

			this.Query<FloatField>().ForEach(x => x.isDelayed = true);

			keyToggle = this.Q<VisualElement>(className: "rails_key_toggle");

			SetBinding(IsKeyFrameProperty, new ToTargetBinding(nameof(AnimationTrackViewModel.IsKeyFrame)));
			SetBinding(FloatValueProperty, new ToTargetBinding(nameof(AnimationTrackViewModel.CurrentSingleValue)));
			SetBinding(Vector2ValueProperty, new ToTargetBinding(nameof(AnimationTrackViewModel.CurrentVector2Value)));
			SetBinding(Vector3ValueProperty, new ToTargetBinding(nameof(AnimationTrackViewModel.CurrentVector3Value)));
			SetBinding(RemoveCommandProperty, new CommandBinding(nameof(AnimationTrackViewModel.RemoveCommand)));
			SetBinding(KeyFrameAddCommandProperty, new CommandBinding(nameof(AnimationTrackViewModel.KeyFrameAddCommand)));
			SetBinding(KeyFrameRemoveCommandProperty, new CommandBinding(nameof(AnimationTrackViewModel.KeyFrameRemoveCommand)));
			SetBinding(ValueEditCommandProperty, new CommandBinding(nameof(AnimationTrackViewModel.ValueEditCommand)));

			this.AddManipulator(new ContextualMenuManipulator(x =>
			{
				x.menu.AppendAction("Remove", x =>
				{
					RemoveCommand.Execute();
				}, DropdownMenuAction.Status.Normal);
			}));
		}

		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			keyToggle.RegisterCallback<ClickEvent>(OnKeyClicked);
			floatField.RegisterValueChangedCallback(OnValueChanged);
			vector2Field.RegisterValueChangedCallback(OnValueChanged);
			vector3Field.RegisterValueChangedCallback(OnValueChanged);
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			keyToggle.UnregisterCallback<ClickEvent>(OnKeyClicked);
			floatField.UnregisterValueChangedCallback(OnValueChanged);
			vector2Field.UnregisterValueChangedCallback(OnValueChanged);
			vector3Field.UnregisterValueChangedCallback(OnValueChanged);
		}

		private void OnKeyClicked(ClickEvent evt)
		{
			if (evt.button == 0 && evt.clickCount == 1)
			{
				if (IsKeyFrame)
					KeyFrameRemoveCommand.Execute();
				else
					KeyFrameAddCommand.Execute();
			}
		}

		private void OnValueChanged(ChangeEvent<float> evt)
		{
			ValueEditCommand.Execute(new ValueEditArgs(evt.newValue));
		}

		private void OnValueChanged(ChangeEvent<Vector2> evt)
		{
			ValueEditCommand.Execute(new ValueEditArgs(evt.newValue));
		}

		private void OnValueChanged(ChangeEvent<Vector3> evt)
		{
			ValueEditCommand.Execute(new ValueEditArgs(evt.newValue));
		}
	}
}