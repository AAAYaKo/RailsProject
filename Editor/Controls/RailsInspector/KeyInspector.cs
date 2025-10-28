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
		private static readonly VisualTreeAsset templateMain;
		private static readonly VisualTreeAsset templateAnimation;

		private string trackClass;
		private bool? showInspectorFoldout;
		private new VisualElement contentContainer;
		private VisualElement contentEvent;
		private VisualElement contentAnimation;
		private TextField timeField;
		private string timePositionText;

		static KeyInspector()
		{
			templateMain = Resources.Load<VisualTreeAsset>("KeyEditor");
			templateAnimation = Resources.Load<VisualTreeAsset>("AnimationKeyEditor");
		}

		public KeyInspector()
		{
			templateMain.CloneTree(this);
			SetBinding(TrackClassProperty, new ToTargetBinding(nameof(IKeyViewModel.TrackClass)));
			SetBinding(ShowInspectorFoldoutProperty, new TwoWayBinding(nameof(IKeyViewModel.ShowInspectorFoldout)));
			SetBinding(TimePositionTextProperty, new TwoWayBinding(nameof(IKeyViewModel.TimePositionText)));

			contentContainer = this.Q<VisualElement>("content");
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
				contentAnimation = templateAnimation.Instantiate();
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
	}
}