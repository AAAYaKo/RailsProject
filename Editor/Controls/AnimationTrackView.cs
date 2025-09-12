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

		public event Action<AnimationTrackView> RemoveClicked;

		private static VisualTreeAsset templateMain;
		private AnimationTrack.ValueType? type;
		private Dictionary<AnimationTrack.ValueType, VisualElement> valueViews = new();
		private VisualElement keyToggle;
		private string trackClass;


		public AnimationTrackView()
		{
			if (templateMain == null)
				templateMain = Resources.Load<VisualTreeAsset>("RailsTrack");
			templateMain.CloneTree(this);

			valueViews.Add(AnimationTrack.ValueType.Single, this.Q<FloatField>("float-value"));
			valueViews.Add(AnimationTrack.ValueType.Vector2, this.Q<Vector2Field>("vector2-value"));
			valueViews.Add(AnimationTrack.ValueType.Vector3, this.Q<Vector3Field>("vector3-value"));
			keyToggle = this.Q<Toggle>(className:"rails_key_toggle").Q<VisualElement>(className: "unity-toggle__checkmark");
			this.AddManipulator(new ContextualMenuManipulator(x =>
			{
				x.menu.AppendAction("Remove", x =>
				{
					RemoveClicked?.Invoke(this);
				}, DropdownMenuAction.Status.Normal);
			}));
		}
	}
}