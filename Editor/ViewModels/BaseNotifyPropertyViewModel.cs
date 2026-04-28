using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Rails.Editor.Context;
using Unity.EditorCoroutines.Editor;
using UnityEngine.UIElements;

namespace Rails.Editor.ViewModel
{
	public abstract class BaseNotifyPropertyViewModel<TModel> : INotifyBindablePropertyChanged
	{
		public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;
		protected TModel model;


		public void BindModel(TModel model)
		{
			bool modelChanged = !this.model?.Equals(model) ?? true;
			this.model = model;

			EventBus.Subscribe<PropertyChanged>(OnModelPropertyChanged);
			OnBind();
			if (modelChanged)
				OnModelChanged();
		}

		public void UnbindModel()
		{
			if (model == null)
				return;
			EventBus.Unsubscribe<PropertyChanged>(OnModelPropertyChanged);
			model = default;
			OnUnbind();
		}

		protected void NotifyPropertyChanged([CallerMemberName] string property = "")
		{
			propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
		}

		protected virtual void OnModelPropertyChanged(PropertyChanged evt)
		{
			if (!EqualityComparer<object>.Default.Equals(model, evt.Sender))
				return;
			OnModelPropertyChanged(evt.Sender, evt.PropertyName);
		}

		protected abstract void OnModelPropertyChanged(object sender, string propertyName);
		protected abstract void OnModelChanged();

		protected virtual void OnBind()
		{
		}

		protected virtual void OnUnbind()
		{
		}

		protected void UpdateViewModels<VM, M>(ObservableList<VM> viewModels, IList<M> models, Func<int, VM> createViewModel, Action<VM> resetViewModel = null, Action<VM, M> viewModelBindCallback = null, Action<VM, M> viewModelPreBindCallback = null)
			where VM : BaseNotifyPropertyViewModel<M>
		{
			if (models == null)
			{
				ClearViewModels<VM, M>(viewModels, resetViewModel);
				viewModels.NotifyListChanged();
				return;
			}

			while (viewModels.Count < models.Count)
			{
				VM viewModel = createViewModel(viewModels.Count);
				viewModels.AddWithoutNotify(viewModel);
			}
			while (viewModels.Count > models.Count)
			{
				var viewModel = viewModels[^1];
				viewModel.UnbindModel();
				resetViewModel?.Invoke(viewModel);
				viewModels.RemoveWithoutNotify(viewModel);
			}

			for (int i = 0; i < models.Count; i++)
			{
				var model = models[i];
				var viewModel = viewModels[i];

				viewModel.UnbindModel();
				resetViewModel?.Invoke(viewModel);
				viewModelPreBindCallback?.Invoke(viewModel, model);
				viewModel.BindModel(model);
				viewModelBindCallback?.Invoke(viewModel, model);
			}

			viewModels.NotifyListChanged();
		}

		protected void ClearViewModels<VM, M>(ObservableList<VM> viewModels, Action<VM> resetViewModel = null)
			where VM : BaseNotifyPropertyViewModel<M>
		{
			foreach (var viewModel in viewModels)
			{
				viewModel.UnbindModel();
				resetViewModel?.Invoke(viewModel);
			}
			viewModels.ClearWithoutNotify();
		}

#nullable enable
		protected bool SetProperty<T>(T oldValue, T newValue, IEqualityComparer<T> comparer, Action<T> setter, [CallerMemberName] string? propertyName = "")
		{
			comparer ??= EqualityComparer<T>.Default;
			if (comparer.Equals(oldValue, newValue))
				return false;
			setter.Invoke(newValue);
			if (!propertyName.IsNullOrEmpty())
				NotifyPropertyChanged(propertyName);
			return true;
		}

		protected bool SetProperty<T>(T oldValue, T newValue, Action<T> setter, [CallerMemberName] string? propertyName = "")
		{
			return SetProperty<T>(oldValue, newValue, EqualityComparer<T>.Default, setter, propertyName);
		}

		protected bool SetProperty<T>(ref T field, T value, IEqualityComparer<T> comparer, [CallerMemberName] string? propertyName = "")
		{
			comparer ??= EqualityComparer<T>.Default;
			if (comparer.Equals(field, value))
				return false;
			field = value;
			if (!propertyName.IsNullOrEmpty())
				NotifyPropertyChanged(propertyName);
			return true;
		}

		protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = "")
		{
			return SetProperty(ref field, value, EqualityComparer<T>.Default, propertyName);
		}
#nullable disable
	}
}