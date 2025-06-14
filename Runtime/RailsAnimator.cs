﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Rails.Runtime.Tracks;
using Unity.Properties;
using UnityEngine;

namespace Rails.Runtime
{
	public class RailsAnimator : MonoBehaviour, INotifyPropertyChanged
	{
		[SerializeField] private List<RailsClip> clips = new();

		[CreateProperty]
		public List<RailsClip> Clips
		{
			get => clips;
			set
			{
				if (Utils.ListEquals(clips, value))
					return;
				clips = value;
				NotifyPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

#if UNITY_EDITOR
		public static event Action<RailsAnimator> AnimatorReset;
#endif

		//Test
		private void Start()
		{
			//MoveAnchorTrack track = new MoveAnchorTrack();
			//track.animationComponent = (RectTransform)transform;
			//track.AddKey(new AnimationKey
			//{
			//	TimePosition = 0,
			//	Vector2Value = new Vector2(0, 0),
			//	Ease = new RailsEase
			//	{
			//		Type = RailsEase.EaseType.NoAnimation,
			//	}
			//});

			//track.AddKey(new AnimationKey
			//{
			//	TimePosition = 15,
			//	Vector2Value = new Vector2(100, 0),
			//	Ease = new RailsEase
			//	{
			//		Type = RailsEase.EaseType.EaseFunction,
			//		EaseFunc = DG.Tweening.Ease.InOutSine,
			//	}
			//});

			//track.AddKey(new AnimationKey
			//{
			//	TimePosition = 75,
			//	Vector2Value = new Vector2(200, 150),
			//	Ease = new RailsEase
			//	{
			//		Type = RailsEase.EaseType.EaseCurve,
			//		Controls = new Unity.Mathematics.float4(1 / 5f, 1 / 3f, 0, 1),
			//	}
			//});

			//track.AddKey(new AnimationKey
			//{
			//	TimePosition = 90,
			//	Vector2Value = new Vector2(400, 100),
			//	Ease = new RailsEase
			//	{
			//		Type = RailsEase.EaseType.NoAnimation,
			//	}
			//});

			//track.AddKey(new AnimationKey
			//{
			//	TimePosition = 120,
			//	Vector2Value = new Vector2(50, 50),
			//	Ease = new RailsEase
			//	{
			//		Type = RailsEase.EaseType.NoAnimation,
			//	}
			//});

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
		private void OnValidate()
		{
			NotifyPropertyChanged(nameof(Clips));
		}

		private void Reset()
		{
			AnimatorReset?.Invoke(this);
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
