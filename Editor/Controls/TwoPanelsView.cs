using System;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class TwoPanelsView : BaseView
	{
		private static readonly BindingId fixedPaneInitialDimensionProperty = new (nameof(FixedPaneInitialDimension));
		[CreateProperty, UxmlAttribute("fixed-panel-initial-dimension")]
		public float FixedPaneInitialDimension
		{
			get => split.fixedPaneInitialDimension;
			set
			{
				if (split.fixedPaneInitialDimension != value)
				{
					split.fixedPaneInitialDimension = value;
					NotifyPropertyChanged(fixedPaneInitialDimensionProperty);
				}
			}
		}
		public VisualElement FirstPanel => firstPanel;
		public VisualElement SecondPanel => secondPanel;
		public event Action<float> FixedPanelDimensionChanged;

		private VisualElement firstPanel;
		private VisualElement secondPanel;
		private TwoPaneSplitView split; 


		public TwoPanelsView()
		{
			firstPanel = new VisualElement();
			firstPanel.name = "first-panel";
			firstPanel.style.flexGrow = 1;
			secondPanel = new VisualElement();
			secondPanel.name = "second-panel";
			secondPanel.style.flexGrow = 1;

			split = new TwoPaneSplitView();

			hierarchy.Add(split);
			split.Add(firstPanel);
			split.Add(secondPanel);
		}

		protected override void OnAttach(AttachToPanelEvent evt)
		{
			base.OnAttach(evt);
			FirstPanel.RegisterCallback<GeometryChangedEvent>(OnPanelDimensionChange);
		}

		protected override void OnDetach(DetachFromPanelEvent evt)
		{
			base.OnDetach(evt);
			FirstPanel.UnregisterCallback<GeometryChangedEvent>(OnPanelDimensionChange);
		}

		private void OnPanelDimensionChange(GeometryChangedEvent evt)
		{
			FixedPanelDimensionChanged?.Invoke(firstPanel.layout.width);
		}
	}
}