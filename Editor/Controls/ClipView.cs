using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class ClipView : BaseView
	{
		private const string FixedDimensionKey = "clipFixedDimension";
		private TracksListView tracksListView;
		private ClipControl clipControl;
		private TrackLinesView trackView;
		private RailsRuler ruler;
		private RailsPlayHead playHead;
		private TwoPanelsView split;

		private static VisualTreeAsset templateLeft;
		private static VisualTreeAsset templateRight;


		static ClipView()
		{
			templateLeft = Resources.Load<VisualTreeAsset>("RailsSecondPage");
			templateRight = Resources.Load<VisualTreeAsset>("RailsThirdPage");
		}

		public ClipView()
		{
			split = new TwoPanelsView();
			split.style.width = new Length(100, LengthUnit.Percent);
			split.style.height = new Length(100, LengthUnit.Percent);
			split.style.flexGrow = 1;
			templateLeft.CloneTree(split.FirstPanel);
			templateRight.CloneTree(split.SecondPanel);

			split.FixedPaneInitialDimension = Storage.RecordsFloat.Get(FixedDimensionKey, 300);
			hierarchy.Add(split);

			clipControl = split.FirstPanel.Q<ClipControl>();
			tracksListView = split.FirstPanel.Q<TracksListView>();
			trackView = split.SecondPanel.Q<TrackLinesView>();
			ruler = split.SecondPanel.Q<RailsRuler>();
			playHead = split.SecondPanel.Q<RailsPlayHead>();
		}

		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			RegisterCallback<WheelEvent>(ScrollHandler, TrickleDown.TrickleDown);
			trackView.VerticalScroller.valueChanged += OnVerticalScroller;
			split.FixedPanelDimensionChanged += OnDimensionChanged;
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			UnregisterCallback<WheelEvent>(ScrollHandler, TrickleDown.TrickleDown);
			trackView.VerticalScroller.valueChanged -= OnVerticalScroller;
			split.FixedPanelDimensionChanged -= OnDimensionChanged;
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

		private void OnDimensionChanged(float dimension)
		{
			Storage.RecordsFloat.Set(FixedDimensionKey, dimension);
		}
	}
}