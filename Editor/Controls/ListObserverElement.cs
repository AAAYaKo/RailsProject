using System.Collections.Generic;
using Rails.Editor.ViewModel;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	public abstract class ListObserverElement<TElementModel, TElementView> : BaseView where TElementView : VisualElement
	{
		public static readonly BindingId ValuesProperty = nameof(Values);

		[CreateProperty]
		public ObservableList<TElementModel> Values
		{
			get => values;
			set
			{
				if (values == value)
					return;

				if (values != null)
					values.ListChanged -= UpdateList;

				values = value;
				values.ListChanged += UpdateList;
				UpdateList();
			}
		}

		public List<TElementView> Views => views;

		protected ObservableList<TElementModel> values = new();
		protected List<TElementView> views = new();
		protected VisualElement container;


		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			if (values != null)
				values.ListChanged += UpdateList;
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			if (values != null)
				values.ListChanged -= UpdateList;
		}

		protected abstract TElementView CreateElement();
		protected virtual void ResetElement(TElementView element) { }

		protected virtual void UpdateList()
		{
			if (Values.IsNullOrEmpty())
			{
				views.ForEach(x =>
				{
					ResetElement(x);
					container.Remove(x);
				});
				views.Clear();
				return;
			}
			while (Values.Count > views.Count)
			{
				var view = CreateElement();
				container.Add(view);
				views.Add(view);
			}
			while (Values.Count < views.Count)
			{
				var view = views[^1];
				ResetElement(view);
				container.Remove(view);
				views.Remove(view);
			}
			for (int i = 0; i < views.Count; i++)
			{
				views[i].dataSource = Values[i];
			}
		}
	}
}