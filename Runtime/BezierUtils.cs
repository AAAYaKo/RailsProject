//Based on
//https://github.com/qwe321qwe321qwe321/Unity-EasingAnimationCurve
// https://github.com/thednp/bezier-easing/blob/master/src/index.ts

using Unity.Burst;
using Unity.Mathematics;

namespace Rails.Runtime
{
	[BurstCompile]
	public static class BezierUtils
	{
		private const float epsilon = 1e-6f;

		[BurstCompile(FloatMode = FloatMode.Fast)]
		//Normalized Bezier, where p0 = (0,0), p1 (1,1)
		public static float GetBezierYbyX(in float x, in float3x2 polynomial)
		{
			SampleValueAtT(SolveX(x, polynomial[0]), polynomial[1], out float result);
			return result;
		}

		[BurstCompile(FloatMode = FloatMode.Fast)]
		public static float SolveX(in float x, in float3 polynomial)
		{
			if (x <= 0)
				return 0;
			if (x >= 1)
				return 1;

			float t2 = x;
			float x2 = 0;
			float d2 = 0;

			for (int i = 0; i < 8; i++)
			{
				SampleValueAtT(t2, polynomial, out x2);
				x2 -= x;
				if (math.abs(x2) < epsilon)
					return t2;
				SampleDerivativeAtT(t2, polynomial, out d2);
				if (d2 < epsilon)
					break;
				t2 -= x2 / d2;
			}

			float t0 = 0;
			float t1 = 1;

			t2 = x;

			while (t0 < t1)
			{
				SampleValueAtT(t2, polynomial, out x2);
				if (math.abs(x2 - x) < epsilon)
					return t2;
				if (x > x2)
					t0 = t2;
				else
					t1 = t2;

				t2 = (t1 - t0) * 0.5f + t0;
			}

			return t2;
		}

		/// <summary>
		/// Calculate polynomials for normalized Bezier, where p0 = (0,0), p3 (1,1)
		/// </summary>
		/// <param name="controlPoints"> controls p1 and p2 coordinates in order: point1x, point2x, point1y, point2y</param>
		/// <param name="result">polynomials for x and y: result[0] - x, result[1] - y</param>
		[BurstCompile(FloatMode = FloatMode.Fast)]
		public static void CalculatePolynomial(in float4 controlPoints, out float3x2 result)
		{
			result = new float3x2();
			result[0].z = 3 * controlPoints.x;
			result[0].y = 3 * (controlPoints.y - controlPoints.x) - result[0].z;
			result[0].x = 1 - result[0].y - result[0].z;
			result[1].z = 3 * controlPoints.z;
			result[1].y = 3 * (controlPoints.w - controlPoints.z) - result[1].z;
			result[1].x = 1 - result[1].y - result[1].z;
		}

		[BurstCompile(FloatMode = FloatMode.Fast)]
		public static void SampleValueAtT(in float t, in float3 polinomial, out float result)
		{
			result = ((t * polinomial.x + polinomial.y) * t + polinomial.z) * t;
		}

		[BurstCompile(FloatMode = FloatMode.Fast)]
		public static void SampleDerivativeAtT(in float t, in float3 polinomial, out float result)
		{
			result = (3 * polinomial.x * t + 2 * polinomial.y) * t + polinomial.z;
		}
	}
}
