using System;
using System.Collections.Generic;
using System.ComponentModel;
using DG.Tweening;
using UnityEngine;

namespace Rails.Runtime
{
	[Serializable]
	public class RailsClip : INotifyPropertyChanged
	{
		private const float frameTime = 1f / 60;

		[SerializeReference] private List<AnimationTrack> tracks = new();
		[SerializeField] private EventsTrack eventTrack = new();
		[SerializeField] private int length; //in frames
		[SerializeField] private string name;

		public event PropertyChangedEventHandler PropertyChanged;

		public List<AnimationTrack> Tracks { get => tracks; set => tracks = value; }
		public EventsTrack EventTrack { get => eventTrack; set => eventTrack = value; }
		public int Length { get => length; set => length = value; }
		public string Name { get => name; set => name = value; }

		public Sequence BuildSequence()
		{
			var sequence = DOTween.Sequence();
			foreach (var track in Tracks)
				track.InsertInSequence(sequence, frameTime);
			EventTrack.InsertInSequence(sequence, frameTime);
			return sequence;
		}

		public void AddTrack(AnimationTrack track)
		{
			Tracks.Add(track);
		}

		public void RemoveTrack(AnimationTrack track)
		{
			Tracks.Remove(track);
		}
	}
}
