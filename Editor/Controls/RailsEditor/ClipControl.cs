using Rails.Editor.Context;
using Rails.Editor.ViewModel;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class ClipControl : VisualElement
	{
		private static readonly BindingId IsPreviewProperty = nameof(IsPreview);
		private static readonly BindingId IsPlayProperty = nameof(IsPlay);
		private static readonly BindingId GotoNextFrameCommandProperty = nameof(GotoNextFrameCommand);

		[UxmlAttribute("can-edit"), CreateProperty]
		public bool CanEdit
		{
			get => canEdit ?? false;
			set
			{
				if (canEdit == value)
					return;
				canEdit = value;
				controls.enabledSelf = value;
			}
		}
		[UxmlAttribute("loop-icon-style"), CreateProperty]
		public string LoopIconStyle
		{
			get => loopIconStyle;
			set
			{
				if (loopIconStyle == value)
					return;
				loopIcon.RemoveFromClassList(loopIconStyle);
				loopIconStyle = value;
				loopIcon.AddToClassList(loopIconStyle);
			}
		}
		[UxmlAttribute("is-preview"), CreateProperty]
		public bool IsPreview
		{
			get => isPreview;
			set
			{
				if (isPreview == value)
					return;
				isPreview = value;
				playControls.enabledSelf = value;
				preview.SetValueWithoutNotify(value);
				NotifyPropertyChanged(IsPreviewProperty);
			}
		}
		[UxmlAttribute("is-play"), CreateProperty]
		public bool IsPlay
		{
			get => isPlay;
			set
			{
				if (isPlay == value)
					return;
				isPlay = value;
				play.SetValueWithoutNotify(value);
				NotifyPropertyChanged(IsPlayProperty);
			}
		}
		[CreateProperty]
		public ICommand GotoNextFrameCommand { get; set; }

		private static VisualTreeAsset templateMain;
		private VisualElement controls;
		private VisualElement playControls;
		private VisualElement loopIcon;
		private Toggle preview;
		private Toggle play;
		private bool? canEdit;
		private string loopIconStyle;
		private bool isPreview;
		private bool isPlay;

		static ClipControl()
		{
			templateMain = Resources.Load<VisualTreeAsset>("RailsClipControl");
		}

		public ClipControl()
		{
			templateMain.CloneTree(this);
			controls = this.Q<VisualElement>("controls");
			playControls = this.Q<VisualElement>("play-controls");
			VisualElement time = controls.Q<VisualElement>("time");
			VisualElement loop = controls.Q<VisualElement>("loop");
			loopIcon = loop.Q<Image>("loop-icon");
			preview = this.Q<Toggle>("preview");
			play = this.Q<Toggle>("play");
			Button next = this.Q<Button>("next");
			RailsClipTimePopupContent timeContent = new();
			RailsClipLoopPopupContent loopContent = new();

			time.RegisterCallback<ClickEvent>(x =>
			{
				if (x is { clickCount: 1, button: 0 })
				{
					timeContent.DataSource = EditorContext.Instance.SelectedClip;
					UnityEditor.PopupWindow.Show(time.worldBound, timeContent);
				}
			});
			loop.RegisterCallback<ClickEvent>(x =>
			{
				if (x is { clickCount: 1, button: 0 })
				{
					loopContent.DataSource = EditorContext.Instance.SelectedClip;
					UnityEditor.PopupWindow.Show(loop.worldBound, loopContent);
				}
			});
			preview.RegisterValueChangedCallback(x =>
			{
				IsPreview = x.newValue;
			});
			play.RegisterValueChangedCallback(x =>
			{
				IsPlay = x.newValue;
			});
			next.clicked += () =>
			{
				GotoNextFrameCommand.Execute();
			};

			SetBinding(GotoNextFrameCommandProperty, new CommandBinding(nameof(RailsClipViewModel.GotoNextFrameCommand)));
		}
	}
}