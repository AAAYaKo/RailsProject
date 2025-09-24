using Unity.Properties;
using UnityEngine.UIElements;

namespace Rails.Editor
{
	public class CommandBinding : DataBinding
	{
		public CommandBinding(string dataSourcePath)
		{
			this.dataSourcePath = new PropertyPath(dataSourcePath);
			this.bindingMode = BindingMode.ToTarget;
		}
	}
}