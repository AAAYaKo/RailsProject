using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Rails.Runtime.Tracks;
using Unity.Properties;
using UnityEngine;

namespace Rails.Runtime
{
	public class RailsAnimator : MonoBehaviour, INotifyPropertyChanged
#if UNITY_EDITOR
		, ISerializationCallbackReceiver
#endif
	{
		private static readonly CollectionComparer<RailsClip> comparer = new ();

		[SerializeField] private List<RailsClip> clips = new();

		public List<RailsClip> Clips
		{
			get => clips;
			set
			{
				if (comparer.Equals(clips, value))
					return;
				clips = value;
				NotifyPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

#if UNITY_EDITOR
		private readonly List<RailsClip> clipsCopy = new();
		public static event Action<RailsAnimator> AnimatorReset;
#endif

		//Test
		private void Start()
		{
			//RailsClip clip = new RailsClip();
			//clips.Add(clip);
			//clip.AddTrack(track);
			//var event1 = new EventKey
			//{
			//	TimePosition = 15,
			//};
			//event1.AnimationEvent.AddListener(() =>
			//{
			//	Debug.Log("first");
			//});

			//var event2 = new EventKey
			//{
			//	TimePosition = 115,
			//};
			//event2.AnimationEvent.AddListener(() =>
			//{
			//	Debug.Log("second");
			//});

			//clip.EventTrack.AddKey(event1);
			//clip.EventTrack.AddKey(event2);
			//clip.BuildSequence();
		}

#if UNITY_EDITOR
		private void Reset()
		{
			AnimatorReset?.Invoke(this);
		}

		public void OnBeforeSerialize()
		{
			clipsCopy.Clear();
			clipsCopy.AddRange(Clips);
		}

		public void OnAfterDeserialize()
		{
			if (!comparer.Equals(clips, clipsCopy))
				NotifyPropertyChanged(nameof(Clips));

			clipsCopy.Clear();
			clipsCopy.AddRange(Clips);
		}
#endif

		public void AddClip()
		{
			clips.Add(new RailsClip
			{
				Name = $"Clip {clips.Count + 1}",
				Duration = 60,
			});
			NotifyPropertyChanged(nameof(Clips));
		}

		public void RemoveClip(RailsClip clip)
		{
			clips.Remove(clip);
			NotifyPropertyChanged(nameof(Clips));
		}

		private void NotifyPropertyChanged([CallerMemberName] string property = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
	}
}
