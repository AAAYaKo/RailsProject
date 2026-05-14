using System;
using Unity.Mathematics;
using UnityEngine;

namespace Rails.Runtime.Drivers
{
	[Serializable]
	public class CanvasRelativePosition : BaseRailsDriver<Vector2>
	{
		private static readonly Vector3[] corners = new Vector3[4];

		[SerializeField] private Edge edge;


		public override Vector2 ComputeValue(UnityEngine.Object reference)
		{
			if (reference == null)
				return Vector2.zero;
			if (reference is RectTransform rect)
			{
				return CalulateEndValueByEdge(rect);
			}

			return Vector2.zero;
		}

		private Vector2 CalulateEndValueByEdge(RectTransform animatedComponent)
		{
			var rootCanvasRect = (RectTransform)animatedComponent.GetComponentInParent<Canvas>().rootCanvas.transform;
			Vector2 value = animatedComponent.anchoredPosition;
			var size = animatedComponent.rect.size / 2;
			size.x *= animatedComponent.lossyScale.x;
			size.y *= animatedComponent.lossyScale.y;
			rootCanvasRect.GetWorldCorners(corners);
			switch (edge)
			{
				case Edge.behindLeft:
					value.x += corners[0].x - size.x;
					break;
				case Edge.behindTop:
					value.y += corners[1].y + size.y;
					break;
				case Edge.behindRight:
					value.x += corners[2].x + size.x;
					break;
				case Edge.behindBottom:
					value.y += corners[3].y - size.y;
					break;
				case Edge.centerOfCanvas:
					value = rootCanvasRect.TransformPoint(rootCanvasRect.rect.center);
					break;
				case Edge.centerOfVerticalLine:
				case Edge.centerOfHorizontalLine:
					value = CalculateCenterOfLine(rootCanvasRect, animatedComponent, edge is Edge.centerOfVerticalLine);
					break;
			}

			value = SetRawPositionByEdge(animatedComponent, edge, value);
			value = TransformToAnchorPosition(animatedComponent, value);
			return value;
		}

		private Vector2 SetRawPositionByEdge(RectTransform animatedComponent, Edge edge, Vector2 value)
		{
			if (edge == Edge.behindLeft || edge == Edge.behindRight)
				value.y = animatedComponent.position.y;
			else if (edge == Edge.behindTop || edge == Edge.behindBottom)
				value.x = animatedComponent.position.x;
			return value;
		}

		private Vector2 TransformToAnchorPosition(RectTransform animatedComponent, Vector3 worldPos)
		{
			var parent = animatedComponent.parent as RectTransform;

			Vector2 local = parent.InverseTransformPoint(worldPos);
			var rect = parent.rect;
			Vector2 anchorAreaPos = math.lerp(rect.min, rect.max, animatedComponent.anchorMin);
			Vector2 anchorAreaSize = (animatedComponent.anchorMax - animatedComponent.anchorMin) * rect.size;

			return local - anchorAreaPos - Vector2.Scale(anchorAreaSize, animatedComponent.pivot);
		}

		private Vector2 CalculateCenterOfLine(RectTransform rootCanvasRect, RectTransform animatedComponent, bool isVertical)
		{
			Vector2 value = rootCanvasRect.TransformPoint(rootCanvasRect.rect.center);
			Vector2 anchoredPosition = rootCanvasRect.TransformPoint(animatedComponent.anchoredPosition);
			if (isVertical)
				value.x = anchoredPosition.x;
			else
				value.y = anchoredPosition.y;
			return value;
		}

		private enum Edge
		{
			behindLeft, behindTop, behindRight, behindBottom, centerOfCanvas, centerOfVerticalLine, centerOfHorizontalLine
		}
	}
}
