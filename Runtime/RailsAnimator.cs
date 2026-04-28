using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace Rails.Runtime
{
	public class RailsAnimator : MonoBehaviour
	{
		private static readonly CollectionComparer<RailsClip> comparer = new();

		[SerializeField, DontCreateProperty] private List<RailsClip> clips = new();

		[CreateProperty]
		public List<RailsClip> Clips
		{
			get => clips;
			set
			{
				if (comparer.Equals(clips, value))
					return;
				clips = value;
				//NotifyPropertyChanged();
			}
		}


#if UNITY_EDITOR
		public static event Action<RailsAnimator> AnimatorReset;
#endif


#if UNITY_EDITOR
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
		}

		public void RemoveClip(RailsClip clip)
		{
			clips.Remove(clip);
		}
	}
}
