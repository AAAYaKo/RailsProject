using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class ClipView : VisualElement
	{
		private TracksListView tracksListView;
		private ClipControl clipControl;
		private TrackLinesView trackView;
		private RailsRuler ruler;
		private RailsPlayHead playHead;

		private static VisualTreeAsset templateLeft;
		private static VisualTreeAsset templateRight;


		public ClipView()
		{
			if (templateLeft == null)
				templateLeft = Resources.Load<VisualTreeAsset>("RailsSecondPage");
			if (templateRight == null)
				templateRight = Resources.Load<VisualTreeAsset>("RailsThirdPage");

			TwoPanelsView split = new();
			split.style.width = new Length(100, LengthUnit.Percent);
			split.style.height = new Length(100, LengthUnit.Percent);
			split.style.flexGrow = 1;
			templateLeft.CloneTree(split.FirstPanel);
			templateRight.CloneTree(split.SecondPanel);

			split.FixedPaneInitialDimension = 300;
			hierarchy.Add(split);

			clipControl = split.FirstPanel.Q<ClipControl>();
			tracksListView = split.FirstPanel.Q<TracksListView>();
			trackView = split.SecondPanel.Q<TrackLinesView>();
			ruler = split.SecondPanel.Q<RailsRuler>();
			playHead = split.SecondPanel.Q<RailsPlayHead>();
			RegisterCallback<WheelEvent>(ScrollHandler, TrickleDown.TrickleDown);
			trackView.VerticalScroller.valueChanged += OnVerticalScroller;
		}

		private void OnVerticalScroller(float value)
		{
			tracksListView.Scroll.verticalScroller.value = value;
		}

		private void ScrollHandler(WheelEvent evt)
		{
			float size = trackView.ScrollView.mouseWheelScrollSize;
			float y = evt.delta.y * ((trackView.VerticalScroller.lowValue < trackView.VerticalScroller.highValue) ? 1f : (-1f)) * size;
			float x = evt.delta.x * size;

			trackView.Scroll(new Vector2(x, y));
			tracksListView.Scroll.scrollOffset += new Vector2(0, y);

			evt.StopPropagation();
		}
	}
}