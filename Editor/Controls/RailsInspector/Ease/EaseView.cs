using System;
using Rails.Runtime;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class EaseView : VisualElement
	{
		public static readonly BindingId FirstPointProperty = nameof(FirstPoint);
		public static readonly BindingId SecondPointProperty = nameof(SecondPoint);
		public static readonly BindingId SplineProperty = nameof(Spline);
		public static readonly BindingId HasHandlesProperty = nameof(HasHandles);

		private static readonly float DefaultBounds = 0.1f;
		private static readonly CollectionComparer<Vector2> comparer = new(VectorComparer.Instance);

		[CreateProperty]
		public Vector2 FirstPoint
		{
			get => firstPoint ?? Vector2.zero;
			set
			{
				if (firstPoint != null && Utils.Approximately(firstPoint.Value, value))
					return;

				value.x = Mathf.Clamp(value.x, 0, 1);
				firstPoint = value;

				firstHandle.Value = value;
				NotifyPropertyChanged(FirstPointProperty);
			}
		}
		[CreateProperty]
		public Vector2 SecondPoint
		{
			get => secondPoint ?? Vector2.zero;
			set
			{
				if (secondPoint != null && Utils.Approximately(secondPoint.Value, value))
					return;

				value.x = Mathf.Clamp(value.x, 0, 1);
				secondPoint = value;

				secondHandle.Value = value;
				NotifyPropertyChanged(SecondPointProperty);
			}
		}
		[CreateProperty]
		public Vector2[] Spline
		{
			get => spline;
			set
			{
				if (comparer.Equals(spline, value))
					return;
				spline = value;
				FindMinMax(Spline, out float2 min, out float2 max);
				splineView.Spline = Spline;
				splineView.MinSplineBound = min;
				splineView.MaxSplineBound = max;

				bottomPadding = math.max(-min.y, DefaultBounds);
				topPadding = math.max((max.y - 1), DefaultBounds);
				float full = bottomPadding + topPadding + 1;
				bottomPadding /= full;
				topPadding /= full;

				UpdateScope();
				NotifyPropertyChanged(SplineProperty);
			}
		}
		[CreateProperty]
		public bool HasHandles
		{
			get => hasHandles ?? false;
			set
			{
				if (hasHandles == value)
					return;
				hasHandles = value;
				UpdateHandlesVisibility();
				NotifyPropertyChanged(SplineProperty);
			}
		}

		public event Action<Vector2> FirstPointChanged;
		public event Action<Vector2> SecondPointChanged;

		private CurveDragHandler firstHandle;
		private CurveDragHandler secondHandle;
		private SplineView splineView;
		private VisualElement container;
		private VisualElement axes;
		private Vector2[] spline;
		private Vector2? firstPoint;
		private Vector2? secondPoint;
		private float topPadding;
		private float bottomPadding;
		private bool? hasHandles = true;
		private Scope scope = new() { MinValue = Vector2.zero, MaxValue = Vector2.one };


		public EaseView()
		{
			container = new VisualElement();
			container.name = "ease-container";
			container.AddToClassList("ease-container");

			var border = new VisualElement();
			border.name = "border";
			border.pickingMode = PickingMode.Ignore;
			border.AddToClassList("ease-border");

			axes = new VisualElement();
			axes.name = "axes";
			axes.pickingMode = PickingMode.Ignore;
			axes.AddToClassList("ease-axes");

			secondHandle = new CurveDragHandler((x) =>
			{
				SecondPointChanged?.Invoke(x);
			});
			secondHandle.name = "ease-second-handle";
			secondHandle.style.position = Position.Absolute;
			secondHandle.OriginValue = new Vector2(1, 1);
			secondHandle.AddToClassList("ease-end");

			firstHandle = new CurveDragHandler((x) =>
			{
				FirstPointChanged?.Invoke(x);
			});
			firstHandle.name = "ease-first-handle";
			firstHandle.style.position = Position.Absolute;
			firstHandle.OriginValue = new Vector2(0, 0);
			firstHandle.AddToClassList("ease-start");

			splineView = new SplineView();
			splineView.name = "ease-spline-view";
			splineView.style.position = Position.Absolute;
			splineView.AddToClassList("ease-spline");
			splineView.pickingMode = PickingMode.Ignore;

			container.Add(axes);
			container.Add(splineView);
			container.Add(secondHandle);
			container.Add(firstHandle);
			hierarchy.Add(container);
			hierarchy.Add(border);

			AddToClassList("ease-view");
			RegisterCallback<GeometryChangedEvent>(OnGeometryChange);
		}

		private void OnGeometryChange(GeometryChangedEvent evt)
		{
			UpdateScope();
		}

		private void UpdateHandlesVisibility()
		{
			DisplayStyle style = HasHandles ? DisplayStyle.Flex : DisplayStyle.None;
			firstHandle.style.display = style;
			secondHandle.style.display = style;
		}

		private void FindMinMax(Vector2[] spline, out float2 min, out float2 max)
		{
			min = new float2(0, 0 - DefaultBounds);
			max = new float2(1, 1 + DefaultBounds);

			foreach (var point in spline)
			{
				min.y = math.min(point.y, min.y);
				max.y = math.max(point.y, max.y);
			}
		}

		private void UpdateScope()
		{
			scope.MinPosition = new Vector2(0, topPadding * layout.height);
			scope.MaxPosition = new Vector2(layout.width, (1 - bottomPadding) * layout.height);
			firstHandle.Scope = scope;
			secondHandle.Scope = scope;

			axes.style.bottom = new Length(layout.height - scope.MaxPosition.y, LengthUnit.Pixel);
			axes.style.top = new Length(scope.MinPosition.y, LengthUnit.Pixel);
		}
	}
}