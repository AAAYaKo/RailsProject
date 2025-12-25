using System;
using Rails.Editor.ViewModel;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class KeyInspector : BaseView
	{
		public static readonly BindingId TrackClassProperty = nameof(TrackClass);
		public static readonly BindingId ShowInspectorFoldoutProperty = nameof(ShowInspectorFoldout);
		public static readonly BindingId TimePositionTextProperty = nameof(TimePositionText);

		[UxmlAttribute("track-class"), CreateProperty]
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
		[UxmlAttribute("show-foldout"), CreateProperty]
		public bool ShowInspectorFoldout
		{
			get => showInspectorFoldout ?? false;
			set
			{
				if (showInspectorFoldout == value)
					return;
				showInspectorFoldout = value;
				NotifyPropertyChanged(ShowInspectorFoldoutProperty);
			}
		}
		[UxmlAttribute("time-position"), CreateProperty]
		public string TimePositionText
		{
			get => timePositionText;
			set
			{
				if (timePositionText == value)
					return;
				timePositionText = value;
				if (timeField != null)
				{
					timeField.SetValueWithoutNotify(value);
				}
				NotifyPropertyChanged(TimePositionTextProperty);
			}
		}
		private static readonly VisualTreeAsset template;

		private string trackClass;
		private bool? showInspectorFoldout;
		private new VisualElement contentContainer;
		private VisualElement topContainer;
		private VisualElement contentEvent;
		private VisualElement contentAnimation;
		private TextField timeField;
		private string timePositionText;

		static KeyInspector()
		{
			template = Resources.Load<VisualTreeAsset>("KeyEditor");
		}

		public KeyInspector()
		{
			template.CloneTree(this);
			SetBinding(TrackClassProperty, new ToTargetBinding(nameof(IKeyViewModel.TrackClass)));
			SetBinding(ShowInspectorFoldoutProperty, new TwoWayBinding(nameof(IKeyViewModel.ShowInspectorFoldout)));
			SetBinding(TimePositionTextProperty, new TwoWayBinding(nameof(IKeyViewModel.TimePositionText)));

			topContainer = this.Q<VisualElement>("title");
			contentContainer = this.Q<VisualElement>("content");
			topContainer.RegisterCallback<ClickEvent>(OnClickTop);
		}

		public void ChangeKeyInspector()
		{
			if (dataSource is EventKeyViewModel && contentEvent == null)
			{
				if (contentAnimation != null)
				{
					contentContainer.Remove(contentAnimation);
					contentAnimation = null;
				}
				contentEvent = new EventKeyInspector();
				contentContainer.Add(contentEvent);
			}
			else if (dataSource is AnimationKeyViewModel && contentAnimation == null)
			{
				if (contentEvent != null)
				{
					contentContainer.Remove(contentEvent);
					contentEvent = null;
				}
				contentAnimation = new AnimationKeyInspector();
				contentContainer.Add(contentAnimation);
			}

			timeField?.UnregisterValueChangedCallback(OnTimeFieldValueChanged);
			timeField = contentContainer.Q<TextField>("time");
			if (timeField != null)
			{
				timeField.RegisterValueChangedCallback(OnTimeFieldValueChanged);
				timeField.SetValueWithoutNotify(TimePositionText);
			}
		}

		private void OnTimeFieldValueChanged(ChangeEvent<string> evt)
		{
			TimePositionText = evt.newValue;
		}

		private void OnClickTop(ClickEvent evt)
		{
			if (evt.button == 0)
			{
				ShowInspectorFoldout = !ShowInspectorFoldout;
			}
		}
	}
}