using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Rails.Runtime.Tracks;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.ViewModel
{
	public class AnimationTrackViewModel : BaseNotifyPropertyViewModel<AnimationTrack>
	{
		public const string StoreKey = "selectedKeys_";

		[CreateProperty]
		public UnityEngine.Object Reference
		{
			get => reference;
			set
			{
				if (SetProperty(ref reference, value))
					model.SceneReference = reference;
			}
		}
		[CreateProperty]
		public Type Type => trackData?.AnimationComponentType;
		[CreateProperty]
		public AnimationTrack.ValueType ValueType => trackData?.ValueType ?? AnimationTrack.ValueType.Single;
		[CreateProperty]
		public string TrackClass => trackData?.TrackClass;
		[CreateProperty]
		public ObservableList<AnimationKeyViewModel> Keys => keys;
		[CreateProperty]
		public float CurrentSingleValue
		{
			get => currentSingleValue ?? 0;
			set => SetProperty(ref currentSingleValue, value);
		}
		[CreateProperty]
		public Vector2 CurrentVector2Value
		{
			get => currentVector2Value ?? Vector2.zero;
			set => SetProperty(ref currentVector2Value, value);
		}
		[CreateProperty]
		public Vector3 CurrentVector3Value
		{
			get => currentVector3Value ?? Vector3.zero;
			set => SetProperty(ref currentVector3Value, value);
		}
		[CreateProperty]
		public bool IsKeyFrame
		{
			get => isKeyFrame;
			set => SetProperty(ref isKeyFrame, value);
		}
		[CreateProperty]
		public ObservableList<int> SelectedIndexes => selectedIndexes;
		[CreateProperty]
		public ICommand RemoveCommand
		{
			get => removeCommand;
			set => SetProperty(ref removeCommand, value);
		}
		[CreateProperty]
		public ICommand KeyFrameAddCommand
		{
			get => keyFrameAddCommand;
			set => SetProperty(ref keyFrameAddCommand, value);
		}
		[CreateProperty]
		public ICommand KeyFrameRemoveCommand
		{
			get => keyFrameRemoveCommand;
			set => SetProperty(ref keyFrameRemoveCommand, value);
		}
		[CreateProperty]
		public ICommand<ValueEditArgs> ValueEditCommand
		{
			get => valueEditCommand;
			set => SetProperty(ref valueEditCommand, value);
		}
		[CreateProperty]
		public ICommand<List<int>> ChangeSelection
		{
			get => changeSelection;
			set => SetProperty(ref changeSelection, value);
		}
		[CreateProperty]
		public ICommand<Dictionary<int, int>> MoveKeys
		{
			get => moveKeys;
			set => SetProperty(ref moveKeys, value);
		}

		private UnityEngine.Object reference;
		private TrackData trackData;
		private ObservableList<AnimationKeyViewModel> keys = new();
		private float? currentSingleValue;
		private Vector2? currentVector2Value;
		private Vector3? currentVector3Value;
		private StoredIntList storedSelectedIndexes;
		private bool isKeyFrame;
		private int currentFrame;
		private ObservableList<int> selectedIndexes = new();
		private ICommand removeCommand;
		private ICommand keyFrameAddCommand;
		private ICommand keyFrameRemoveCommand;
		private ICommand<ValueEditArgs> valueEditCommand;
		private ICommand<List<int>> changeSelection;
		private ICommand<Dictionary<int, int>> moveKeys;

		public AnimationTrackViewModel(int trackIndex)
		{
			storedSelectedIndexes = new StoredIntList(StoreKey + trackIndex);

			KeyFrameRemoveCommand = new RelayCommand(() =>
			{
				int keyIndex = keys.FindIndex(x => x.TimePosition == currentFrame);
				if (keyIndex < 0)
					return;
				EditorContext.Instance.Record("Key Frame Removed");
				if (SelectedIndexes.Contains(keyIndex))
				{
					SelectedIndexes.Remove(keyIndex);
					storedSelectedIndexes.Value = SelectedIndexes.ToList();
				}
				model.RemoveKey(model.AnimationKeys[keyIndex]);
			});

			KeyFrameAddCommand = new RelayCommand(() =>
			{
				int keyIndex = keys.FindIndex(x => x.TimePosition == currentFrame);
				if (keyIndex >= 0)
					return;
				EditorContext.Instance.Record("Key Frame Added");
				model.InsertNewKeyAt(currentFrame);
			});
			ValueEditCommand = new RelayCommand<ValueEditArgs>(args =>
			{
				int keyIndex = keys.FindIndex(x => x.TimePosition == currentFrame);
				if (keyIndex >= 0)
				{
					EditorContext.Instance.Record("Key Value Changed");
					model.AnimationKeys[keyIndex].SingleValue = args.SingleValue;
					model.AnimationKeys[keyIndex].Vector2Value = args.Vector2Value;
					model.AnimationKeys[keyIndex].Vector3Value = args.Vector3Value;
				}
				else
				{
					EditorContext.Instance.Record("Key Frame Added");
					model.InsertNewKeyAt(currentFrame, args.SingleValue, args.Vector2Value, args.Vector3Value);
				}
			});

			ChangeSelection = new RelayCommand<List<int>>(x =>
			{
				EditorContext.Instance.Record(EditorContext.Instance.EditorWindow, "Keys Selection Changed");
				storedSelectedIndexes.Value = new(x);
			});

			MoveKeys = new RelayCommand<Dictionary<int, int>>(MoveAnimationKeys);
		}

		protected override void OnBind()
		{
			base.OnBind();
		}

		protected override void OnUnbind()
		{
			base.OnUnbind();
			ClearViewModels<AnimationKeyViewModel, AnimationKey>(Keys,
				resetViewModel: vm =>
				{
					vm.propertyChanged -= OnKeyPropertyChanged;
				});
		}

		protected override void OnModelChanged()
		{
			if (model == null)
				return;

			Reference = model.SceneReference;
			trackData = TrackTypes[model.GetType()];

			selectedIndexes.Clear();
			if (model == null)
			{
				if (keys.Count > 0)
					ClearViewModels<AnimationKeyViewModel, AnimationKey>(Keys, vm =>
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

			NotifyPropertyChanged(nameof(Type));
			NotifyPropertyChanged(nameof(ValueType));
			NotifyPropertyChanged(nameof(Keys));
			NotifyPropertyChanged(nameof(SelectedIndexes));
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(AnimationTrack.SceneReference))
				Reference = model.SceneReference;
			else if (e.PropertyName == nameof(AnimationTrack.AnimationKeys))
			{
				UpdateKeys();
				OnTimeHeadPositionChanged(currentFrame);
			}
		}

		public void OnTimeHeadPositionChanged(int frame)
		{
			currentFrame = frame;
			int previousIndex = keys.FindLastIndex(x =>
			{
				return x.TimePosition <= frame;
			});
			if (previousIndex == -1)
			{
				IsKeyFrame = false;
				UpdateCurrentValue(null, null, frame);
				return;
			}
			IsKeyFrame = keys[previousIndex].TimePosition == frame;
			int nextIndex = previousIndex + 1;
			if (nextIndex >= keys.Count)
			{
				UpdateCurrentValue(keys[previousIndex], null, frame);
				return;
			}
			UpdateCurrentValue(keys[previousIndex], keys[nextIndex], frame);
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

		private void MoveAnimationKeys(Dictionary<int, int> keysFramesPositions)
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

		private void UpdateKeys()
		{
			UpdateVieModels(Keys, model.AnimationKeys,
				createViewModel: i => new AnimationKeyViewModel(),
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

		private void OnKeyPropertyChanged(object sender, BindablePropertyChangedEventArgs e)
		{
			OnTimeHeadPositionChanged(currentFrame);
		}

		private void UpdateCurrentValue(AnimationKeyViewModel previousKey, AnimationKeyViewModel nextKey, int frame)
		{
			switch (ValueType)
			{
				case AnimationTrack.ValueType.Single:
					if (previousKey == null)
					{
						CurrentSingleValue = 0;
						return;
					}
					if (previousKey.TimePosition == frame || nextKey == null)
					{
						CurrentSingleValue = previousKey.SingleValue;
						return;
					}
					CurrentSingleValue = previousKey.Ease.EasedValue(previousKey.SingleValue, nextKey.SingleValue, T());
					return;
				case AnimationTrack.ValueType.Vector2:
					if (previousKey == null)
					{
						CurrentVector2Value = Vector2.zero;
						return;
					}
					if (previousKey.TimePosition == frame || nextKey == null)
					{
						CurrentVector2Value = previousKey.Vector2Value;
						return;
					}
					CurrentVector2Value = previousKey.Ease.EasedValue(previousKey.Vector2Value, nextKey.Vector2Value, T());
					return;
				case AnimationTrack.ValueType.Vector3:
					if (previousKey == null)
					{
						CurrentVector3Value = Vector3.zero;
						return;
					}
					if (previousKey.TimePosition == frame || nextKey == null)
					{
						CurrentVector3Value = previousKey.Vector3Value;
						return;
					}
					CurrentVector3Value = previousKey.Ease.EasedValue(previousKey.Vector3Value, nextKey.Vector3Value, T());
					return;
				default:
					CurrentSingleValue = 0;
					CurrentVector2Value = Vector2.zero;
					CurrentVector3Value = Vector3.zero;
					break;
			}

			float T()
			{
				return math.remap(previousKey.TimePosition, nextKey.TimePosition, 0f, 1f, frame);
			}
		}

		private void OnStoredSelectedChanged(List<int> newValue)
		{
			int[] toRemove = newValue == null ? SelectedIndexes.ToArray() : SelectedIndexes.Except(newValue).ToArray();
			int[] toAdd = newValue?.Except(SelectedIndexes).ToArray() ?? new int[0];
			toRemove.ForEach(x => SelectedIndexes.RemoveWithoutNotify(x));
			toAdd.ForEach(x => SelectedIndexes.AddWithoutNotify(x));

			SelectedIndexes.NotifyListChanged();
		}

		public static readonly Dictionary<Type, TrackData> TrackTypes = new()
		{
			{ typeof(MoveAnchorTrack), new TrackData(typeof(MoveAnchorTrack), AnimationTrack.ValueType.Vector2, typeof(RectTransform), "move-anchor") },
			{ typeof(FadeTrack),       new TrackData(typeof(FadeTrack),       AnimationTrack.ValueType.Single,  typeof(CanvasGroup),   "fade")        },
		};

		public class TrackData
		{
			public Type Type { get; }
			public AnimationTrack.ValueType ValueType { get; }
			public Type AnimationComponentType { get; }
			public string TrackClass { get; }


			public TrackData(Type type, AnimationTrack.ValueType valueType, Type animationComponentType, string trackClass)
			{
				Type = type;
				ValueType = valueType;
				TrackClass = trackClass;
				AnimationComponentType = animationComponentType;
			}
		}
	}
}