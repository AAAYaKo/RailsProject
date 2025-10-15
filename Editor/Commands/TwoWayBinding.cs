using Unity.Properties;
using UnityEngine.UIElements;

namespace Rails.Editor
{
	public class TwoWayBinding : DataBinding
	{
		public TwoWayBinding(string dataSourcePath)
		{
			this.dataSourcePath = new PropertyPath(dataSourcePath);
			this.bindingMode = BindingMode.TwoWay;
		}
	}
}