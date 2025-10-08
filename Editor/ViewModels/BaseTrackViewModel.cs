using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Rails.Runtime;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Rails.Editor.ViewModel
{
	public abstract class BaseTrackViewModel<TTrack, TKey, TKeyViewModel> : BaseNotifyPropertyViewModel<TTrack>
		where TKey : BaseKey
		where TTrack : BaseTrack<TKey>
		where TKeyViewModel : BaseKeyViewModel<TKey>, new()
	{
		public const string StoreKey = "selectedKeys_";

		[CreateProperty]
		public abstract string TrackClass { get; }
		[CreateProperty]
		public ObservableList<TKeyViewModel> Keys => keys;
		[CreateProperty]
		public bool IsKeyFrame
		{
			get => isKeyFrame;
			set => SetProperty(ref isKeyFrame, value);
		}
		[CreateProperty]
		public ObservableList<int> SelectedIndexes => selectedIndexes;
		[CreateProperty]
		public ICommand<List<int>> ChangeSelectionCommand
		{
			get => changeSelection;
			set => SetProperty(ref changeSelection, value);
		}
		[CreateProperty]
		public ICommand<Dictionary<int, int>> MoveKeysCommand
		{
			get => moveKeys;
			set => SetProperty(ref moveKeys, value);
		}
		[CreateProperty]
		public ICommand<int> KeyFrameAddAtTimeCommand
		{
			get => keyFrameAddAtTimeCommand;
			set => SetProperty(ref keyFrameAddAtTimeCommand, value);
		}

		protected ObservableList<TKeyViewModel> keys = new();
		protected ObservableList<int> selectedIndexes = new();
		private ICommand<int> keyFrameAddAtTimeCommand;
		protected ICommand<List<int>> changeSelection;
		protected ICommand<Dictionary<int, int>> moveKeys;
		protected StoredIntList storedSelectedIndexes;
		protected bool isKeyFrame;
		protected int currentFrame;


		protected BaseTrackViewModel()
		{
			ChangeSelectionCommand = new RelayCommand<List<int>>(ChangeSelection);
			MoveKeysCommand = new RelayCommand<Dictionary<int, int>>(MoveKeys);
			KeyFrameAddAtTimeCommand = new RelayCommand<int>(AddKey);
		}

		public void OnClipSelect()
		{
			storedSelectedIndexes.Bind(EditorContext.Instance.DataStorage.RecordsSelectedClips);
			storedSelectedIndexes.ValueChanged += OnStoredSelectedChanged;
			OnStoredSelectedChanged(storedSelectedIndexes.Value);
		}

		public void OnClipDeselect()
		{
			storedSelectedIndexes.Unbind();
			storedSelectedIndexes.ValueChanged -= OnStoredSelectedChanged;
		}

		public virtual void OnTimeHeadPositionChanged(int frame)
		{
			currentFrame = frame;
			int previousIndex = keys.FindLastIndex(x =>
			{
				return x.TimePosition <= frame;
			});
			IsKeyFrame = previousIndex >= 0 && previousIndex < keys.Count && keys[previousIndex].TimePosition == frame;
		}

		public void RemoveSelectedKeys()
		{
			if (SelectedIndexes.IsNullOrEmpty())
				return;
			var toRemove = SelectedIndexes
				.Select(x => model.AnimationKeys[x])
				.ToArray();
			storedSelectedIndexes.Value = new();
			model.RemoveKeys(toRemove);
		}

		protected override void OnBind()
		{
			base.OnBind();
		}

		protected override void OnUnbind()
		{
			base.OnUnbind();
			ClearViewModels<TKeyViewModel, TKey>(Keys,
				resetViewModel: vm =>
				{
					vm.propertyChanged -= OnKeyPropertyChanged;
				});
		}

		protected override void OnModelChanged()
		{
			selectedIndexes.Clear();
			if (model == null)
			{
				if (keys.Count > 0)
					ClearViewModels<TKeyViewModel, TKey>(Keys, vm =>
					{
						vm.propertyChanged -= OnKeyPropertyChanged;
					});
				return;
			}

			UpdateKeys();

			if (!storedSelectedIndexes.Value.IsNullOrEmpty())
			{
				if (storedSelectedIndexes.Value.Any(x => x >= keys.Count || x < 0))
					storedSelectedIndexes.Value = new List<int>();
				SelectedIndexes.AddRangeWithoutNotify(storedSelectedIndexes.Value);
			}

			NotifyPropertyChanged(nameof(Keys));
			NotifyPropertyChanged(nameof(SelectedIndexes));
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(BaseTrack<TKey>.AnimationKeys))
			{
				UpdateKeys();
				OnTimeHeadPositionChanged(currentFrame);
			}
		}

		protected void UpdateKeys()
		{
			UpdateVieModels(Keys, model.AnimationKeys,
				createViewModel: i => new TKeyViewModel(),
				resetViewModel: vm =>
				{
					vm.propertyChanged -= OnKeyPropertyChanged;
				},
				viewModelBindCallback: (vm, m) =>
				{
					vm.propertyChanged += OnKeyPropertyChanged;
				}
			);
		}

		protected void AddKey(int frame)
		{
			int keyIndex = keys.FindIndex(x => x.TimePosition == frame);
			if (keyIndex >= 0)
				return;
			EditorContext.Instance.Record("Key Frame Added");

			keyIndex = keys.FindIndex(x => x.TimePosition > frame);
			var copy = SelectedIndexes.ToList();
			if (keyIndex >= 0)
			{
				for (int i = 0; i < copy.Count; i++)
				{
					if (copy[i] >= keyIndex)
						copy[i] += 1;
				}
			}

			model.InsertNewKeyAt(frame);
			storedSelectedIndexes.Value = copy;
		}

		protected void RemoveKey(int frame)
		{
			int keyIndex = keys.FindIndex(x => x.TimePosition == frame);
			if (keyIndex < 0)
				return;
			EditorContext.Instance.Record("Key Frame Removed");
			if (SelectedIndexes.Contains(keyIndex))
			{
				SelectedIndexes.Remove(keyIndex);
				storedSelectedIndexes.Value = SelectedIndexes.ToList();
			}
			model.RemoveKey(model.AnimationKeys[keyIndex]);
		}

		protected void ChangeSelection(List<int> selection)
		{
			EditorContext.Instance.Record(EditorContext.Instance.EditorWindow, "Keys Selection Changed");
			storedSelectedIndexes.Value = new(selection);
		}

		protected void MoveKeys(Dictionary<int, int> keysFramesPositions)
		{
			EditorContext.Instance.Record(EditorContext.Instance.EditorWindow, "Animation Keys Moved");
			UpdateSelectionAfterMove(keysFramesPositions);
			model.MoveMultipleKeys(keysFramesPositions);
		}

		private void UpdateSelectionAfterMove(Dictionary<int, int> keysFramesPositions)
		{
			HashSet<int> removedKeys = new();
			List<(int Index, int Position)> movedKeysMap = new(keys.Count);

			for (int i = 0; i < keys.Count; i++)
			{
				if (keysFramesPositions.ContainsValue(keys[i].TimePosition)) //these keys will be removed
				{
					removedKeys.Add(i);
					continue;
				}

				movedKeysMap.Add((i, keysFramesPositions.TryGetValue(i, out int newPosition) ?
					newPosition :
					keys[i].TimePosition)); //add keys with their positions after moving
			}

			movedKeysMap.Sort((a, b) => a.Position.CompareTo(b.Position)); //keys must be ordered by time position

			Dictionary<int, int> newIndexMap = new(movedKeysMap.Count);
			for (int i = 0; i < movedKeysMap.Count; i++)
				newIndexMap[movedKeysMap[i].Index] = i; //fill the map: from current key index in list to new index after moving
			List<int> newSelection = new(storedSelectedIndexes.Value.Count);
			foreach (int keyIndex in storedSelectedIndexes.Value) //form new selection
			{
				if (removedKeys.Contains(keyIndex)) //skip removed keys
					continue;

				if (newIndexMap.TryGetValue(keyIndex, out int newKeyIndex))
					newSelection.Add(newKeyIndex); //convert old index to new index
			}
			storedSelectedIndexes.Value = newSelection;
		}

		private void OnStoredSelectedChanged(List<int> newValue)
		{
			int[] toRemove = newValue == null ? SelectedIndexes.ToArray() : SelectedIndexes.Except(newValue).ToArray();
			int[] toAdd = newValue?.Except(SelectedIndexes).ToArray() ?? new int[0];
			toRemove.ForEach(x => SelectedIndexes.RemoveWithoutNotify(x));
			toAdd.ForEach(x => SelectedIndexes.AddWithoutNotify(x));

			SelectedIndexes.NotifyListChanged();
		}

		private void OnKeyPropertyChanged(object sender, BindablePropertyChangedEventArgs e)
		{
			OnTimeHeadPositionChanged(currentFrame);
		}
	}
}