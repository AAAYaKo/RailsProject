using System;
using System.Collections.Generic;
using DG.Tweening;
using Rails.Runtime.Tracks;
using Unity.Properties;
using UnityEngine;

namespace Rails.Runtime
{
	[Serializable]
	public class RailsClip
	{
		public const float FrameTime = 1f / 60;
		public const int Fps = 60;
		private static readonly CollectionComparer<IAnimationTrack> comparer = new();

		[SerializeReference, DontCreateProperty] private List<IAnimationTrack> tracks = new();
		[SerializeField, DontCreateProperty] private EventsTrack eventTrack = new();
		[SerializeField, DontCreateProperty] private int duration; //in frames
		[SerializeField, DontCreateProperty] private string name;
		[SerializeField, DontCreateProperty] private LoopType loopType = LoopType.Restart;
		[SerializeField, DontCreateProperty] private int loopCount = 1;
		[SerializeField, DontCreateProperty] private bool isFullDuration = true;

		[CreateProperty]
		public List<IAnimationTrack> Tracks
		{
			get => tracks;
			set => tracks = value;
		}
		[CreateProperty]
		public EventsTrack EventTrack => eventTrack;
		[CreateProperty]
		public int Duration
		{
			get => duration;
			set => duration = value;
		}
		[CreateProperty]
		public string Name
		{
			get => name;
			set => name = value;
		}
		[CreateProperty]
		public LoopType LoopType
		{
			get => loopType;
			set => loopType = value;
		}
		[CreateProperty]
		public int LoopCount
		{
			get => loopCount;
			set => loopCount = value;
		}
		[CreateProperty]
		public bool IsFullDuration
		{
			get => isFullDuration;
			set => isFullDuration = value;
		}


		public Tween BuildSequence(bool recomputeDrivers = true)
		{
			var sequence = DOTween.Sequence();
			if (isFullDuration)
				sequence.AppendInterval(Duration * FrameTime);
			foreach (var track in Tracks)
				track.InsertInSequence(sequence, FrameTime, recomputeDrivers);
			EventTrack.InsertInSequence(sequence, FrameTime, recomputeDrivers);
			sequence.SetLoops(LoopCount, LoopType);
			return sequence;
		}

		public void RecomputeDrivers()
		{
			foreach (var track in Tracks)
				track.RecomputeDrivers();
		}

		public void SaveCurrentValue()
		{
			foreach (var track in Tracks)
				track.SaveCurrentValue();
		}

		public void RestoreValue()
		{
			foreach (var track in Tracks)
				track.RestoreValue();
		}

		public void AddTrack(IAnimationTrack track)
		{
			Tracks.Add(track);
		}

		public void RemoveTrack(IAnimationTrack track)
		{
			Tracks.Remove(track);
		}
	}
}
