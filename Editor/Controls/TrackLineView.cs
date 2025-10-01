using System;
using System.Collections.Generic;
using System.Linq;
using Rails.Editor.ViewModel;
using Unity.Mathematics;
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
					RemoveFromClassList(trackClass);
				trackClass = value;
				AddToClassList(trackClass);
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

				if (selectedIndexes != null)
					selectedIndexes.ListChanged -= OnSelectionChanged;

				selectedIndexes = value;
				selectedIndexes.ListChanged += OnSelectionChanged;
				OnSelectionChanged();
			}
		}
		public List<int> SelectedKeysFrames { get; } = new();
		public int FirstSelectedKeyFrame { get; private set; }
		public int LastSelectedKeyFrame { get; private set; }

		private ObservableList<int> selectedIndexes;
		private List<int> selectedViewKeys = new();
		private List<TrackTweenLineView> tweenLines = new();
		private Dictionary<int, TrackTweenLineView> keyToTweenLines = new();
		private VisualElement moveContainer;
		private string trackClass;


		public TrackLineView()
		{
			container = new VisualElement();
			moveContainer = new VisualElement();
			container.style.position = Position.Absolute;
			container.style.width = new Length(100, LengthUnit.Percent);
			container.style.height = new Length(100, LengthUnit.Percent);
			container.style.flexDirection = FlexDirection.Row;
			container.style.flexShrink = 0;
			container.pickingMode = PickingMode.Ignore;
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

			pickingMode = PickingMode.Ignore;
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
			SetBinding(nameof(SelectedIndexes), new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(AnimationTrackViewModel.SelectedIndexes)),
				bindingMode = BindingMode.ToTarget,
			});
		}

		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			EventBus.Subscribe<KeyClickEvent>(OnClickKey);
			EventBus.Subscribe<KeyMoveEvent>(OnMoveKey);
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			EventBus.Unsubscribe<KeyClickEvent>(OnClickKey);
			EventBus.Unsubscribe<KeyMoveEvent>(OnMoveKey);
		}


		public void OnSelectionBoxBegin(MouseDownEvent evt)
		{
			if (!evt.actionKey)
			{
				DeselectAllKeys();
				PopulateSelectedIndexes();
			}
		}

		public void OnSelectionBoxChanged(Rect selectionWorldRect, MouseMoveEvent evt)
		{
			Rect selectionRect = this.WorldToLocal(selectionWorldRect);
			for (int i = 0; i < views.Count; i++)
			{
				var view = views[i];
				if (view.layout.Overlaps(selectionRect))
					SelectKey(i);
				else if (!SelectedIndexes.Contains(i))
					DeselectKey(i);
			}
		}

		public void OnSelectionBoxComplete()
		{
			PopulateSelectedIndexes();
		}

		public void SelectKey(int keyIndex)
		{
			if (selectedViewKeys.Contains(keyIndex))
				return;
			if (keyIndex < 0 || keyIndex >= views.Count)
				return;
			selectedViewKeys.Add(keyIndex);
			var key = views[keyIndex];
			container.Remove(key);
			moveContainer.Add(key);
			key.AddToClassList(SelectedClass);
			if (keyToTweenLines.ContainsKey(keyIndex))
				keyToTweenLines[keyIndex].AddToClassList(SelectedClass);
		}

		public void DeselectKey(int keyIndex)
		{
			if (!selectedViewKeys.Contains(keyIndex))
				return;
			selectedViewKeys.Remove(keyIndex);
			var key = views[keyIndex];
			moveContainer.Remove(key);
			container.Add(key);
			key.RemoveFromClassList(SelectedClass);
			if (keyToTweenLines.ContainsKey(keyIndex))
				keyToTweenLines[keyIndex].RemoveFromClassList(SelectedClass);
		}

		public void DeselectAllKeys(TrackKeyView keyIgnore = null)
		{
			bool wasSelected = false;
			selectedViewKeys.ForEach(x =>
			{
				var key = views[x];
				if (keyIgnore == key)
				{
					wasSelected = true;
					return;
				}
				moveContainer.Remove(key);
				container.Add(key);
				key.RemoveFromClassList(SelectedClass);
				if (keyToTweenLines.ContainsKey(x))
					keyToTweenLines[x].RemoveFromClassList(SelectedClass);
			});
			selectedViewKeys.Clear();
			if (wasSelected)
			{
				int keyIgnoreIndex = views.IndexOf(keyIgnore);
				if (keyIgnoreIndex >= 0)
					selectedViewKeys.Add(keyIgnoreIndex);
			}
		}

		public void UpdateSelectedKeyFrames()
		{
			SelectedKeysFrames.Clear();
			FirstSelectedKeyFrame = int.MaxValue;
			LastSelectedKeyFrame = int.MinValue;
			foreach (int key in SelectedIndexes)
			{
				int frame = views[key].TimePosition;
				FirstSelectedKeyFrame = math.min(frame, FirstSelectedKeyFrame);
				LastSelectedKeyFrame = math.max(frame, LastSelectedKeyFrame);
				SelectedKeysFrames.Add(frame);
			}
		}

		private void OnSelectionChanged()
		{
			List<int> toRemove = selectedViewKeys.FindAll(x => !SelectedIndexes.Contains(x));
			foreach (int key in toRemove)
			{
				DeselectKey(key);
			}

			foreach (int key in SelectedIndexes)
			{
				if (!selectedViewKeys.Contains(key))
					SelectKey(key);
			}

			UpdateSelectedKeyFrames();
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
				if (previous.Ease.EaseType is not Runtime.RailsEase.EaseType.NoAnimation)
				{
					TrackTweenLineView line = tweenLines[lineI];
					keyToTweenLines.Add(i, line);
					lineI++;
					line.StartFrame = views[i].TimePosition;
					line.EndFrame = views[i + 1].TimePosition;
				}
			}
		}

		private TrackTweenLineView CreateTweenLine()
		{
			TrackTweenLineView line = new();
			return line;
		}

		protected override void UpdateList()
		{
			DeselectAllKeys();
			base.UpdateList();
			for (int i = 0; i < views.Count; i++)
			{
				var keyView = views[i];
				var keyViewModel = Values[i];
				keyView.SetTimePositionWithoutUpdate(keyViewModel.TimePosition);
			}
			if (!SelectedIndexes.IsNullOrEmpty())
			{
				foreach (int key in SelectedIndexes)
					SelectKey(key);
			}
			UpdateTweenLines();
		}

		protected override TrackKeyView CreateElement()
		{
			TrackKeyView key = new();
			return key;
		}

		private void OnMoveKey(KeyMoveEvent evt)
		{
			if (SelectedIndexes.IsNullOrEmpty())
				return;
			foreach (var key in SelectedIndexes)
				views[key].TimePosition = Values[key].TimePosition + evt.DeltaFrames;

			UpdateTweenLines();
		}

		private void OnClickKey(KeyClickEvent evt)
		{
			TrackKeyView key = evt.Key;
			bool actionKey = evt.ActionKey;
			int index = views.IndexOf(key);

			if (index < 0)
				return;

			if (!actionKey && !selectedViewKeys.Contains(index))
				DeselectAllKeys(key);

			if (!selectedViewKeys.Contains(index))
			{
				SelectKey(index);
			}

			PopulateSelectedIndexes();
		}

		private void PopulateSelectedIndexes()
		{
			List<int> toRemove = SelectedIndexes.FindAll(x => !selectedViewKeys.Contains(x));
			List<int> toAdd = selectedViewKeys.FindAll(x => !SelectedIndexes.Contains(x));
			foreach (int keyIndex in toRemove)
				SelectedIndexes.RemoveWithoutNotify(keyIndex);

			foreach (int keyIndex in toAdd)
				SelectedIndexes.AddWithoutNotify(keyIndex);

			SelectedIndexes.NotifyListChanged();
		}
	}
}