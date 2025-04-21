using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class ThreePanelsView : VisualElement
	{
		private VisualElement firstPanel;
		private VisualElement secondPanel;
		private VisualElement thirdPanel;

		public VisualElement FirstPanel => firstPanel;
		public VisualElement SecondPanel => secondPanel;
		public VisualElement ThirdPanel => thirdPanel;


		public ThreePanelsView()
		{
			firstPanel = new VisualElement();
			firstPanel.name = "first-panel";
			firstPanel.style.flexGrow = 1;
			secondPanel = new VisualElement();
			secondPanel.name = "second-panel";
			secondPanel.style.flexGrow = 1;
			thirdPanel = new VisualElement();
			thirdPanel.name = "third-panel";
			thirdPanel.style.flexGrow = 1;
			TwoPaneSplitView firstSplit = new();
			TwoPaneSplitView secondSplit = new();

			hierarchy.Add(firstSplit);
			firstSplit.Add(secondSplit);
			firstSplit.Add(thirdPanel);
			firstSplit.fixedPaneInitialDimension = 500;
			secondSplit.Add(firstPanel);
			secondSplit.Add(secondPanel);
			secondSplit.fixedPaneInitialDimension = 200;
		}
	}
}