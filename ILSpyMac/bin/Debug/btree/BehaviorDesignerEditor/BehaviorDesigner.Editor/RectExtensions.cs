using System;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	public static class RectExtensions
	{
		public static Vector2 TopLeft(this Rect rect)
		{
			return new Vector2(rect.xMin, rect.yMin);
		}

		public static Rect ScaleSizeBy(this Rect rect, float scale)
		{
			return rect.ScaleSizeBy(scale, rect.center);
		}

		public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
		{
			Rect result = rect;
			result.set_x(result.x - pivotPoint.x);
			result.set_y(result.y - pivotPoint.y);
			result.set_xMin(result.xMin * scale);
			result.set_xMax(result.xMax * scale);
			result.set_yMin(result.yMin * scale);
			result.set_yMax(result.yMax * scale);
			result.set_x(result.x + pivotPoint.x);
			result.set_y(result.y + pivotPoint.y);
			return result;
		}
	}
}
