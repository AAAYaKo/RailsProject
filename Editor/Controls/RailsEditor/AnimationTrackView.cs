using Rails.Editor.ViewModel;
using Unity.Properties;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class AnimationTrackView : BaseView
	{
		public const string KeyFrameClass = "key_frame";
		public static readonly BindingId IsKeyFrameProperty = nameof(IsKeyFrame);
		public static readonly BindingId RemoveCommandProperty = nameof(RemoveCommand);
		public static readonly BindingId KeyFrameAddCommandProperty = nameof(KeyFrameAddCommand);
		public static readonly BindingId KeyFrameRemoveCommandProperty = nameof(KeyFrameRemoveCommand);
		public static readonly BindingId TrackClassProperty = nameof(TrackClass);
		public static readonly BindingId ReferenceProperty = nameof(Reference);
		public static readonly BindingId ChangeReferenceCommandProperty = nameof(ChangeReferenceCommand);

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
		[CreateProperty]
		public Object Reference
		{
			get => reference;
			set
			{
				if (reference == value)
					return;
				reference = value;
				referenceField.SetValueWithoutNotify(value);
			}
		}
		[CreateProperty]
		public ICommand RemoveCommand { get; set; }
		[CreateProperty]
		public ICommand KeyFrameAddCommand { get; set; }
		[CreateProperty]
		public ICommand KeyFrameRemoveCommand { get; set; }
		[CreateProperty]
		public ICommand<Object> ChangeReferenceCommand { get; set; }

		private static VisualTreeAsset templateMain;
		private VisualElement keyToggle;
		private ObjectField referenceField;
		private string trackClass;
		private bool? isKeyFrame;
		private Object reference;

		static AnimationTrackView()
		{
			templateMain = Resources.Load<VisualTreeAsset>("RailsTrack");
		}

		public AnimationTrackView()
		{
			templateMain.CloneTree(this);

			keyToggle = this.Q<VisualElement>(className: "rails_key_toggle");

			SetBinding(IsKeyFrameProperty, new ToTargetBinding(nameof(AnimationTrackViewModel.IsKeyFrame)));
			SetBinding(RemoveCommandProperty, new CommandBinding(nameof(AnimationTrackViewModel.RemoveCommand)));
			SetBinding(KeyFrameAddCommandProperty, new CommandBinding(nameof(AnimationTrackViewModel.KeyFrameAddCommand)));
			SetBinding(KeyFrameRemoveCommandProperty, new CommandBinding(nameof(AnimationTrackViewModel.KeyFrameRemoveCommand)));
			SetBinding(ReferenceProperty, new ToTargetBinding(nameof(AnimationTrackViewModel.Reference)));
			SetBinding(ChangeReferenceCommandProperty, new CommandBinding(nameof(AnimationTrackViewModel.ChangeReferenceCommand)));
			
			AnimationValueControl valueControl = this.Q<AnimationValueControl>();
			valueControl.SetBinding(AnimationValueControl.ValueEditCommandProperty, new CommandBinding(nameof(AnimationTrackViewModel.ValueEditCommand)));
			valueControl.SetBinding(AnimationValueControl.ConstrainedProportionsChangeCommandProperty, new CommandBinding(nameof(AnimationTrackViewModel.ConstrainedProportionsChangeCommand)));

			this.AddManipulator(new ContextualMenuManipulator(x =>
			{
				x.menu.AppendAction("Remove", x =>
				{
					RemoveCommand.Execute();
				}, DropdownMenuAction.Status.Normal);
			}));
			referenceField = this.Q<ObjectField>();
			referenceField.RegisterValueChangedCallback(ReferenceChanged);
		}

		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			keyToggle.RegisterCallback<ClickEvent>(OnKeyClicked);
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			keyToggle.UnregisterCallback<ClickEvent>(OnKeyClicked);
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

		private void ReferenceChanged(ChangeEvent<Object> evt)
		{
			if (!ChangeReferenceCommand.Validate(evt.newValue))
			{
				Debug.LogWarning("Cannot add multiple references to an Object!");
				referenceField.SetValueWithoutNotify(evt.previousValue);
				return;
			}
			ChangeReferenceCommand.Execute(evt.newValue);
		}
	}
}