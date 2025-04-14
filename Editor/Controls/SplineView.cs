using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class SplineView : VisualElement
	{
		private static readonly CustomStyleProperty<Color> _startColorUss = new("--start-color");
		private static readonly CustomStyleProperty<Color> _endColorUss = new("--end-color");

		private Gradient _splineGradient;
		[UxmlAttribute("spline"), CreateProperty]
		public Vector2[] Spline
		{
			get => _spline;
			set
			{
				if (!Utils.SpliteEquals(_spline, value))
				{
					_spline = value;
					MarkDirtyRepaint();
				}
			}
		}

		private Vector2[] _spline;


		public SplineView()
		{
			_splineGradient = new Gradient();
			_splineGradient.colorKeys = new GradientColorKey[]
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

			if (customStyle.TryGetValue(_startColorUss, out var startColor))
				repaint = true;
			if (customStyle.TryGetValue(_endColorUss, out var endColor))
				repaint = true;

			if (repaint)
			{
				_splineGradient = new Gradient()
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
			if (_spline == null)
				return;
			var painter2D = context.painter2D;
			painter2D.lineWidth = 2;
			painter2D.strokeGradient = _splineGradient;

			painter2D.BeginPath();
			for (int i = 0; i < _spline.Length / 3; ++i)
			{
				painter2D.MoveTo(_spline[i * 3]);
				painter2D.BezierCurveTo(_spline[i * 3 + 1], _spline[i * 3 + 2], _spline[i * 3 + 3]);
			}
			painter2D.Stroke();
		}
	}
}