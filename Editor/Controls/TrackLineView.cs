using System;
using System.Collections.Generic;
using System.Linq;
using Rails.Editor.ViewModel;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class TrackLineView : ListObserverElement<AnimationKeyViewModel, TrackKeyView>
	{
		private const string SelectedClass = "track-key--selected";
		public static readonly BindingId SelectedIndexesProperty = nameof(SelectedIndexes);

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
				tweenLines.ForEach(x => x.TrackClass = trackClass);
			}
		}

		[CreateProperty]
		public ObservableList<int> SelectedIndexes 
		{
			get => selectedIndexes;
			set
			{
				if (selectedIndexes == value)
					return;

				if (values != null)
					selectedIndexes.ListChanged -= OnSelectionChanged;

				selectedIndexes = value;
				selectedIndexes.ListChanged += OnSelectionChanged;
				OnSelectionChanged();
			}
		}

		private ObservableList<int> selectedIndexes = new();
		private List<TrackTweenLineView> tweenLines = new();
		private Dictionary<int, TrackTweenLineView> keyToTweenLines = new();
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
			tweenLines.ForEach(x => x.OnFramePixelSizeChanged(framePixelSize));
			UpdateTweenLines();
		}

		public void SelectKey(int keyIndex)
		{
			SelectedIndexes.Add(keyIndex);
			views[keyIndex].AddToClassList(SelectedClass);
			if (keyToTweenLines.ContainsKey(keyIndex))
				keyToTweenLines[keyIndex].AddToClassList(SelectedClass);
		}

		public void DeselectKey(int keyIndex)
		{
			SelectedIndexes.Remove(keyIndex);
			views[keyIndex].RemoveFromClassList(SelectedClass);
			if (keyToTweenLines.ContainsKey(keyIndex))
				keyToTweenLines[keyIndex].RemoveFromClassList(SelectedClass);
		}

		public void DeselectKeysAll()
		{
			SelectedIndexes.ForEach(x =>
			{
				views[x].RemoveFromClassList(SelectedClass);
				if (keyToTweenLines.ContainsKey(x))
					keyToTweenLines[x].RemoveFromClassList(SelectedClass);
			});
			SelectedIndexes.Clear();
		}

		private void OnSelectionChanged()
		{
			
		}

		private void UpdateTweenLines()
		{
			if (Values.IsNullOrEmpty())
			{
				tweenLines.ForEach(x => container.Remove(x));
				tweenLines.Clear();
				return;
			}
			int count = Values.Take(Values.Count - 1).Count(x => x.Ease.EaseType is not Runtime.RailsEase.EaseType.NoAnimation);

			while (count > tweenLines.Count)
			{
				var line = CreateTweenLine();
				container.Add(line);
				tweenLines.Add(line);
			}
			while (count < tweenLines.Count)
			{
				var line = tweenLines[^1];
				container.Remove(line);
				tweenLines.Remove(line);
			}

			int lineI = 0;
			keyToTweenLines.Clear();
			for (int i = 0; i < Values.Count - 1; i++)
			{
				var previous = Values[i];
				var next = Values[i + 1];

				if (previous.Ease.EaseType is not Runtime.RailsEase.EaseType.NoAnimation)
				{
					TrackTweenLineView line = tweenLines[lineI];
					keyToTweenLines.Add(Values.IndexOf(previous), line);
					lineI++;
					line.StartFrame = previous.TimePosition;
					line.EndFrame = next.TimePosition;
				}
			}
		}

		private TrackTweenLineView CreateTweenLine()
		{
			TrackTweenLineView line = new();
			if (!TrackClass.IsNullOrEmpty())
				line.TrackClass = trackClass;
			line.OnFramePixelSizeChanged(framePixelSize);
			return line;
		}

		protected override void UpdateList()
		{
			base.UpdateList();
			views.ForEach(x => x.OnFramePixelSizeChanged(framePixelSize));
			UpdateTweenLines();
		}

		protected override TrackKeyView CreateElement()
		{
			TrackKeyView key = new();
			if (!TrackClass.IsNullOrEmpty())
				key.AddToClassList(TrackClass);
			key.OnClick += OnClickKey;
			return key;
		}

		protected override void ResetElement(TrackKeyView element)
		{
			element.OnClick -= OnClickKey;
		}

		private void OnClickKey(TrackKeyView key, ClickEvent clickEvent)
		{
			int index = views.IndexOf(key);
			if (index < 0)
				return;

			if (!SelectedIndexes.Contains(index) && !clickEvent.actionKey)
				DeselectKeysAll();

			if (SelectedIndexes.Contains(index))
				DeselectKey(index);
			else
				SelectKey(index);
		}
	}
}