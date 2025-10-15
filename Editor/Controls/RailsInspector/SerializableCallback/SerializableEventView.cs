using System.Collections.Generic;
using System.Linq;
using Rails.Editor.ViewModel;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class SerializableEventView : BaseView
	{
		[CreateProperty]
		public ObservableList<SerializableCallbackViewModel> Callbacks
		{
			get => animationEvent;
			set
			{
				if (animationEvent == value)
					return;
				if (animationEvent != null)
					animationEvent.ListChanged -= OnAnimationEventChanged;
				animationEvent = value;
				animationEvent.ListChanged += OnAnimationEventChanged;
				list.itemsSource = value;
			}
		}
		[CreateProperty]
		public ICommand<ReorderArgs> ReorderItemCommand { get; set; }
		[CreateProperty]
		public ICommand<IEnumerable<int>> AddItemsCommand { get; set; }
		[CreateProperty]
		public ICommand<IEnumerable<int>> RemoveItemsCommand { get; set; }

		private ObservableList<SerializableCallbackViewModel> animationEvent;
		private ListView list;


		public SerializableEventView()
		{
			list = new ListView();
			list.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
			list.showBorder = true;
			list.reorderable = true;
			list.reorderMode = ListViewReorderMode.Animated;
			list.selectionType = SelectionType.Multiple;
			list.showFoldoutHeader = true;
			list.headerTitle = "Event";
			list.showAddRemoveFooter = true;
			list.showBoundCollectionSize = false;
			list.bindingSourceSelectionMode = BindingSourceSelectionMode.Manual;
			list.makeItem = () =>
			{
				var control = new SerializableCallbackControl();
				control.SetBinding(SerializableCallbackControl.TargetObjectProperty, new TwoWayBinding(nameof(SerializableCallbackViewModel.TargetObject)));
				control.SetBinding(SerializableCallbackControl.StateProperty, new TwoWayBinding(nameof(SerializableCallbackViewModel.State)));
				control.SetBinding(SerializableCallbackControl.MethodOptionsProperty, new ToTargetBinding(nameof(SerializableCallbackViewModel.MethodOptions)));
				control.SetBinding(SerializableCallbackControl.SelectedMethodProperty, new ToTargetBinding(nameof(SerializableCallbackViewModel.SelectedMethod)));
				control.SetBinding(SerializableCallbackControl.ParamsProperty, new ToTargetBinding(nameof(SerializableCallbackViewModel.Params)));
				control.SetBinding(SerializableCallbackControl.SelectMethodCommandProperty, new CommandBinding(nameof(SerializableCallbackViewModel.SelectMethodCommand)));
				return control;
			};
			list.bindItem = (element, index) =>
			{
				element.dataSource = Callbacks[index];
			};
			list.onAdd = x => OnAdd(x.selectedIndices);
			list.onRemove = x => OnRemove(x.selectedIndices);

			hierarchy.Add(list);

			SetBinding(nameof(Callbacks), new ToTargetBinding(nameof(SerializableEventViewModel.Callbacks)));
			list.SetBinding(nameof(ListView.selectedIndex), new DataBinding()
			{
				dataSourcePath = new PropertyPath(nameof(SerializableEventViewModel.SelectedIndex)),
				bindingMode = BindingMode.ToSource,
			});
			SetBinding(nameof(ReorderItemCommand), new CommandBinding(nameof(SerializableEventViewModel.ReorderItemCommand)));
			SetBinding(nameof(AddItemsCommand), new CommandBinding(nameof(SerializableEventViewModel.AddItemsCommand)));
			SetBinding(nameof(RemoveItemsCommand), new CommandBinding(nameof(SerializableEventViewModel.RemoveItemsCommand)));
		}

		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			if (animationEvent != null)
				animationEvent.ListChanged += OnAnimationEventChanged;
			list.itemIndexChanged += OnReorder;
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			if (animationEvent != null)
				animationEvent.ListChanged -= OnAnimationEventChanged;
			list.itemIndexChanged -= OnReorder;
		}

		private void OnAnimationEventChanged()
		{
			list.Rebuild();
		}

		private void OnReorder(int oldIndex, int newIndex)
		{
			ReorderItemCommand?.Execute(new ReorderArgs(oldIndex, newIndex));
		}

		private void OnAdd(IEnumerable<int> addIndexes)
		{
			if (addIndexes.Any())
			{
				AddItemsCommand?.Execute(addIndexes);
				list.ClearSelection();
			}
			else if (Callbacks != null)
			{
				AddItemsCommand?.Execute(new int[] { Callbacks.Count });
			}
		}

		private void OnRemove(IEnumerable<int> removeIndexes)
		{
			if (removeIndexes.Any())
			{
				RemoveItemsCommand?.Execute(removeIndexes);
				list.ClearSelection();
			}
			else if (Callbacks?.Count > 0)
			{
				RemoveItemsCommand?.Execute(new int[] { Callbacks.Count - 1 });
			}
		}
	}
}