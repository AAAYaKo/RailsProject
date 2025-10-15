using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class EventKeyInspector : BaseView
	{
		private static VisualTreeAsset template;


		static EventKeyInspector()
		{
			template = Resources.Load<VisualTreeAsset>("EventKeyEditor");
		}

		public EventKeyInspector()
		{
			template.CloneTree(this);
		}
	}
}