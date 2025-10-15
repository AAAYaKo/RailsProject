using Rails.Editor.ViewModel;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class RailsInspectorControl : BaseView
	{
		[CreateProperty]
		public ObservableList<IKeyViewModel> SelectedKeys
		{
			get => selectedKeys;
			set
			{
				if (selectedKeys == value)
					return;
				if (selectedKeys != null)
					selectedKeys.ListChanged -= OnSelectionChanged;
				selectedKeys = value;
				selectedKeys.ListChanged += OnSelectionChanged;
				list.itemsSource = value;
			}
		}
		private ListView list;
		private ObservableList<IKeyViewModel> selectedKeys;

		public RailsInspectorControl()
		{
			list = new();
			list.style.flexGrow = 1;
			list.style.width = new Length(100, LengthUnit.Percent);
			list.style.height = new Length(100, LengthUnit.Percent);
			list.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
			list.selectionType = SelectionType.None;
			list.allowAdd = false;
			list.allowRemove = false;
			list.reorderable = false;
			list.bindingSourceSelectionMode = BindingSourceSelectionMode.Manual;

			list.makeItem = () =>
			{
				return new KeyInspector();
			};
			list.makeNoneElement = () =>
			{
				return new VisualElement();
			};

			list.bindItem = (element, index) =>
			{
				element.dataSource = SelectedKeys[index];
				(element as KeyInspector).ChangeKeyInspector();
			};

			Add(list);
			SetBinding(nameof(SelectedKeys), new DataBinding
			{
				dataSourcePath = new(nameof(RailsClipViewModel.SelectedKeys)),
				bindingMode = BindingMode.ToTarget
			});
		}

		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			if (selectedKeys != null)
				selectedKeys.ListChanged += OnSelectionChanged;
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			if (selectedKeys != null)
				selectedKeys.ListChanged -= OnSelectionChanged;
		}

		private void OnSelectionChanged()
		{
			list.RefreshItems();
		}
	}
}