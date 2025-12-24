using System;
using Rails.Editor.Manipulator;
using Rails.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	public class CurveDragHandler : VisualElement
	{
		private static readonly CustomStyleProperty<Color> handlerColorUss = new("--handler-color");
		public Vector2 OriginValue
		{
			get => originValue;
			set
			{
				if (Utils.Approximately(originValue, value))
					return;
				originValue = value;
				UpdateOriginPosition();
				UpdateTranslation();
			}
		}
		public Vector2 Value
		{
			get => value;
			set
			{
				if (Utils.Approximately(this.value, value))
					return; 
				this.value = value;
				UpdateTranslation();
			}
		}
		public Scope Scope
		{
			get => scope;
			set
			{
				if (Utils.Approximately(scope.MinValue, value.MinValue) && Utils.Approximately(scope.MaxValue, value.MaxValue)
					&& Utils.Approximately(scope.MinPosition, value.MinPosition) && Utils.Approximately(scope.MaxPosition, value.MaxPosition))
					return;
				scope = value;
				UpdateOriginPosition();
				UpdateTranslation();
			}
		}
		public Offsets Paddings => new(0, 0, 0, 0);

		private Color handlerColor = Color.white;
		private Vector2 originValue;
		private Vector2 value;
		private Vector2 originPosition;
		private Scope scope;


		public CurveDragHandler(Action<Vector2> setter)
		{
			generateVisualContent = OnGenerateVisualContent;
			AddToClassList("ease-handle");
			RegisterCallback<CustomStyleResolvedEvent>(CustomStylesResolved);
			this.AddManipulator(new EaseDragManipulator(x =>
			{
				setter.Invoke(Scope.CalculateValue(x, Paddings));
			}));
			RegisterCallback<GeometryChangedEvent>(OnGeometryChange);
		}

		private void OnGeometryChange(GeometryChangedEvent evt)
		{
			UpdateOriginPosition();
			UpdateTranslation();
		}

		private void CustomStylesResolved(CustomStyleResolvedEvent evt)
		{
			bool repaint = false;

			if (customStyle.TryGetValue(handlerColorUss, out handlerColor))
				repaint = true;

			if (repaint)
				MarkDirtyRepaint();
		}

		private void OnGenerateVisualContent(MeshGenerationContext context)
		{
			var painter2D = context.painter2D;

			painter2D.fillColor = handlerColor;
			Vector2 center = new(layout.width / 2, layout.height / 2);
			painter2D.BeginPath();
			painter2D.MoveTo(center);
			painter2D.Arc(center, layout.width / 4, 0, 360, ArcDirection.Clockwise);
			painter2D.Fill();
			painter2D.lineWidth = 2;
			painter2D.strokeColor = handlerColor;
			painter2D.MoveTo(center);
			painter2D.LineTo(center - (Vector2)resolvedStyle.translate);
			painter2D.Stroke();
		}

		private void UpdateOriginPosition()
		{
			originPosition = Scope.CalculatePlace(OriginValue, Paddings);
			originPosition -= new Vector2(layout.width / 2, layout.height / 2);
			style.left = originPosition.x;
			style.top = originPosition.y;
			MarkDirtyRepaint();
		}

		private void UpdateTranslation()
		{
			Vector2 position = Scope.CalculatePlace(Value, Paddings);
			position -= new Vector2(layout.width / 2, layout.height / 2);
			Vector2 translate = position - originPosition;
			style.translate = translate;
			MarkDirtyRepaint();
		}
	}
}