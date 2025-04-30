using System.Collections.Generic;
using System.ComponentModel;
using Rails.Runtime;
using Unity.Properties;
using UnityEngine;

namespace Rails.Editor.ViewModel
{
	public class RailsAnimatorViewModel : BaseNotifyPropertyViewModel<RailsAnimator>
	{
		[CreateProperty]
		public ObservableList<RailsClipViewModel> Clips
		{
			get => clips;
			set
			{
				if (clips == value)
					return;
				clips = value;
				NotifyPropertyChanged();
			}
		}
		[CreateProperty]
		public bool CanAddClip
		{
			get => canAddClip;
			set
			{
				if (canAddClip == value)
					return;
				canAddClip = value;
				NotifyPropertyChanged();
			}
		}
		[CreateProperty]
		public int SelectedClipIndex
		{
			get => selectedClipIndex;
			set
			{
				if (selectedClipIndex == value)
					return;
				selectedClipIndex = value;
				NotifyPropertyChanged();
				SelectedClip = Clips[SelectedClipIndex];
			}
		}
		[CreateProperty]
		public RailsClipViewModel SelectedClip
		{
			get => selectedClip;
			set
			{
				if (selectedClip == value)
					return;
				selectedClip?.Deselect();
				selectedClip = value;
				selectedClip.Select(() => NotifyPropertyChanged(nameof(SelectedClip)));
				NotifyPropertyChanged();
			}
		}

		private ObservableList<RailsClipViewModel> clips = new();
		private RailsClipViewModel selectedClip = RailsClipViewModel.Empty;
		private int selectedClipIndex = 0;
		private bool canAddClip;


		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(RailsAnimator.Clips))
			{
				UpdateViewModels(model.Clips);
			}
		}

		protected override void OnModelChanged()
		{
			CanAddClip = model != null;
			if (model == null)
			{
				if (clips.Count > 0)
					ClearViewModels();
				SelectedClip = RailsClipViewModel.Empty;
				return;
			}

			UpdateViewModels(model.Clips);

			if (SelectedClipIndex >= Clips.Count)
			{
				selectedClipIndex = 0;
				NotifyPropertyChanged(nameof(SelectedClipIndex));
			}
			SelectedClip = Clips.Count > 0 ? Clips[SelectedClipIndex] : RailsClipViewModel.Empty;
		}

		public void AddClip()
		{
			EditorContext.Instance.Record("Clip Added");
			model.AddClip();
		}

		public void RemoveClip(int index)
		{
			EditorContext.Instance.Record("Clip Removed");
			model.RemoveClip(model.Clips[index]);
		}

		private void UpdateViewModels(List<RailsClip> models)
		{
			if (models == null)
			{
				ClearViewModels();
				return;
			}

			while (Clips.Count < model.Clips.Count)
			{
				Clips.AddWithoutNotify(new RailsClipViewModel());
			}
			while (Clips.Count > model.Clips.Count)
			{
				var clip = Clips[^1];
				clip.UnbindModel();
				Clips.RemoveWithoutNotify(clip);
			}
			for (int i = 0; i < model.Clips.Count; i++)
			{
				var clip = model.Clips[i];
				var viewModel = Clips[i];

				viewModel.UnbindModel();
				viewModel.BindModel(clip);
			}

			Clips.NotifyListChanged();
		}

		private void ClearViewModels()
		{
			foreach (var clip in Clips)
				clip.UnbindModel();
			Clips.Clear();
		}
	}
}