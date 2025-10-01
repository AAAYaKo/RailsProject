using System.ComponentModel;
using Rails.Runtime;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.ViewModel
{
	public class RailsAnimatorViewModel : BaseNotifyPropertyViewModel<RailsAnimator>
	{
		public const string SelectedClipKey = "selectedClip";
		private static readonly CollectionComparer<RailsClipViewModel> clipsComparer = new();

		[CreateProperty]
		public ObservableList<RailsClipViewModel> Clips
		{
			get => clips;
			set => SetProperty(ref clips, value, clipsComparer);
		}
		[CreateProperty]
		public bool CanAddClip
		{
			get => canAddClip;
			set => SetProperty(ref canAddClip, value);
		}
		[CreateProperty]
		public int SelectedClipIndex
		{
			get => selectedClipIndex;
			set
			{
				SetProperty(ref selectedClipIndex, value);
				SelectedClip = Clips.Count > 0 && selectedClipIndex >= 0 ? Clips[selectedClipIndex] : RailsClipViewModel.Empty;
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
			set => SetProperty(ref clipAddCommand, value);
		}

		[CreateProperty]
		public ICommand<int> ClipSelectCommand
		{
			get => clipSelectCommand;
			set => SetProperty(ref clipSelectCommand, value);
		}

		private ObservableList<RailsClipViewModel> clips = new();
		private RailsClipViewModel selectedClip = RailsClipViewModel.Empty;
		private int selectedClipIndex = 0;
		private StoredInt storedSelectedIndex = new(SelectedClipKey);
		private bool canAddClip;
		private ICommand clipAddCommand;
		private ICommand<int> clipSelectCommand;


		public RailsAnimatorViewModel()
		{
			ClipAddCommand = new RelayCommand(() =>
			{
				EditorContext.Instance.Record("Clip Added");
				model.AddClip();

				if (storedSelectedIndex.Value >= Clips.Count)
					storedSelectedIndex.Value = 0;
				SelectedClipIndex = storedSelectedIndex.Value;
			});
			ClipSelectCommand = new RelayCommand<int>(x =>
			{
				EditorContext.Instance.Record(EditorContext.Instance.EditorWindow, $"Select Clip: {Clips[x].Name}");
				storedSelectedIndex.Value = x;
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

			if (storedSelectedIndex.Value >= Clips.Count || storedSelectedIndex.Value < 0)
			{
				if (Clips.Count != 0)
					storedSelectedIndex.Value = 0;
				else
					storedSelectedIndex.Value = -1;
			}

			SelectedClipIndex = storedSelectedIndex.Value;
		}

		protected override void OnBind()
		{
			base.OnBind();
			storedSelectedIndex.Bind(EditorContext.Instance.DataStorage.RecordsInt);
			storedSelectedIndex.ValueChanged += OnStoredIndexChanged;
		}

		protected override void OnUnbind()
		{
			base.OnUnbind();
			ClearViewModels<RailsClipViewModel, RailsClip>(Clips);
			storedSelectedIndex.Unbind();
			storedSelectedIndex.ValueChanged -= OnStoredIndexChanged;
		}

		private void OnStoredIndexChanged(int value)
		{
			SelectedClipIndex = value;
		}

		private void UpdateClips()
		{
			UpdateVieModels(Clips, model.Clips,
				createViewModel: i => new RailsClipViewModel(),
				viewModelBindCallback: (vm, m) =>
				{
					vm.RemoveCommand = new RelayCommand(() =>
					{
						EditorContext.Instance.Record("Clip Removed");
						model.RemoveClip(m);

						if (storedSelectedIndex.Value >= Clips.Count || storedSelectedIndex.Value < 0)
						{
							if (Clips.Count != 0)
								storedSelectedIndex.Value = 0;
							else
								storedSelectedIndex.Value = -1;
						}

						SelectedClipIndex = storedSelectedIndex.Value;
					});
				});
		}

		private void NotifySelectedClipChanged(object sender, BindablePropertyChangedEventArgs e)
		{
			NotifyPropertyChanged(nameof(SelectedClip));
		}
	}
}