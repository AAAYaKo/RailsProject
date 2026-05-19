using System;
using System.Collections.Generic;
using System.ComponentModel;
using Rails.Editor.Context;
using Rails.Runtime.Tracks;
using Unity.Mathematics;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.ViewModel
{
	public class AnimationTrackViewModel : BaseTrackViewModel<IAnimationTrack, IAnimationKey, AnimationKeyViewModel>
	{
		public const string RandomColorClass = "random-color-";
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
		public IAnimationTrack.ValueType ValueType => trackData?.ValueType ?? IAnimationTrack.ValueType.Single;
		[CreateProperty]
		public override string TrackClass
		{
			get
			{
				if (useRandomColor)
					return RandomColorClass + (trackIndex % 8);
				return trackData?.TrackClass;
			}
		}
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
		public bool CurrentHasDriver
		{
			get => hasDriver ?? false;
			set => SetProperty(ref hasDriver, value);
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
		public string ClipProperty { get; set; }

		private UnityEngine.Object reference;
		private TrackData trackData;
		private float? currentSingleValue;
		private Vector2? currentVector2Value;
		private Vector3? currentVector3Value;
		private bool? constrainedProportions;
		private bool? hasDriver;
		private ICommand removeCommand;
		private ICommand keyFrameAddCommand;
		private ICommand keyFrameRemoveCommand;
		private ICommand<UnityEngine.Object> changeReferenceCommand;
		private ICommand<ValueEditArgs> valueEditCommand;
		private ICommand<bool> constrainedProportionsChangeCommand;
		private int trackIndex;
		private bool useRandomColor;


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
					if (ValueType is IAnimationTrack.ValueType.Single)
						model.AnimationKeys[keyIndex].Value = args.SingleValue;
					else if (ValueType is IAnimationTrack.ValueType.Vector2)
						model.AnimationKeys[keyIndex].Value = args.Vector2Value;
					else if (ValueType is IAnimationTrack.ValueType.Vector3)
						model.AnimationKeys[keyIndex].Value = args.Vector3Value;
				}
				else
				{
					EditorContext.Instance.Record("Key Frame Added");
					if (ValueType is IAnimationTrack.ValueType.Single)
						model.InsertNewKeyAt(currentFrame, args.SingleValue, constrainedProportions.Value);
					else if (ValueType is IAnimationTrack.ValueType.Vector2)
						model.InsertNewKeyAt(currentFrame, args.Vector2Value, constrainedProportions.Value);
					else if (ValueType is IAnimationTrack.ValueType.Vector3)
						model.InsertNewKeyAt(currentFrame, args.Vector3Value, constrainedProportions.Value);
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
				else
				{
					EditorContext.Instance.Record("Key Frame Added");
					if (ValueType is IAnimationTrack.ValueType.Single)
						model.InsertNewKeyAt(currentFrame, currentSingleValue, x);
					else if (ValueType is IAnimationTrack.ValueType.Vector2)
						model.InsertNewKeyAt(currentFrame, currentVector2Value, x);
					else if (ValueType is IAnimationTrack.ValueType.Vector3)
						model.InsertNewKeyAt(currentFrame, currentVector3Value, x);
				}
			});

			ChangeReferenceCommand = new RelayCommand<UnityEngine.Object>(x =>
			{
				EditorContext.Instance.Record("Track Reference Changed");
				model.SceneReference = x;
			}, x => x == null || CheckReference(x));
			this.trackIndex = trackIndex;
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
				CurrentHasDriver = false;
				UpdateCurrentValue(null, null, frame);
				return;
			}
			IsKeyFrame = Keys[previousIndex].TimePosition == frame;
			CurrentConstrainedProportions = IsKeyFrame && Keys[previousIndex].ConstrainedProportions;
			CurrentHasDriver = IsKeyFrame && Keys[previousIndex].HasDriver;
			int nextIndex = previousIndex + 1;
			UpdateCurrentValue(Keys[previousIndex], nextIndex >= Keys.Count ? null : Keys[nextIndex], frame);
		}

		public void SetUseRandomColor(bool use)
		{
			useRandomColor = use;
			NotifyPropertyChanged(nameof(TrackClass));
		}

		protected override void OnModelChanged()
		{
			if (model != null)
				trackData = TrackTypes[model.GetType()];

			base.OnModelChanged();

			if (model == null)
				return;

			Reference = model.SceneReference;
			NotifyPropertyChanged(nameof(Type));
			NotifyPropertyChanged(nameof(ValueType));
		}

		protected override void OnModelPropertyChanged(object sender, string propertyName)
		{
			base.OnModelPropertyChanged(sender, propertyName);
			if (propertyName == nameof(IAnimationTrack.SceneReference))
			{
				Reference = model.SceneReference;
				OnTimeHeadPositionChanged(currentFrame);
			}
		}

		private void UpdateCurrentValue(AnimationKeyViewModel previousKey, AnimationKeyViewModel nextKey, int frame)
		{
			switch (ValueType)
			{
				case IAnimationTrack.ValueType.Single:
					if (previousKey == null)
					{
						CurrentSingleValue = (float)(model?.GetCurrentValue() ?? 0);
						return;
					}
					if (previousKey.TimePosition == frame || nextKey == null)
					{
						CurrentSingleValue = previousKey.SingleValue;
						return;
					}
					CurrentSingleValue = previousKey.Ease.EasedValue(previousKey.SingleValue, nextKey.SingleValue, T());
					return;
				case IAnimationTrack.ValueType.Vector2:
					if (previousKey == null)
					{
						CurrentVector2Value = (Vector2)(model?.GetCurrentValue() ?? Vector2.zero);
						return;
					}
					if (previousKey.TimePosition == frame || nextKey == null)
					{
						CurrentVector2Value = previousKey.Vector2Value;
						return;
					}
					CurrentVector2Value = previousKey.Ease.EasedValue(previousKey.Vector2Value, nextKey.Vector2Value, T());
					return;
				case IAnimationTrack.ValueType.Vector3:
					if (previousKey == null)
					{
						CurrentVector3Value = (Vector3)(model?.GetCurrentValue() ?? Vector2.zero);
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
			return new AnimationKeyViewModel(index, new RelayCommand<AnimationTime>(x =>
			{
				Dictionary<int, int> keysFramesPositions = new()
				{
					{ index, x.Frames }
				};
				MoveKeys(keysFramesPositions);
			}));
		}

		protected override void OnKeyBind(AnimationKeyViewModel vm, IAnimationKey m)
		{
			base.OnKeyBind(vm, m);
			vm.Reference = Reference;
		}

		protected override void OnKeyPreBind(AnimationKeyViewModel vm, IAnimationKey m)
		{
			base.OnKeyPreBind(vm, m);
			vm.ValueType = ValueType;
			vm.TrackProperty = $"{ClipProperty}.tracks.Array.data[{trackIndex}]";
		}

		protected override void OnKeyPropertyChanged(object sender, BindablePropertyChangedEventArgs e)
		{
			base.OnKeyPropertyChanged(sender, e);
			Keys.NotifyListChanged();
		}

		public static readonly Dictionary<Type, TrackData> TrackTypes = new()
		{
			{ typeof(MoveAnchorTrack), new TrackData(typeof(MoveAnchorTrack), IAnimationTrack.ValueType.Vector2, typeof(RectTransform), "move-anchor") },
			{ typeof(FadeTrack),       new TrackData(typeof(FadeTrack),       IAnimationTrack.ValueType.Single,  typeof(CanvasGroup),   "fade")        },
			{ typeof(RotateTrack),     new TrackData(typeof(RotateTrack),     IAnimationTrack.ValueType.Vector3,  typeof(Transform),    "rotate")      },
			{ typeof(ScaleTrack),      new TrackData(typeof(ScaleTrack),      IAnimationTrack.ValueType.Vector3,  typeof(Transform),    "scale")       },
		};

		public class TrackData
		{
			public Type Type { get; }
			public IAnimationTrack.ValueType ValueType { get; }
			public Type AnimationComponentType { get; }
			public string TrackClass { get; }


			public TrackData(Type type, IAnimationTrack.ValueType valueType, Type animationComponentType, string trackClass)
			{
				Type = type;
				ValueType = valueType;
				TrackClass = trackClass;
				AnimationComponentType = animationComponentType;
			}
		}
	}
}