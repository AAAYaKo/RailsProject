using Rails.Runtime;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class SplineView : VisualElement
	{
		private static readonly CustomStyleProperty<Color> startColorUss = new("--start-color");
		private static readonly CustomStyleProperty<Color> endColorUss = new("--end-color");

		private Gradient splineGradient;
		[UxmlAttribute("spline"), CreateProperty]
		public Vector2[] Spline
		{
			get => spline;
			set
			{
				if (!Utils.SplineEquals(spline, value))
				{
					spline = value;
					MarkDirtyRepaint();
				}
			}
		}

		private Vector2[] spline;


		public SplineView()
		{
			splineGradient = new Gradient();
			splineGradient.colorKeys = new GradientColorKey[]
			{
				new GradientColorKey(Color.white, 0),
				new GradientColorKey(new Color(0.43f, 0.43f, 0.43f), 0.5f),
				new GradientColorKey(Color.white, 1),
			};
			generateVisualContent += DrawSpline;
			RegisterCallback<CustomStyleResolvedEvent>(CustomStylesResolved);
		}

		private void CustomStylesResolved(CustomStyleResolvedEvent evt)
		{
			bool repaint = false;

			if (customStyle.TryGetValue(startColorUss, out var startColor))
				repaint = true;
			if (customStyle.TryGetValue(endColorUss, out var endColor))
				repaint = true;

			if (repaint)
			{
				splineGradient = new Gradient()
				{
					colorKeys = new GradientColorKey[]
					{
						new GradientColorKey(startColor, 0),
						new GradientColorKey(new Color(0.43f, 0.43f, 0.43f), 0.5f),
						new GradientColorKey(endColor, 1),
					}
				};
				MarkDirtyRepaint();
			}
		}

		private void DrawSpline(MeshGenerationContext context)
		{
			if (spline == null)
				return;
			var painter2D = context.painter2D;
			painter2D.lineWidth = 2;
			painter2D.strokeGradient = splineGradient;

			painter2D.BeginPath();
			for (int i = 0; i < spline.Length / 3; ++i)
			{
				painter2D.MoveTo(spline[i * 3]);
				painter2D.BezierCurveTo(spline[i * 3 + 1], spline[i * 3 + 2], spline[i * 3 + 3]);
			}
			painter2D.Stroke();
		}
	}
}