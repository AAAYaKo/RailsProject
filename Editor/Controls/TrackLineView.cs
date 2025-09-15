using Rails.Editor.ViewModel;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class TrackLineView : ListObserverElement<AnimationKeyViewModel, TrackKey>
	{
		private const string SelectedClass = "tack-key--selected";

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

		private VisualElement moveContainer;
		private string trackClass;
		private float framePixelSize = 30;


		public TrackLineView()
		{
			container = new VisualElement();
			moveContainer = new VisualElement();
			container.style.position = Position.Absolute;
			container.style.width = new Length(100, LengthUnit.Percent);
			container.style.height = new Length(100, LengthUnit.Percent);
			container.style.flexDirection = FlexDirection.Row;
			container.style.flexShrink = 0;
			container.name = "keys-container";
			moveContainer.style.position = Position.Absolute;
			moveContainer.style.width = new Length(100, LengthUnit.Percent);
			moveContainer.style.height = new Length(100, LengthUnit.Percent);
			moveContainer.style.flexDirection = FlexDirection.Row;
			moveContainer.style.flexShrink = 0;
			moveContainer.pickingMode = PickingMode.Ignore;
			moveContainer.name = "move-container";
			Add(container);
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

		protected override void UpdateList()
		{
			base.UpdateList();
			views.ForEach(x => x.OnFramePixelSizeChanged(framePixelSize));
		}

		protected override TrackKey CreateElement()
		{
			TrackKey key = new();
			key.AddToClassList(trackClass);
			return key;
		}

		protected override void ResetElement(TrackKey element)
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
			RegisterCallback<GeometryChangedEvent>(x =>
			{
				UpdatePosition();
			});
			RegisterCallback<ClickEvent>(x =>
			{
				Debug.Log("a");
				AddToClassList("tack-key--selected");
			});
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
	}
}