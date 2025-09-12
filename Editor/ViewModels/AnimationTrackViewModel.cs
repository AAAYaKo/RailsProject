using System;
using System.Collections.Generic;
using System.ComponentModel;
using Rails.Runtime.Tracks;
using Unity.Properties;
using UnityEngine;

namespace Rails.Editor.ViewModel
{
	public class AnimationTrackViewModel : BaseNotifyPropertyViewModel<AnimationTrack>
	{
		[CreateProperty]
		public UnityEngine.Object Reference
		{
			get => reference;
			set
			{
				if (reference == value)
					return;
				reference = value;
				NotifyPropertyChanged();
				model.SceneReference = reference;
			}
		}
		[CreateProperty]
		public Type Type => trackData?.AnimationComponentType;
		[CreateProperty]
		public AnimationTrack.ValueType ValueType => trackData?.ValueType ?? AnimationTrack.ValueType.Single;
		[CreateProperty]
		public string TrackClass => trackData?.TrackClass;
		[CreateProperty]
		public ObservableList<AnimationKeyViewModel> Keys => keys;

		private UnityEngine.Object reference;
		private TrackData trackData;
		private ObservableList<AnimationKeyViewModel> keys = new();


		public AnimationTrackViewModel()
		{
		}

		protected override void OnModelChanged()
		{
			if (model == null)
				return;

			Reference = model.SceneReference;
			trackData = TrackTypes[model.GetType()];
			UpdateViewModels(model.AnimationKeys);

			NotifyPropertyChanged(nameof(Type));
			NotifyPropertyChanged(nameof(ValueType));
			NotifyPropertyChanged(nameof(Keys));

			if (model == null)
			{
				if (keys.Count > 0)
					ClearViewModels();
				return;
			}

			UpdateViewModels(model.AnimationKeys);
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(AnimationTrack.SceneReference))
				Reference = model.SceneReference;

			if (e.PropertyName == nameof(AnimationTrack.AnimationKeys))
				UpdateViewModels(model.AnimationKeys);
		}

		private void UpdateViewModels(List<AnimationKey> models)
		{
			if (models == null)
			{
				ClearViewModels();
				return;
			}

			while (Keys.Count < model.AnimationKeys.Count)
			{
				Keys.AddWithoutNotify(new AnimationKeyViewModel());
			}
			while (Keys.Count > model.AnimationKeys.Count)
			{
				var clip = Keys[^1];
				clip.UnbindModel();
				Keys.RemoveWithoutNotify(clip);
			}
			for (int i = 0; i < model.AnimationKeys.Count; i++)
			{
				var clip = model.AnimationKeys[i];
				var viewModel = Keys[i];

				viewModel.UnbindModel();
				viewModel.BindModel(clip);
			}

			Keys.NotifyListChanged();
		}

		private void ClearViewModels()
		{
			foreach (var clip in Keys)
				clip.UnbindModel();
			Keys.Clear();
		}

		public static readonly Dictionary<Type, TrackData> TrackTypes = new()
		{
			{ typeof(MoveAnchorTrack), new TrackData(typeof(MoveAnchorTrack), AnimationTrack.ValueType.Vector2, typeof(RectTransform), "move-anchor") },
			{ typeof(FadeTrack),       new TrackData(typeof(FadeTrack),       AnimationTrack.ValueType.Single,  typeof(CanvasGroup),   "fade")        },
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