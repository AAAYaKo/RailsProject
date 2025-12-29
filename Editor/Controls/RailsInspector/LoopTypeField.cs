using DG.Tweening;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class LoopTypeField : PopupField<LoopType>
	{
		public LoopTypeField()
		{
			index = 0;
		}
	}
}