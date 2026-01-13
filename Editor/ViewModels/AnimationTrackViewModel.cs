using System;
using System.Collections.Generic;
using System.ComponentModel;
using Rails.Editor.Context;
using Rails.Runtime.Tracks;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.ViewModel
{
	public class AnimationTrackViewModel : BaseTrackViewModel<AnimationTrack, AnimationKey, AnimationKeyViewModel>
	{
		[CreateProperty]
		public UnityEngine.Object Reference
		{
			get => reference;
			set
			{
				if (SetProperty(ref reference, value))
					Keys.ForEach(x => x.Reference = reference);
			}
		}
		[CreateProperty]
		public Type Type => trackData?.AnimationComponentType;
		[CreateProperty]
		public Type TrackType => trackData?.Type;
		[CreateProperty]
		public AnimationTrack.ValueType ValueType => trackData?.ValueType ?? AnimationTrack.ValueType.Single;
		[CreateProperty]
		public override string TrackClass => trackData?.TrackClass;
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
		public bool CurrentConstrainedProportions
		{
			get => constrainedProportions ?? false;
			set => SetProperty(ref constrainedProportions, value);
		}
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
		public ICommand<bool> ConstrainedProportionsChangeCommand
		{
			get => constrainedProportionsChangeCommand;
			set => SetProperty(ref constrainedProportionsChangeCommand, value);
		}
		[CreateProperty]
		public ICommand<UnityEngine.Object> ChangeReferenceCommand
		{
			get => changeReferenceCommand;
			set => SetProperty(ref changeReferenceCommand, value);
		}
		public Predicate<UnityEngine.Object> CheckReference { get; set; }

		private UnityEngine.Object reference;
		private TrackData trackData;
		private float? currentSingleValue;
		private Vector2? currentVector2Value;
		private Vector3? currentVector3Value;
		private bool? constrainedProportions;
		private ICommand removeCommand;
		private ICommand keyFrameAddCommand;
		private ICommand keyFrameRemoveCommand;
		private ICommand<UnityEngine.Object> changeReferenceCommand;
		private ICommand<ValueEditArgs> valueEditCommand;
		private ICommand<bool> constrainedProportionsChangeCommand;

		public AnimationTrackViewModel(int trackIndex) : base()
		{
			storedSelectedIndexes = new StoredIntList(StoreKey + trackIndex);

			KeyFrameRemoveCommand = new RelayCommand(() => RemoveKey(currentFrame));
			KeyFrameAddCommand = new RelayCommand(() => AddKey(currentFrame));

			ValueEditCommand = new RelayCommand<ValueEditArgs>(args =>
			{
				int keyIndex = Keys.FindIndex(x => x.TimePosition == currentFrame);
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

			ConstrainedProportionsChangeCommand = new RelayCommand<bool>(x =>
			{
				int keyIndex = Keys.FindIndex(x => x.TimePosition == currentFrame);
				if (keyIndex >= 0)
				{
					string recordName = x ? "Key Constrained Proportions Enabled" : "Key Constrained Proportions Disabled";
					EditorContext.Instance.Record(recordName);
					model.AnimationKeys[keyIndex].ConstrainedProportions = x;
				}
			});

			ChangeReferenceCommand = new RelayCommand<UnityEngine.Object>(x =>
			{
				EditorContext.Instance.Record("Track Reference Changed");
				model.SceneReference = x;
			}, x => x != null && CheckReference(x));
		}

		public override void OnTimeHeadPositionChanged(int frame)
		{
			currentFrame = frame;
			int previousIndex = Keys.FindLastIndex(x =>
			{
				return x.TimePosition.Frames <= frame;
			});
			if (previousIndex == -1)
			{
				IsKeyFrame = false;
				CurrentConstrainedProportions = false;
				UpdateCurrentValue(null, null, frame);
				return;
			}
			IsKeyFrame = Keys[previousIndex].TimePosition == frame;
			CurrentConstrainedProportions = IsKeyFrame && Keys[previousIndex].ConstrainedProportions;
			int nextIndex = previousIndex + 1;
			UpdateCurrentValue(Keys[previousIndex], nextIndex >= Keys.Count ? null : Keys[nextIndex], frame);
		}

		protected override void OnModelChanged()
		{
			if (model != null)
				trackData = TrackTypes[model.GetType()];

			base.OnModelChanged();

			if (model == null)
				return;

			Reference = model.SceneReference;

			keys.ForEach(x => x.TrackClass = trackData.TrackClass);

			NotifyPropertyChanged(nameof(Type));
			NotifyPropertyChanged(nameof(ValueType));
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnModelPropertyChanged(sender, e);
			if (e.PropertyName == nameof(AnimationTrack.SceneReference))
			{
				Reference = model.SceneReference;
				EventBus.Publish(new ClipChangedEvent());
			}
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

		protected override AnimationKeyViewModel CreateKey(int index)
		{
			return new AnimationKeyViewModel(TrackClass, index, new RelayCommand<AnimationTime>(x =>
			{
				Dictionary<int, int> keysFramesPositions = new()
				{
					{ index, x.Frames }
				};
				MoveKeys(keysFramesPositions);
			}))
			{
				Reference = Reference,
				ValueType = ValueType,
			};
		}

		protected override void OnKeyPropertyChanged(object sender, BindablePropertyChangedEventArgs e)
		{
			base.OnKeyPropertyChanged(sender, e);
			UpdateKeys();
		}

		public static readonly Dictionary<Type, TrackData> TrackTypes = new()
		{
			{ typeof(MoveAnchorTrack), new TrackData(typeof(MoveAnchorTrack), AnimationTrack.ValueType.Vector2, typeof(RectTransform), "move-anchor") },
			{ typeof(FadeTrack),       new TrackData(typeof(FadeTrack),       AnimationTrack.ValueType.Single,  typeof(CanvasGroup),   "fade")        },
			{ typeof(RotateTrack),     new TrackData(typeof(RotateTrack),     AnimationTrack.ValueType.Vector3,  typeof(Transform),    "rotate")      },
			{ typeof(ScaleTrack),      new TrackData(typeof(ScaleTrack),      AnimationTrack.ValueType.Vector3,  typeof(Transform),    "scale")       },
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