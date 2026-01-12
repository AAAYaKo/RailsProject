using System;
using System.Collections.Generic;
using DG.Tweening;
using Rails.Runtime.Tracks;
using UnityEngine;

namespace Rails.Runtime
{
	[Serializable]
	public class RailsClip : BaseSerializableNotifier
	{
		public const float FrameTime = 1f / 60;
		public const int Fps = 60;
		private static readonly CollectionComparer<AnimationTrack> comparer = new();

		[SerializeReference] private List<AnimationTrack> tracks = new();
		[SerializeField] private EventsTrack eventTrack = new();
		[SerializeField] private int duration; //in frames
		[SerializeField] private string name;
		[SerializeField] private LoopType loopType = LoopType.Restart;
		[SerializeField] private int loopCount = 1;
		[SerializeField] private bool isFullDuration = true;

		public List<AnimationTrack> Tracks
		{
			get => tracks;
			set => SetProperty(ref tracks, value);
		}
		public EventsTrack EventTrack => eventTrack;
		public int Duration
		{
			get => duration;
			set => SetProperty(ref duration, value);
		}
		public string Name { get => name; set => name = value; }
		public LoopType LoopType
		{
			get => loopType;
			set => SetProperty(ref loopType, value);
		}
		public int LoopCount
		{
			get => loopCount;
			set => SetProperty(ref loopCount, value);
		}
		public bool IsFullDuration
		{
			get => isFullDuration;
			set => SetProperty(ref isFullDuration, value);
		}

#if UNITY_EDITOR
		[NonSerialized] private readonly List<AnimationTrack> tracksCopy = new();
		[NonSerialized] private int durationCopy;
		[NonSerialized] private string nameCopy;
		[NonSerialized] private LoopType loopTypeCopy;
		[NonSerialized] private int loopCountCopy;
		[NonSerialized] private bool isFullDurationCopy;
#endif


		public Tween BuildSequence()
		{
			var sequence = DOTween.Sequence();
			if (isFullDuration)
				sequence.AppendInterval(Duration * FrameTime);
			foreach (var track in Tracks)
				track.InsertInSequence(sequence, FrameTime);
			EventTrack.InsertInSequence(sequence, FrameTime);
			sequence.SetLoops(LoopCount, LoopType);
			return sequence;
		}

		public void AddTrack(AnimationTrack track)
		{
			Tracks.Add(track);
			NotifyPropertyChanged(nameof(Tracks));
		}

		public void RemoveTrack(AnimationTrack track)
		{
			Tracks.Remove(track);
			NotifyPropertyChanged(nameof(Tracks));
		}

		public override void OnBeforeSerialize()
		{
#if UNITY_EDITOR
			CopyList(tracks, tracksCopy);
			durationCopy = Duration;
			nameCopy = Name;
			loopTypeCopy = LoopType;
			loopCountCopy = LoopCount;
			isFullDurationCopy = IsFullDuration;
#endif
		}

		public override void OnAfterDeserialize()
		{
#if UNITY_EDITOR
			if (NotifyIfChanged(Tracks, tracksCopy, nameof(Tracks), comparer))
				CopyList(tracks, tracksCopy);
			if (NotifyIfChanged(Duration, durationCopy, nameof(Duration)))
				durationCopy = Duration;
			if (NotifyIfChanged(Name, nameCopy, nameof(Name)))
				nameCopy = Name;
			if (NotifyIfChanged(LoopType, loopTypeCopy, nameof(LoopType)))
				loopTypeCopy = LoopType;
			if (NotifyIfChanged(LoopCount, loopCountCopy, nameof(LoopCount)))
				loopCountCopy = LoopCount;
			if (NotifyIfChanged(IsFullDuration, isFullDurationCopy, nameof(IsFullDuration)))
				isFullDurationCopy = IsFullDuration;
#endif
		}
	}
}
