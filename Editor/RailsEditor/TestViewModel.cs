using System;
using System.Runtime.CompilerServices;
using Rails.Editor.ViewModel;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Rails.Editor
{
	public class TestViewModel : INotifyBindablePropertyChanged
	{
		[CreateProperty]
		public EaseViewModel Ease
		{
			get => ease;
			set
			{
				if (ease != value)
				{
					if (ease != null)
						ease.propertyChanged -= OnEasePropertyChanged;
					ease = value;
					ease.propertyChanged += OnEasePropertyChanged;
					NotifyPropertyChanged();
				}
			}
		}

		public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

		private EaseViewModel ease;


		private void NotifyPropertyChanged([CallerMemberName] string property = "")
		{
			propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
		}

		private void OnEasePropertyChanged(object sender, BindablePropertyChangedEventArgs e)
		{
			NotifyPropertyChanged(nameof(Ease));
		}
	}
}