using System.Collections.Generic;
using System.ComponentModel;
using Rails.Runtime;
using Unity.Properties;

namespace Rails.Editor.ViewModel
{
	public class RailsAnimatorViewModel : BaseNotifyPropertyViewModel<RailsAnimator>
	{
		[CreateProperty]
		public List<RailsClipViewModel> Clips
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

		private List<RailsClipViewModel> clips;
		private bool canAddClip;

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(RailsAnimator.Clips))
			{
				Clips = CreateViewModels(model.Clips);
			}
		}

		protected override void OnModelChanged()
		{
			CanAddClip = model != null;
			if (model == null)
			{
				Clips = new();
				return;
			}

			Clips = CreateViewModels(model.Clips);
		}

		public void AddClip()
		{
			model.AddClip();
		}

		public void RemoveClip(int index)
		{
			model.RemoveClip(model.Clips[index]);
		}

		private List<RailsClipViewModel> CreateViewModels(List<RailsClip> models)
		{
			if (models == null)
				return new();
			List<RailsClipViewModel> result = new();
			foreach (var model in models)
			{
				RailsClipViewModel viewModel = new();
				viewModel.BindModel(model);
				result.Add(viewModel);
			}
			return result;
		}
	}
}