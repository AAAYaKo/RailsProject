using Unity.Properties;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class TwoPanelsView : VisualElement
	{
		private static readonly BindingId fixedPaneInitialDimensionProperty = new (nameof(FixedPaneInitialDimension));
		[CreateProperty, UxmlAttribute("fixed-pane-initial-dimension")]
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

			split = new();

			hierarchy.Add(split);
			split.Add(firstPanel);
			split.Add(secondPanel);
		}
	}
}