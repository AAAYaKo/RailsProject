﻿using System;
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

#if UNITY_EDITOR
		private readonly List<AnimationTrack> tracksCopy = new();
		private int durationCopy;
		private string nameCopy;
#endif


		public Sequence BuildSequence()
		{
			var sequence = DOTween.Sequence();
			foreach (var track in Tracks)
				track.InsertInSequence(sequence, FrameTime);
			EventTrack.InsertInSequence(sequence, FrameTime);
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
#endif
		}
	}
}
