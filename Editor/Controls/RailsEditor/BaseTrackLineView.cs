using System.Collections.Generic;
using System.Linq;
using Rails.Editor.Context;
using Rails.Editor.ViewModel;
using Rails.Runtime;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class BaseTrackLineView<TKeyViewModel, TKey> : ListObserverElement<TKeyViewModel, TrackKeyView>
		where TKeyViewModel : BaseKeyViewModel<TKey>
		where TKey : BaseKey
	{
		public const string SelectedClass = "track-key--selected";
		public static readonly BindingId SelectedIndexesProperty = nameof(SelectedIndexes);
		public static readonly BindingId TrackClassProperty = nameof(TrackClass);
		public static readonly BindingId ChangeSelectionCommandProperty = nameof(ChangeSelectionCommand);
		public static readonly BindingId MoveKeysCommandProperty = nameof(MoveKeysCommand);
		public static readonly BindingId KeyFrameAddAtTimeCommandProperty = nameof(KeyFrameAddAtTimeCommand);

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
		[CreateProperty]
		public ICommand<List<int>> ChangeSelectionCommand { get; set; }
		[CreateProperty]
		public ICommand<Dictionary<int, int>> MoveKeysCommand { get; set; }
		[CreateProperty]
		public ICommand<int> KeyFrameAddAtTimeCommand { get; set; }

		public List<int> SelectedKeysFrames { get; } = new();
		public int FirstSelectedKeyFrame { get; private set; }
		public int LastSelectedKeyFrame { get; private set; }

		private ObservableList<int> selectedIndexes;
		private List<int> selectedViewKeys = new();
		private VisualElement moveContainer;
		private string trackClass;


		public BaseTrackLineView()
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
			SetBinding(ValuesProperty, new ToTargetBinding("Keys"));
			SetBinding(TrackClassProperty, new ToTargetBinding("TrackClass"));
			SetBinding(SelectedIndexesProperty, new ToTargetBinding("SelectedIndexes"));
			SetBinding(ChangeSelectionCommandProperty, new CommandBinding("ChangeSelectionCommand"));
			SetBinding(MoveKeysCommandProperty, new CommandBinding("MoveKeysCommand"));
			SetBinding(KeyFrameAddAtTimeCommandProperty, new CommandBinding("KeyFrameAddAtTimeCommand"));
		}

		public void AddKey(int frame)
		{
			KeyFrameAddAtTimeCommand.Execute(frame);
		}

		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			if (selectedIndexes != null)
				selectedIndexes.ListChanged += OnSelectionChanged;
			EventBus.Subscribe<KeyClickEvent>(OnClickKey);
			EventBus.Subscribe<KeyRightClickEvent>(OnRightClickKey);
			EventBus.Subscribe<KeyMoveEvent>(OnMoveKey);
			EventBus.Subscribe<DeselectAllKeysEvent>(OnDeselectAll);
			EventBus.Subscribe<SelectionBoxBeginEvent>(OnSelectionBoxBegin);
			EventBus.Subscribe<SelectionBoxChangeEvent>(OnSelectionBoxChange);
			EventBus.Subscribe<SelectionBoxCompleteEvent>(OnSelectionBoxComplete);
			EventBus.Subscribe<KeyDragCompleteEvent>(OnKeyDragComplete);
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			if (selectedIndexes != null)
				selectedIndexes.ListChanged -= OnSelectionChanged;
			EventBus.Unsubscribe<KeyClickEvent>(OnClickKey);
			EventBus.Unsubscribe<KeyRightClickEvent>(OnRightClickKey);
			EventBus.Unsubscribe<KeyMoveEvent>(OnMoveKey);
			EventBus.Unsubscribe<DeselectAllKeysEvent>(OnDeselectAll);
			EventBus.Unsubscribe<SelectionBoxBeginEvent>(OnSelectionBoxBegin);
			EventBus.Unsubscribe<SelectionBoxChangeEvent>(OnSelectionBoxChange);
			EventBus.Unsubscribe<SelectionBoxCompleteEvent>(OnSelectionBoxComplete);
			EventBus.Unsubscribe<KeyDragCompleteEvent>(OnKeyDragComplete);
		}

		private void UpdateSelectedKeyFrames()
		{
			SelectedKeysFrames.Clear();
			FirstSelectedKeyFrame = int.MaxValue;
			LastSelectedKeyFrame = int.MinValue;
			if (SelectedIndexes.IsNullOrEmpty())
				return;
			foreach (int key in SelectedIndexes)
			{
				if (key < 0 || key >= views.Count)
					continue;
				int frame = views[key].TimePosition;
				FirstSelectedKeyFrame = math.min(frame, FirstSelectedKeyFrame);
				LastSelectedKeyFrame = math.max(frame, LastSelectedKeyFrame);
				SelectedKeysFrames.Add(frame);
			}
		}

		private void SelectKey(int keyIndex)
		{
			if (selectedViewKeys.Contains(keyIndex))
				return;
			if (keyIndex < 0 || keyIndex >= views.Count)
				return;
			selectedViewKeys.Add(keyIndex);
			var key = views[keyIndex];
			SelectVisually(key, keyIndex);
		}

		private void DeselectKey(int keyIndex)
		{
			if (!selectedViewKeys.Contains(keyIndex))
				return;
			selectedViewKeys.Remove(keyIndex);
			var key = views[keyIndex];
			DeselectVisually(key, keyIndex);
		}

		private void DeselectAllKeys(TrackKeyView keyIgnore = null)
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
				DeselectVisually(key, x);
			});
			selectedViewKeys.Clear();
			if (wasSelected)
			{
				int keyIgnoreIndex = views.IndexOf(keyIgnore);
				if (keyIgnoreIndex >= 0)
					selectedViewKeys.Add(keyIgnoreIndex);
			}
		}

		protected virtual void SelectVisually(TrackKeyView key, int keyIndex)
		{
			container.Remove(key);
			moveContainer.Add(key);
			key.AddToClassList(SelectedClass);
		}

		protected virtual void DeselectVisually(TrackKeyView key, int keyIndex)
		{
			moveContainer.Remove(key);
			container.Add(key);
			key.RemoveFromClassList(SelectedClass);
		}

		private void OnSelectionChanged()
		{
			if (SelectedIndexes.IsNullOrEmpty())
			{
				DeselectAllKeys();
				UpdateSelectedKeyFrames();
				return;
			}
			int[] toRemove = selectedViewKeys.Except(SelectedIndexes).ToArray();
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

		protected override void UpdateList()
		{
			DeselectAllKeys();
			base.UpdateList();
			OnSelectionChanged();
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
				views[key].TimePosition = Values[key].TimePosition.Frames + evt.DeltaFrames;
		}

		private void OnClickKey(KeyClickEvent evt)
		{
			TrackKeyView key = evt.Key;
			int index = views.IndexOf(key);

			if (index < 0)
				return;

			if (!evt.ActionKey && !selectedViewKeys.Contains(index))
				EventBus.Publish(new DeselectAllKeysEvent(key));
			
			if (!selectedViewKeys.Contains(index))
			{
				SelectKey(index);
			}
			else if (evt.ActionKey && selectedViewKeys.Contains(index))
			{
				DeselectKey(index);
			}

			ChangeSelectionCommand.Execute(selectedViewKeys);
		}

		private void OnRightClickKey(KeyRightClickEvent evt)
		{
			TrackKeyView key = evt.Key;
			int index = views.IndexOf(key);

			if (index < 0)
				return;

			if (!selectedViewKeys.Contains(index))
				EventBus.Publish(new DeselectAllKeysEvent(key));

			if (!selectedViewKeys.Contains(index))
				SelectKey(index);

			ChangeSelectionCommand.Execute(selectedViewKeys);
		}

		private void OnDeselectAll(DeselectAllKeysEvent evt)
		{
			DeselectAllKeys(evt.Key);
			ChangeSelectionCommand.Execute(selectedViewKeys);
		}

		private void OnSelectionBoxBegin(SelectionBoxBeginEvent evt)
		{
			if (evt.ActionKey)
				return;
			DeselectAllKeys();
			ChangeSelectionCommand.Execute(selectedViewKeys);
		}

		private void OnSelectionBoxChange(SelectionBoxChangeEvent evt)
		{
			Rect selectionRect = parent.WorldToLocal(evt.SelectionWorldRect);

			if (!layout.Overlaps(selectionRect))
				return;

			selectionRect = this.WorldToLocal(evt.SelectionWorldRect);
			for (int i = 0; i < views.Count; i++)
			{
				var view = views[i];
				if (view.layout.Overlaps(selectionRect))
					SelectKey(i);
				else if (!SelectedIndexes.Contains(i))
					DeselectKey(i);
			}
		}

		private void OnSelectionBoxComplete(SelectionBoxCompleteEvent evt)
		{
			Rect selectionRect = parent.WorldToLocal(evt.SelectionWorldRect);

			if (!layout.Overlaps(selectionRect))
				return;

			ChangeSelectionCommand.Execute(selectedViewKeys);
		}

		private void OnKeyDragComplete(KeyDragCompleteEvent evt)
		{
			if (SelectedIndexes.IsNullOrEmpty())
				return;
			UpdateSelectedKeyFrames();
			var keysMoveMap = SelectedIndexes
				.Zip(SelectedKeysFrames, (x, y) => new { x, y })
				.ToDictionary(x => x.x, x => x.y);

			MoveKeysCommand.Execute(keysMoveMap);
		}
	}
}