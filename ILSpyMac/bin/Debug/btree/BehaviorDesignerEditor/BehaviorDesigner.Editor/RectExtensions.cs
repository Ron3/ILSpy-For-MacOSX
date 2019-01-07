using System;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	public static class RectExtensions
	{
		public static Vector2 TopLeft(this Rect rect)
		{
			return new Vector2(rect.get_xMin(), rect.get_yMin());
		}

		public static Rect ScaleSizeBy(this Rect rect, float scale)
		{
			return rect.ScaleSizeBy(scale, rect.get_center());
		}

		public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
		{
			Rect result = rect;
			result.set_x(result.get_x() - pivotPoint.x);
			result.set_y(result.get_y() - pivotPoint.y);
			result.set_xMin(result.get_xMin() * scale);
			result.set_xMax(result.get_xMax() * scale);
			result.set_yMin(result.get_yMin() * scale);
			result.set_yMax(result.get_yMax() * scale);
			result.set_x(result.get_x() + pivotPoint.x);
			result.set_y(result.get_y() + pivotPoint.y);
			return result;
		}
	}
}
