using System;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class TrackLine : VisualElement
	{



		public TrackLine()
		{
			AddToClassList("track-line");
		}
	}
}