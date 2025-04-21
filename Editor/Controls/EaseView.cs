using Rails.Editor.Manipulator;
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
		public static readonly BindingId SplinePointProperty = nameof(Spline);
		public static readonly BindingId HasHandlesProperty = nameof(HasHandles);

		[UxmlAttribute("first-handle"), CreateProperty]
		public Vector2 FirstPoint
		{
			get => _firstPoint;
			set
			{
				if (!Utils.Approximately(_firstPoint, value))
				{
					value.x = Mathf.Clamp(value.x, 0, 1);
					_firstPoint = value;

					_topLine = layout.height * 0.1f;
					_bottomLine = layout.height * 0.9f;
					MoveHandle(_firstHandle, new Vector2(0, 0), FirstPoint);

					_firstHandle.MarkDirtyRepaint();
					NotifyPropertyChanged(FirstPointProperty);
				}
			}
		}
		[UxmlAttribute("second-handle"), CreateProperty]
		public Vector2 SecondPoint
		{
			get => _secondPoint;
			set
			{
				if (!Utils.Approximately(_secondPoint, value))
				{
					value.x = Mathf.Clamp(value.x, 0, 1);
					_secondPoint = value;

					_topLine = layout.height * 0.1f;
					_bottomLine = layout.height * 0.9f;
					MoveHandle(_secondHandle, new Vector2(1, 1), SecondPoint);

					_secondHandle.MarkDirtyRepaint();
					NotifyPropertyChanged(SecondPointProperty);
				}
			}
		}

		[UxmlAttribute("spline"), CreateProperty]
		public Vector2[] Spline
		{
			get
			{
				if (_spline == null)
					return new Vector2[0];
				Vector2[] result = new Vector2[_spline.Length];
				Offsets padding = new(0, 0, 2, -2);
				for (int i = 0; i < _spline.Length; i++)
					result[i] = CalculateTargetPosition(_spline[i], padding);
				return result;
			}
			set
			{
				if (!Utils.SplineEquals(_spline, value))
				{
					_spline = value;

					MarkDirtyRepaint();
					NotifyPropertyChanged(SplinePointProperty);
				}
			}
		}

		[UxmlAttribute("has-handles"), CreateProperty]
		public bool HasHandles
		{
			get => _hasHandles ?? false;
			set
			{
				if (_hasHandles != value)
				{
					_hasHandles = value;
					UpdateHandlesVisibility();
					NotifyPropertyChanged(SplinePointProperty);
				}
			}
		}

		private DragHandler _firstHandle;
		private DragHandler _secondHandle;
		private SplineView _splineView;
		private VisualElement _container;
		private VisualElement _axes;
		private Vector2[] _spline;
		private Vector2 _firstPoint;
		private Vector2 _secondPoint;
		private float _topLine;
		private float _bottomLine;
		private bool? _hasHandles = true;


		public EaseView()
		{
			_container = new VisualElement();
			_container.name = "ease-container";
			_container.AddToClassList("ease-container");

			var border = new VisualElement();
			border.name = "border";
			border.pickingMode = PickingMode.Ignore;
			border.AddToClassList("ease-border");

			_axes = new VisualElement();
			_axes.name = "axes";
			_axes.pickingMode = PickingMode.Ignore;
			_axes.AddToClassList("ease-axes");

			_secondHandle = new DragHandler();
			_secondHandle.name = "ease-second-handle";
			_secondHandle.style.position = Position.Absolute;
			_secondHandle.AddToClassList("ease-end");
			_secondHandle.AddManipulator(new EaseDragManipulator(x =>
			{
				var padding = GetHandlePadding(_secondHandle);
				SecondPoint = GetNormalizedValue(padding, x);
			}));

			_firstHandle = new DragHandler();
			_firstHandle.name = "ease-first-handle";
			_firstHandle.style.position = Position.Absolute;
			_firstHandle.AddToClassList("ease-start");
			_firstHandle.AddManipulator(new EaseDragManipulator(x =>
			{
				var padding = GetHandlePadding(_firstHandle);
				FirstPoint = GetNormalizedValue(padding, x);
			}));

			_splineView = new SplineView();
			_splineView.name = "ease-spline-view";
			_splineView.style.position = Position.Absolute;
			_splineView.AddToClassList("ease-spline");
			_splineView.pickingMode = PickingMode.Ignore;
			_splineView.SetBinding("Spline", new DataBinding
			{
				dataSourcePath = new PropertyPath(nameof(Spline)),
				bindingMode = BindingMode.ToTarget,
				updateTrigger = BindingUpdateTrigger.OnSourceChanged,
			});
			_splineView.dataSource = this;

			_container.Add(_axes);
			_container.Add(_splineView);
			_container.Add(_secondHandle);
			_container.Add(_firstHandle);
			hierarchy.Add(_container);
			hierarchy.Add(border);

			AddToClassList("ease-view");
			RegisterCallback<GeometryChangedEvent>(OnGeometryChange);
		}

		private Offsets GetZone(Offsets padding)
		{
			return new Offsets()
			{
				Top = _topLine + padding.Top,
				Bottom = _bottomLine + padding.Bottom,
				Left = padding.Left,
				Right = layout.width + padding.Right,
			};
		}

		private void OnGeometryChange(GeometryChangedEvent evt)
		{
			_topLine = layout.height * 0.1f;
			_bottomLine = layout.height * 0.9f;

			MoveHandle(_firstHandle, new Vector2(0, 0), FirstPoint);
			MoveHandle(_secondHandle, new Vector2(1, 1), SecondPoint);
			NotifyPropertyChanged(SplinePointProperty);
		}

		private Offsets GetHandlePadding(VisualElement element)
		{
			return new Offsets()
			{
				Left = -element.layout.width * 0.4f,
				Right = -element.layout.width * 0.6f,
				Top = -element.layout.height * 0.5f,
				Bottom = -element.layout.height * 0.5f,
			};
		}

		private void UpdateHandlesVisibility()
		{
			DisplayStyle style = HasHandles ? DisplayStyle.Flex : DisplayStyle.None;
			_firstHandle.style.display = style;
			_secondHandle.style.display = style;
		}

		private void MoveHandle(VisualElement element, Vector2 pointStart, Vector2 point)
		{
			Offsets padding = GetHandlePadding(element);

			Vector2 startPosition = CalculateTargetPosition(pointStart, padding);
			element.style.left = startPosition.x;
			element.style.top = startPosition.y;

			Vector2 position = CalculateTargetPosition(point, padding);
			Vector2 translate = position - startPosition;
			element.transform.position = translate;
			element.MarkDirtyRepaint();
		}

		private Vector2 CalculateTargetPosition(Vector2 point, Offsets padding)
		{
			var zone = GetZone(padding);
			float translateY = Mathf.LerpUnclamped(zone.Bottom, zone.Top, point.y);
			float translateX = Mathf.Lerp(zone.Left, zone.Right, point.x);
			return new Vector2(translateX, translateY);
		}

		private Vector2 GetNormalizedValue(Offsets padding, Vector2 notNormalized)
		{
			var zone = GetZone(padding);
			float pointX = Resize(zone.Left, zone.Right, notNormalized.x);
			float pointY = Resize(zone.Bottom, zone.Top, notNormalized.y);
			return new Vector2(pointX, pointY);
		}

		private static float Resize(float fromOriginal, float toOriginal, float value)
		{
			float result = value - fromOriginal;
			result /= (toOriginal - fromOriginal);
			return result;
		}

		private struct Offsets
		{
			public float Top { get; set; }
			public float Bottom { get; set; }
			public float Left { get; set; }
			public float Right { get; set; }


			public Offsets(float top, float bottom, float left, float right)
			{
				Top = top;
				Bottom = bottom;
				Left = left;
				Right = right;
			}
		}
	}
}