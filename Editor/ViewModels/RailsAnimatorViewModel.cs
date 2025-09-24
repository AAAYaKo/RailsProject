using System.Collections.Generic;
using System.ComponentModel;
using Rails.Runtime;
using Unity.Properties;

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
				EditorContext.Instance.NotifySelectedClipChanged(selectedClipIndex);
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
		[CreateProperty]
		public ICommand ClipAddCommand
		{
			get => clipAddCommand;
			set
			{
				if (clipAddCommand == value)
					return;
				clipAddCommand = value;
				NotifyPropertyChanged();
			}
		}

		private ObservableList<RailsClipViewModel> clips = new();
		private RailsClipViewModel selectedClip = RailsClipViewModel.Empty;
		private int selectedClipIndex = 0;
		private bool canAddClip;
		private ICommand clipAddCommand;


		public RailsAnimatorViewModel()
		{
			ClipAddCommand = new RelayCommand(() =>
			{
				EditorContext.Instance.Record("Clip Added");
				model.AddClip();
			});
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(RailsAnimator.Clips))
			{
				UpdateClips();
			}
		}

		protected override void OnModelChanged()
		{
			CanAddClip = model != null;
			if (model == null)
			{
				if (clips.Count > 0)
					ClearViewModels<RailsClipViewModel, RailsClip>(Clips);
				SelectedClip = RailsClipViewModel.Empty;
				return;
			}

			UpdateClips();

			if (SelectedClipIndex >= Clips.Count)
			{
				selectedClipIndex = 0;
				NotifyPropertyChanged(nameof(SelectedClipIndex));
			}
			SelectedClip = Clips.Count > 0 ? Clips[SelectedClipIndex] : RailsClipViewModel.Empty;
		}

		private void UpdateClips()
		{
			UpdateVieModels(Clips, model.Clips,
				createViewModel: () => new RailsClipViewModel(),
				viewModelBindCallback: (vm, m) =>
				{
					vm.RemoveCommand = new RelayCommand(() =>
					{
						model.RemoveClip(m);
					});
				});
		}
	}
}