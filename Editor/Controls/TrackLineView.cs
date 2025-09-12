using System.Collections.Generic;
using Rails.Editor.ViewModel;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class TrackLineView : VisualElement
	{
		private const string SelectedClass = "tack-key--selected";

		[CreateProperty]
		public ObservableList<AnimationKeyViewModel> Values
		{
			get => values;
			set
			{
				if (values == value)
					return;

				if (values != null)
					values.ListChanged -= UpdateList;

				values = value;
				values.ListChanged += UpdateList;
				UpdateList();
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
					views.ForEach(x => x.RemoveFromClassList(trackClass));
				trackClass = value;
				views.ForEach(x => x.AddToClassList(trackClass));
			}
		}

		[CreateProperty]
		public ObservableList<int> SelectedIndexes { get; set; }

		private ObservableList<AnimationKeyViewModel> values = new();
		private List<TrackKey> views = new();
		private VisualElement keysContainer;
		private VisualElement moveContainer;
		private string trackClass;
		private float framePixelSize = 30;


		public TrackLineView()
		{
			keysContainer = new VisualElement();
			moveContainer = new VisualElement();
			keysContainer.style.position = Position.Absolute;
			keysContainer.style.width = new Length(100, LengthUnit.Percent);
			keysContainer.style.height = new Length(100, LengthUnit.Percent);
			keysContainer.style.flexDirection = FlexDirection.Row;
			keysContainer.style.flexShrink = 0;
			keysContainer.name = "keys-container";
			moveContainer.style.position = Position.Absolute;
			moveContainer.style.width = new Length(100, LengthUnit.Percent);
			moveContainer.style.height = new Length(100, LengthUnit.Percent);
			moveContainer.style.flexDirection = FlexDirection.Row;
			moveContainer.style.flexShrink = 0;
			moveContainer.name = "move-container";
			Add(keysContainer);
			Add(moveContainer);

			AddToClassList("track-line");
			SetBinding(nameof(Values), new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(AnimationTrackViewModel.Keys)),
				bindingMode = BindingMode.ToTarget,
			});
			SetBinding(nameof(TrackClass), new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(AnimationTrackViewModel.TrackClass)),
				bindingMode = BindingMode.ToTarget,
			});
		}

		public void OnFramePixelSizeChanged(float framePixelSize)
		{
			this.framePixelSize = framePixelSize;
			views.ForEach(x => x.OnFramePixelSizeChanged(framePixelSize));
		}

		private void UpdateList()
		{
			if (Values.IsNullOrEmpty())
			{
				keysContainer.Clear();
				views.Clear();
				return;
			}
			while (Values.Count > views.Count)
			{
				var view = CreateElement();
				keysContainer.Add(view);
				views.Add(view);
			}
			while (Values.Count < views.Count)
			{
				var view = views[^1];
				ResetElement(view);
				keysContainer.Remove(view);
				views.Remove(view);
			}
			for (int i = 0; i < views.Count; i++)
			{
				views[i].dataSource = Values[i];
			}
		}

		private TrackKey CreateElement()
		{
			TrackKey key = new();
			key.AddToClassList(trackClass);
			return key;
		}

		private void ResetElement(TrackKey element)
		{

		}
	}

	[UxmlElement]
	public partial class TrackKey : VisualElement
	{
		public static readonly BindingId TimePositionProperty = nameof(TimePosition);

		[UxmlAttribute("timePosition"), CreateProperty]
		public int TimePosition
		{
			get => timePosition;
			private set
			{
				if (timePosition == value)
					return;
				timePosition = value;
				UpdatePosition();
				NotifyPropertyChanged(TimePositionProperty);
			}
		}

		private int timePosition;
		private float framePixelSize = 30;


		public TrackKey()
		{
			AddToClassList("track-key");
			SetBinding(nameof(TimePosition), new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(AnimationKeyViewModel.TimePosition)),
				bindingMode = BindingMode.TwoWay,
			});
			RegisterCallback<GeometryChangedEvent>(OnGeometryChange);
		}

		public void OnFramePixelSizeChanged(float framePixelSize)
		{
			this.framePixelSize = framePixelSize;
			UpdatePosition();
		}

		private void UpdatePosition()
		{
			style.left = TrackLinesView.StartAdditional - layout.width / 2 + TimePosition * framePixelSize;
		}

		private void OnGeometryChange(GeometryChangedEvent evt)
		{
			UpdatePosition();
		}
	}
}