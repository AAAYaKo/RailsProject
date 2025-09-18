using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace Rails.Editor.ViewModel
{
	public abstract class BaseNotifyPropertyViewModel<TModel> : INotifyBindablePropertyChanged
		where TModel : INotifyPropertyChanged
	{
		public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;
		protected TModel model;


		public void BindModel(TModel model)
		{
			bool modelChanged = !this.model?.Equals(model) ?? true;
			this.model = model;
			if (model != null)
				this.model.PropertyChanged += OnModelPropertyChanged;
			if (modelChanged)
				OnModelChanged();
		}

		public void UnbindModel()
		{
			if (model == null)
				return;
			model.PropertyChanged -= OnModelPropertyChanged;
			model = default;
		}

		protected void NotifyPropertyChanged([CallerMemberName] string property = "")
		{
			propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
		}

		protected abstract void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e);
		protected abstract void OnModelChanged();
	}
}