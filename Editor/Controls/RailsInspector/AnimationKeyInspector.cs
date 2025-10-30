using Rails.Editor.ViewModel;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class AnimationKeyInspector : BaseView
	{
		private static readonly VisualTreeAsset template;

		static AnimationKeyInspector()
		{
			template = Resources.Load<VisualTreeAsset>("AnimationKeyEditor");
		}

		public AnimationKeyInspector()
		{
			template.CloneTree(this);
			AnimationValueControl valueControl = this.Q<AnimationValueControl>();
			valueControl.SetBinding(AnimationValueControl.ValueEditCommandProperty, new CommandBinding(nameof(AnimationKeyViewModel.ValueEditCommand)));
			valueControl.SetBinding(AnimationValueControl.ConstrainedProportionsChangeCommandProperty, new CommandBinding(nameof(AnimationKeyViewModel.ConstrainedProportionsChangeCommand)));
		}
	}
}