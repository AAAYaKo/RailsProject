using Rails.Editor.ViewModel;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class MethodsField : PopupField<MethodOption>
	{
		public MethodsField()
		{
			formatListItemCallback = x => x?.OptionName;
			formatSelectedValueCallback = x => x?.SelectedName;
			index = 0;
		}
	}
}