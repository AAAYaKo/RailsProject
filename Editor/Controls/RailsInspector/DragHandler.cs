using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class DragHandler : VisualElement
	{
		private static readonly CustomStyleProperty<Color> _handlerColorUss = new("--handler-color");

		private Color _handlerColor = Color.white;


		public DragHandler()
		{
			generateVisualContent += OnGenerateVisualContent;
			AddToClassList("ease-handle");
			RegisterCallback<CustomStyleResolvedEvent>(CustomStylesResolved);
		}

		private void CustomStylesResolved(CustomStyleResolvedEvent evt)
		{
			bool repaint = false;

			if (customStyle.TryGetValue(_handlerColorUss, out _handlerColor))
				repaint = true;

			if (repaint)
				MarkDirtyRepaint();
		}

		private void OnGenerateVisualContent(MeshGenerationContext context)
		{
			var painter2D = context.painter2D;

			painter2D.fillColor = _handlerColor;
			Vector2 center = new(layout.width / 2, layout.height / 2);
			painter2D.BeginPath();
			painter2D.MoveTo(center);
			painter2D.Arc(center, layout.width / 4, 0, 360, ArcDirection.Clockwise);
			painter2D.Fill();
			painter2D.lineWidth = 2;
			painter2D.strokeColor = _handlerColor;
			painter2D.MoveTo(center);
			painter2D.LineTo(center - (Vector2)transform.position);
			painter2D.Stroke();
		}
	}

	public enum HandleSide { Left, Right, }
}