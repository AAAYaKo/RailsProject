using System.ComponentModel;
using Rails.Runtime;
using Unity.Properties;

namespace Rails.Editor.ViewModel
{
	public class RailsClipViewModel : BaseNotifyPropertyViewModel<RailsClip>
	{
		[CreateProperty]
		public string Name
		{
			get => name;
			set
			{
				if (Name != value)
				{
					name = value;
					NotifyPropertyChanged();
				}
			}
		}

		private string name;


		protected override void OnModelChanged()
		{
			if (model == null)
				return;
			Name = model.Name;
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(RailsClip.Name))
			{
				Name = model.Name;
			}
		}
	}
}