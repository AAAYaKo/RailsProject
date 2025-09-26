using System.ComponentModel;
using Rails.Runtime;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Rails.Editor.ViewModel
{
	public class RailsAnimatorViewModel : BaseNotifyPropertyViewModel<RailsAnimator>
	{
		public const string SelectedClipKey = "selectedClip";

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
				if (selectedClip != null)
					selectedClip.propertyChanged -= NotifySelectedClipChanged;
				selectedClip = value;
				selectedClip.propertyChanged += NotifySelectedClipChanged;
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

		[CreateProperty]
		public ICommand<int> ClipSelectCommand
		{
			get => clipSelectCommand;
			set
			{
				if (clipSelectCommand == value)
					return;
				clipSelectCommand = value;
				NotifyPropertyChanged();
			}
		}

		private ObservableList<RailsClipViewModel> clips = new();
		private RailsClipViewModel selectedClip = RailsClipViewModel.Empty;
		private int selectedClipIndex = 0;
		private bool canAddClip;
		private ICommand clipAddCommand;
		private ICommand<int> clipSelectCommand;


		public RailsAnimatorViewModel()
		{
			ClipAddCommand = new RelayCommand(() =>
			{
				EditorContext.Instance.Record("Clip Added");
				model.AddClip();
			});
			ClipSelectCommand = new RelayCommand<int>(x =>
			{
				EditorContext.Instance.Record(EditorContext.Instance.EditorWindow, $"Select Clip: {Clips[x]}");
				EditorContext.Instance.DataStorage.SetInt(SelectedClipKey, x);
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

			selectedClipIndex = EditorContext.Instance.DataStorage.GetInt(SelectedClipKey, 0);
			bool mustResetSelected = false;
			if (SelectedClipIndex >= Clips.Count)
			{
				selectedClipIndex = 0;
				mustResetSelected = true;
			}
			NotifyPropertyChanged(nameof(SelectedClipIndex));
			SelectedClip = Clips.Count > 0 ? Clips[SelectedClipIndex] : RailsClipViewModel.Empty;
			if (mustResetSelected)
				EditorContext.Instance.DataStorage.SetInt(SelectedClipKey, 0);
		}

		protected override void OnUnbind()
		{
			base.OnUnbind();
			ClearViewModels<RailsClipViewModel, RailsClip>(Clips);
		}

		protected override void OnRecordChanged(RecordIntChangedEvent evt)
		{
			if (evt.Key == SelectedClipKey)
				SelectedClipIndex = evt.NextValue;
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

		private void NotifySelectedClipChanged(object sender, BindablePropertyChangedEventArgs e)
		{
			NotifyPropertyChanged(nameof(SelectedClip));
		}
	}
}