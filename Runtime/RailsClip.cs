using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DG.Tweening;
using Rails.Runtime.Tracks;
using UnityEngine;

namespace Rails.Runtime
{
	[Serializable]
	public class RailsClip : INotifyPropertyChanged
#if UNITY_EDITOR
		, ISerializationCallbackReceiver
#endif
	{
		public const float FrameTime = 1f / 60;
		public const int Fps = 60;
		private static readonly CollectionComparer<AnimationTrack> comparer = new();

		[SerializeReference] private List<AnimationTrack> tracks = new();
		[SerializeField] private EventsTrack eventTrack = new();
		[SerializeField] private int duration; //in frames
		[SerializeField] private string name;

		public event PropertyChangedEventHandler PropertyChanged;

		public List<AnimationTrack> Tracks
		{
			get => tracks;
			set
			{
				if (comparer.Equals(tracks, value))
					return;
				tracks = value;
				NotifyPropertyChanged();
			}
		}
		public EventsTrack EventTrack { get => eventTrack; set => eventTrack = value; }
		public int Duration
		{
			get => duration;
			set
			{
				if (duration == value)
					return;
				duration = value;
				NotifyPropertyChanged();
			}
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

		protected void NotifyPropertyChanged([CallerMemberName] string property = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}

#if UNITY_EDITOR
		public void OnBeforeSerialize()
		{
			tracksCopy.Clear();
			tracksCopy.AddRange(Tracks);
			durationCopy = Duration;
			nameCopy = Name;
		}

		public void OnAfterDeserialize()
		{
			if (!comparer.Equals(tracksCopy, Tracks))
				NotifyPropertyChanged(nameof(Tracks));
			if (durationCopy != Duration)
				NotifyPropertyChanged(nameof(Duration));
			if (nameCopy != Name)
				NotifyPropertyChanged(nameof(Name));

			tracksCopy.Clear();
			tracksCopy.AddRange(Tracks);
			durationCopy = Duration;
			nameCopy = Name;
		}
#endif
	}
}
