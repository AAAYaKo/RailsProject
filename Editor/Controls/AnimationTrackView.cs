using System;
using System.Collections.Generic;
using Rails.Editor.ViewModel;
using Rails.Runtime.Tracks;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class AnimationTrackView : VisualElement
	{
		public const string KeyFrameClass = "key_frame";
		public static readonly BindingId IsKeyFrameProperty = nameof(IsKeyFrame);
		public static readonly BindingId FloatValueProperty = nameof(FloatValue);
		public static readonly BindingId Vector2ValueProperty = nameof(Vector2Value);
		public static readonly BindingId Vector3ValueProperty = nameof(Vector3Value);

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

		public event Action<AnimationTrackView> RemoveClicked;
		public event Action<AnimationTrackView> KeyFrameClicked;
		public event Action<AnimationTrackView, ValueEditArgs> ValueEdited;

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


		public AnimationTrackView()
		{
			if (templateMain == null)
				templateMain = Resources.Load<VisualTreeAsset>("RailsTrack");
			templateMain.CloneTree(this);

			floatField = this.Q<FloatField>("float-value");
			vector2Field = this.Q<Vector2Field>("vector2-value");
			vector3Field = this.Q<Vector3Field>("vector3-value");

			valueViews.Add(AnimationTrack.ValueType.Single, floatField);
			valueViews.Add(AnimationTrack.ValueType.Vector2, vector2Field);
			valueViews.Add(AnimationTrack.ValueType.Vector3, vector3Field);

			this.Query<FloatField>().ForEach(x => x.isDelayed = true);

			keyToggle = this.Q<VisualElement>(className: "rails_key_toggle");

			SetBinding(IsKeyFrameProperty, new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(AnimationTrackViewModel.IsKeyFrame)),
				bindingMode = BindingMode.ToTarget,
			});
			SetBinding(FloatValueProperty, new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(AnimationTrackViewModel.CurrentSingleValue)),
				bindingMode = BindingMode.ToTarget,
			});
			SetBinding(Vector2ValueProperty, new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(AnimationTrackViewModel.CurrentVector2Value)),
				bindingMode = BindingMode.ToTarget,
			});
			SetBinding(Vector3ValueProperty, new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(AnimationTrackViewModel.CurrentVector3Value)),
				bindingMode = BindingMode.ToTarget,
			});

			this.AddManipulator(new ContextualMenuManipulator(x =>
			{
				x.menu.AppendAction("Remove", x =>
				{
					RemoveClicked?.Invoke(this);
				}, DropdownMenuAction.Status.Normal);
			}));
			keyToggle.RegisterCallback<ClickEvent>(x =>
			{
				if (x.button == 0 && x.clickCount == 1)
					KeyFrameClicked?.Invoke(this);
			});

			floatField.RegisterValueChangedCallback(x =>
			{
				ValueEdited?.Invoke(this, new ValueEditArgs(x.newValue));
			});
			vector2Field.RegisterValueChangedCallback(x =>
			{
				ValueEdited?.Invoke(this, new ValueEditArgs(x.newValue));
			});
			vector3Field.RegisterValueChangedCallback(x =>
			{
				ValueEdited.Invoke(this, new ValueEditArgs(x.newValue));
			});
		}
	}
}