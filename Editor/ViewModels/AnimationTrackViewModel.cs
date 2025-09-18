using System;
using System.Collections.Generic;
using System.ComponentModel;
using Rails.Runtime.Tracks;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.ViewModel
{
	public class AnimationTrackViewModel : BaseNotifyPropertyViewModel<AnimationTrack>
	{
		[CreateProperty]
		public UnityEngine.Object Reference
		{
			get => reference;
			set
			{
				if (reference == value)
					return;
				reference = value;
				NotifyPropertyChanged();
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
			set
			{
				if (currentSingleValue == value)
					return;
				currentSingleValue = value;
				NotifyPropertyChanged();
			}
		}
		[CreateProperty]
		public Vector2 CurrentVector2Value
		{
			get => currentVector2Value ?? Vector2.zero;
			set
			{
				if (currentVector2Value == value)
					return;
				currentVector2Value = value;
				NotifyPropertyChanged();
			}
		}
		[CreateProperty]
		public Vector3 CurrentVector3Value
		{
			get => currentVector3Value ?? Vector3.zero;
			set
			{
				if (currentVector3Value == value)
					return;
				currentVector3Value = value;
				NotifyPropertyChanged();
			}
		}
		[CreateProperty]
		public bool IsKeyFrame
		{
			get => isKeyFrame;
			set
			{
				if (isKeyFrame == value)
					return;
				isKeyFrame = value;
				NotifyPropertyChanged();
			}
		}
		[CreateProperty]
		public ObservableList<int> SelectedIndexes
		{
			get => selectedIndexes;
		}

		public event Action SelectionChanged;

		private UnityEngine.Object reference;
		private TrackData trackData;
		private ObservableList<AnimationKeyViewModel> keys = new();
		private float? currentSingleValue;
		private Vector2? currentVector2Value;
		private Vector3? currentVector3Value;
		private bool isKeyFrame;
		private int currentFrame;
		private ObservableList<int> selectedIndexes = new();


		public AnimationTrackViewModel()
		{
			SelectedIndexes.ListChanged += OnSelectionChanged;
		}

		protected override void OnModelChanged()
		{
			if (model == null)
				return;

			Reference = model.SceneReference;
			trackData = TrackTypes[model.GetType()];
			UpdateViewModels(model.AnimationKeys);

			NotifyPropertyChanged(nameof(Type));
			NotifyPropertyChanged(nameof(ValueType));
			NotifyPropertyChanged(nameof(Keys));

			selectedIndexes.Clear();
			NotifyPropertyChanged(nameof(model.AnimationKeys));
			if (model == null)
			{
				if (keys.Count > 0)
					ClearViewModels();
				return;
			}

			UpdateViewModels(model.AnimationKeys);
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(AnimationTrack.SceneReference))
				Reference = model.SceneReference;

			if (e.PropertyName == nameof(AnimationTrack.AnimationKeys))
			{
				UpdateViewModels(model.AnimationKeys);
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

		public void OnKeyFrameButtonClicked()
		{
			int keyIndex = keys.FindIndex(x => x.TimePosition == currentFrame);
			if (keyIndex >= 0)
			{
				EditorContext.Instance.Record("Key Frame Removed");
				if (SelectedIndexes.Contains(keyIndex))
					SelectedIndexes.Remove(keyIndex);
				model.RemoveKey(model.AnimationKeys[keyIndex]);
			}
			else
			{
				EditorContext.Instance.Record("Key Frame Added");
				model.InsertNewKeyAt(currentFrame);
			}
		}

		public void OnValueEdited(ValueEditArgs args)
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
		}

		public void DeselectAll()
		{
			SelectedIndexes.Clear();
		}

		public void MoveAnimationKeys(Dictionary<int, int> keysFramesPositions)
		{
			EditorContext.Instance.Record("Animation Keys Moved");
			model.MoveMultipleKeys(keysFramesPositions);
		}

		private void UpdateViewModels(List<AnimationKey> models)
		{
			if (models == null)
			{
				ClearViewModels();
				return;
			}

			while (Keys.Count < models.Count)
			{
				AnimationKeyViewModel key = new();
				key.propertyChanged += OnKeyPropertyChanged;
				Keys.AddWithoutNotify(key);
			}
			while (Keys.Count > models.Count)
			{
				var key = Keys[^1];
				key.propertyChanged -= OnKeyPropertyChanged;
				key.UnbindModel();
				Keys.RemoveWithoutNotify(key);
			}
			for (int i = 0; i < models.Count; i++)
			{
				var key = models[i];
				var viewModel = Keys[i];

				viewModel.UnbindModel();
				viewModel.BindModel(key);
			}

			Keys.NotifyListChanged();
		}

		private void ClearViewModels()
		{
			foreach (var clip in Keys)
				clip.UnbindModel();
			Keys.Clear();
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

		private void OnSelectionChanged()
		{
			SelectionChanged?.Invoke();
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