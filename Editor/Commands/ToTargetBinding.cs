using Unity.Properties;
using UnityEngine.UIElements;

namespace Rails.Editor
{
	public class ToTargetBinding : DataBinding
	{
		public ToTargetBinding(string dataSourcePath)
		{
			this.dataSourcePath = new PropertyPath(dataSourcePath);
			this.bindingMode = BindingMode.ToTarget;
		}
	}
}