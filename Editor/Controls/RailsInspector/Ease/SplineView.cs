using Rails.Runtime;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class SplineView : VisualElement
	{
		private static readonly CustomStyleProperty<Color> startColorUss = new("--start-color");
		private static readonly CustomStyleProperty<Color> centerColorUss = new("--center-color");
		private static readonly CustomStyleProperty<Color> endColorUss = new("--end-color");
		private static readonly CustomStyleProperty<float> lineWidthUss = new("--line-width");
		private static readonly CollectionComparer<Vector2> comparer = new(VectorComparer.Instance);

		[UxmlAttribute("spline"), CreateProperty]
		public Vector2[] Spline
		{
			get => spline;
			set
			{
				if (comparer.Equals(spline, value))
					return;
				spline = value;
				MarkDirtyRepaint();
			}
		}
		[UxmlAttribute("min-value"), CreateProperty]
		public Vector2 MinSplineBound
		{
			get => minSplineBound;
			set
			{
				if (Utils.Approximately(minSplineBound, value))
					return;
				minSplineBound = value;
				MarkDirtyRepaint();
			}
		}
		[UxmlAttribute("max-value"), CreateProperty]
		public Vector2 MaxSplineBound
		{
			get => maxSplineBound;
			set
			{
				if (Utils.Approximately(maxSplineBound, value))
					return;
				maxSplineBound = value;
				MarkDirtyRepaint();
			}
		}


		private Vector2[] spline;
		private FillGradient splineGradient;
		private Vector2 minSplineBound;
		private Vector2 maxSplineBound;
		private float lineWidth = 2;

		public SplineView()
		{
			splineGradient = new FillGradient();
			splineGradient.gradientType = GradientType.Linear;
			splineGradient.addressMode = AddressMode.Clamp;
			splineGradient.gradient = new Gradient
			{
				colorKeys = new GradientColorKey[]
				{
					new (Color.white, 0),
					new(new Color(0.43f, 0.43f, 0.43f), 0.25f),
					new(new Color(0.43f, 0.43f, 0.43f), 0.75f),
					new (Color.white, 1),
				}
			};
			generateVisualContent = DrawSpline;
			RegisterCallback<CustomStyleResolvedEvent>(CustomStylesResolved);
		}

		private void CustomStylesResolved(CustomStyleResolvedEvent evt)
		{
			bool colorChanged = false;
			bool lineWidthChanged = false;

			if (customStyle.TryGetValue(startColorUss, out var startColor))
				colorChanged = true;
			if (customStyle.TryGetValue(endColorUss, out var endColor))
				colorChanged = true;
			if (customStyle.TryGetValue(centerColorUss, out var centerColor))
				colorChanged = true;
			if (customStyle.TryGetValue(lineWidthUss, out float width))
				lineWidthChanged = true;

			if (colorChanged)
			{
				splineGradient.gradient = new Gradient()
				{
					colorKeys = new GradientColorKey[]
					{
						new (startColor, 0),
						new (centerColor, 0.25f),
						new (centerColor, 0.75f),
						new (endColor, 1),
					}
				};
			}
			if (lineWidthChanged)
				lineWidth = width;
			if (colorChanged || lineWidthChanged)
				MarkDirtyRepaint();
		}

		private void DrawSpline(MeshGenerationContext context)
		{
			if (spline == null || spline.Length < 4)
				return;

			splineGradient.start = new Vector2(0, ConvertToViewPoint(new float2(0, 0)).y);
			splineGradient.end = new Vector2(0, ConvertToViewPoint(new float2(0, 1)).y);
			var painter2D = context.painter2D;
			painter2D.lineWidth = lineWidth;
			painter2D.strokeFillGradient = splineGradient;

			painter2D.BeginPath();
			for (int i = 0; i < spline.Length - 1; i += 3)
			{
				float2 start = ConvertToViewPoint(spline[i]);
				float2 end = ConvertToViewPoint(spline[i + 3]);
				painter2D.MoveTo(start);
				painter2D.BezierCurveTo(ConvertToViewPoint(spline[i + 1]), ConvertToViewPoint(spline[i + 2]), end);
			}
			painter2D.Stroke();
		}

		private float2 ConvertToViewPoint(float2 point)
		{
			float2 rectMax = new(layout.xMin, layout.yMax);
			float2 rectMin = new(layout.xMax - lineWidth / 2, layout.yMin);
			return math.remap(MinSplineBound, MaxSplineBound, rectMax, rectMin, point);
		}
	}
}