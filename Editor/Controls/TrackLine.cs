using System;
using Rails.Editor.ViewModel;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class TrackLine : VisualElement
	{



		public TrackLine()
		{
			style.flexGrow = 0;
			style.flexShrink = 0;
		}
	}

	[UxmlElement]
	public partial class TrackView : VisualElement
	{
		private const int additional = 30;
		public static readonly BindingId DurationProperty = nameof(Duration);

		[UxmlAttribute("duration"), CreateProperty]
		public int Duration
		{
			get => duration;
			set
			{
				if (duration == value)
					return;
				duration = value;
				tracksContainer.style.width = duration * EditorContext.Instance.FramePixelSize + additional;
				NotifyPropertyChanged(DurationProperty);
			}
		}
		[CreateProperty]
		public ObservableList<AnimationTrackViewModel> Tracks
		{
			get => tracks;
			set
			{
				if (tracks == value)
					return;

				if (tracks != null)
					tracks.ListChanged -= UpdateList;

				tracks = value;
				tracks.ListChanged += UpdateList;
				UpdateList();
			}
		}

		[UxmlAttribute("can-edit"), CreateProperty]
		public bool CanEdit
		{
			get => canEdit ?? false;
			set
			{
				if (canEdit == value)
					return;
				canEdit = value;
			}
		}

		private static VisualTreeAsset templateMain;
		private VisualElement tracksContainer;
		private int duration;
		private ObservableList<AnimationTrackViewModel> tracks = new();
		private bool? canEdit;


		public TrackView()
		{
			if (templateMain == null)
				templateMain = Resources.Load<VisualTreeAsset>("RailsTrackView");
			templateMain.CloneTree(this);
			tracksContainer = this.Q<VisualElement>("tracks-container");
		}

		private void UpdateList()
		{
			
		}
	}
}