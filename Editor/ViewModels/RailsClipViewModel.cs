using System;
using System.Collections.Generic;
using System.ComponentModel;
using Rails.Runtime;
using Rails.Runtime.Tracks;
using Unity.Properties;

namespace Rails.Editor.ViewModel
{
	public class RailsClipViewModel : BaseNotifyPropertyViewModel<RailsClip>
	{
		public static readonly RailsClipViewModel Empty = new()
		{
			CanEdit = false,
		};

		[CreateProperty]
		public string Name
		{
			get => name;
			set
			{
				if (Name == value)
					return;
				name = value;
				NotifyPropertyChanged();
			}
		}
		[CreateProperty]
		public ObservableList<AnimationTrackViewModel> Tracks
		{
			get => tracks;
			set
			{
				if (tracks == value)
					return;
				tracks = value;
				NotifyPropertyChanged();
			}
		}
		[CreateProperty]
		public bool CanEdit
		{
			get => canEdit;
			private set
			{
				if (canEdit == value)
					return;
				canEdit = value;
				NotifyPropertyChanged();
			}
		}

		private string name;
		private ObservableList<AnimationTrackViewModel> tracks = new();
		private bool canEdit = true;


		protected override void OnModelChanged()
		{
			if (model == null)
				return;
			Name = model.Name;
			UpdateViewModels(model.Tracks);
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(RailsClip.Name))
			{
				Name = model.Name;
			}
			if (e.PropertyName == nameof(RailsClip.Tracks))
			{
				UpdateViewModels(model.Tracks);
			}
		}

		public void AddTrack(Type trackType)
		{
			if (trackType == typeof(MoveAnchorTrack))
			{
				model.AddTrack(new MoveAnchorTrack());
			}
			else if (trackType == typeof(FadeTrack))
			{
				model.AddTrack(new FadeTrack());
			}
		}

		public void RemoveTrack(int index)
		{
			model.RemoveTrack(model.Tracks[index]);
		}

		private void UpdateViewModels(List<AnimationTrack> models)
		{
			if (models == null)
			{
				ClearViewModels();
				return;
			}

			while (Tracks.Count < model.Tracks.Count)
			{
				Tracks.AddWithoutNotify(new AnimationTrackViewModel());
			}
			while (Tracks.Count > model.Tracks.Count)
			{
				var clip = Tracks[^1];
				clip.UnbindModel();
				Tracks.RemoveWithoutNotify(clip);
			}
			for (int i = 0; i < model.Tracks.Count; i++)
			{
				var clip = model.Tracks[i];
				var viewModel = Tracks[i];

				viewModel.UnbindModel();
				viewModel.BindModel(clip);
			}

			Tracks.NotifyListChanged();
		}

		private void ClearViewModels()
		{
			foreach (var clip in Tracks)
				clip.UnbindModel();
			Tracks.Clear();
		}
	}
}