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
					keyToggle.RemoveFromClassList(trackClass);
				trackClass = value;
				keyToggle.AddToClassList(trackClass);
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

		public event Action<AnimationTrackView> RemoveClicked;
		public event Action<AnimationTrackView> KeyFrameClicked;

		private static VisualTreeAsset templateMain;
		private AnimationTrack.ValueType? type;
		private Dictionary<AnimationTrack.ValueType, VisualElement> valueViews = new();
		private VisualElement keyToggle;
		private string trackClass;
		private bool? isKeyFrame;


		public AnimationTrackView()
		{
			if (templateMain == null)
				templateMain = Resources.Load<VisualTreeAsset>("RailsTrack");
			templateMain.CloneTree(this);

			valueViews.Add(AnimationTrack.ValueType.Single, this.Q<FloatField>("float-value"));
			valueViews.Add(AnimationTrack.ValueType.Vector2, this.Q<Vector2Field>("vector2-value"));
			valueViews.Add(AnimationTrack.ValueType.Vector3, this.Q<Vector3Field>("vector3-value"));
			keyToggle = this.Q<VisualElement>(className: "rails_key_toggle");
			SetBinding(nameof(IsKeyFrame), new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(AnimationTrackViewModel.IsKeyFrame)),
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
		}
	}
}