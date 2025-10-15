using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Rails.Runtime.Callback;
using Unity.Properties;
using UnityEngine;

namespace Rails.Editor.ViewModel
{
	public class SerializableEventViewModel : BaseNotifyPropertyViewModel<SerializableEvent>
	{
		[CreateProperty]
		public ObservableList<SerializableCallbackViewModel> Callbacks => callbacks;
		[CreateProperty]
		public ICommand<ReorderArgs> ReorderItemCommand
		{
			get => reorderItemCommand;
			set => SetProperty(ref reorderItemCommand, value);
		}
		[CreateProperty]
		public ICommand<IEnumerable<int>> AddItemsCommand
		{
			get => addItemsCommand;
			set => SetProperty(ref addItemsCommand, value);
		}
		[CreateProperty]
		public ICommand<IEnumerable<int>> RemoveItemsCommand
		{
			get => removeItemsCommand;
			set => SetProperty(ref removeItemsCommand, value);
		}

		#region For Reorder
		[CreateProperty]
		public UnityEngine.Object TargetObject
		{
			get => Callbacks[SelectedIndex].TargetObject;
			set => Callbacks[SelectedIndex].TargetObject = value;
		}
		[CreateProperty]
		public string SelectedMethod
		{
			get => Callbacks[SelectedIndex].SelectedMethod;
			set => Callbacks[SelectedIndex].SelectedMethod = value;
		}
		[CreateProperty]
		public List<string> MethodOptions
		{
			get => Callbacks[SelectedIndex].MethodOptions;
			set => Callbacks[SelectedIndex].MethodOptions = value;
		}
		[CreateProperty]
		public SerializableCallbackState State
		{
			get => Callbacks[SelectedIndex].State;
			set => Callbacks[SelectedIndex].State = value;
		}
		[CreateProperty]
		public ObservableList<AnyValueViewModel> Params => Callbacks[SelectedIndex].Params;
		[CreateProperty]
		public ICommand<string> SelectMethodCommand
		{
			get => Callbacks[SelectedIndex].SelectMethodCommand;
			set => Callbacks[SelectedIndex].SelectMethodCommand = value;
		}

		[CreateProperty]
		public int SelectedIndex
		{
			get => selectedIndex;
			set => SetProperty(ref selectedIndex, value);
		}
		#endregion

		private ObservableList<SerializableCallbackViewModel> callbacks = new();
		private ICommand<IEnumerable<int>> removeItemsCommand;
		private ICommand<IEnumerable<int>> addItemsCommand;
		private ICommand<ReorderArgs> reorderItemCommand;
		private int selectedIndex;


		public SerializableEventViewModel()
		{
			ReorderItemCommand = new RelayCommand<ReorderArgs>(x =>
			{
				EditorContext.Instance.Record("Changed Event Callbacks Order");
				var callback = model.Callbacks[x.OldIndex];
				model.Callbacks.RemoveAt(x.OldIndex);
				model.Callbacks.Insert(x.NewIndex, callback);
				UpdateCallbacks();
			});
			RemoveItemsCommand = new RelayCommand<IEnumerable<int>>(x =>
			{
				EditorContext.Instance.Record("Removed Event Callbacks");
				var callbacks = x.Select(x => model.Callbacks[x]).ToArray();
				callbacks.ForEach(x => model.Callbacks.Remove(x));
				UpdateCallbacks();
			});
			AddItemsCommand = new RelayCommand<IEnumerable<int>>(x =>
			{
				EditorContext.Instance.Record("Added Event Callbacks");
				x.ForEach(x => model.Callbacks.Insert(x, new SerializableCallback()));
				UpdateCallbacks();
			});
		}

		protected override void OnBind()
		{
			base.OnBind();
		}

		protected override void OnUnbind()
		{
			base.OnUnbind();
			ClearViewModels<SerializableCallbackViewModel, SerializableCallback>(Callbacks);
		}

		protected override void OnModelChanged()
		{
			if (model == null)
			{
				ClearViewModels<SerializableCallbackViewModel, SerializableCallback>(Callbacks);
				return;
			}

			UpdateCallbacks();
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(AnimationEvent))
			{
				UpdateCallbacks();
			}
		}

		private void UpdateCallbacks()
		{
			UpdateViewModels(Callbacks, model.Callbacks,
				createViewModel: x => new SerializableCallbackViewModel());
		}
	}
}