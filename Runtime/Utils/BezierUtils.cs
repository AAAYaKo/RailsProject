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

		/// <summary>
		/// For Normalized Bezier
		/// </summary>
		/// <param name="t"></param>
		/// <param name="polynomial"></param>
		/// <param name="result"></param>
		[BurstCompile(FloatMode = FloatMode.Fast)]
		public static void SampleValueAtT(in float t, in float3 polynomial, out float result)
		{
			result = ((t * polynomial.x + polynomial.y) * t + polynomial.z) * t;
		}
		/// <summary>
		/// For generic Bezier
		/// </summary>
		/// <param name="t"></param>
		/// <param name="polynomial"></param>
		/// <param name="result"></param>
		[BurstCompile(FloatMode = FloatMode.Fast)]
		public static void SampleValueAtT(in float t, in float4 polynomial, out float result)
		{
			result = ((t * polynomial.x + polynomial.y) * t + polynomial.z) * t + polynomial.w;
		}

		[BurstCompile(FloatMode = FloatMode.Fast)]
		public static void SampleDerivativeAtT(in float t, in float3 polynomial, out float result)
		{
			result = (3 * polynomial.x * t + 2 * polynomial.y) * t + polynomial.z;
		}

		/// <summary>
		/// Solve Full Cubic Bezier and Find Min, Max Y
		/// </summary>
		/// <param name="point0Y"></param>
		/// <param name="point1Y"></param>
		/// <param name="point2Y"></param>
		/// <param name="point3Y"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		[BurstCompile(FloatMode = FloatMode.Fast)]
		public static void SolveMinMaxY(in float point0Y, in float point1Y, in float point2Y, in float point3Y, out float min, out float max)
		{
			float4 polynomial = new(
				-point0Y + 3 * point1Y - 3 * point2Y + point3Y,
				3 * point0Y - 6 * point1Y + 3 * point2Y,
				-3 * point0Y + 3 * point1Y,
				point0Y
			);

			min = math.min(point0Y, point3Y);
			max = math.max(point0Y, point3Y);

			float3 derivative = new (
				3 * polynomial.x,
				2 * polynomial.y,
				polynomial.z
				);

			if (math.abs(derivative.x - 0) < epsilon)
			{
				if (math.abs(derivative.y - 0) > epsilon)
				{
					float t = -derivative.z / derivative.y;
					if (t >= 0 && t <= 1)
					{
						SampleValueAtT(t, polynomial, out float y);
						min = math.min(min, y);
						max = math.max(max, y);
					}
				}
			}
			else
			{
				float discriminant = math.square(derivative.y) - 4 * derivative.x * derivative.z;
				if (discriminant >= 0)
				{
					float sqrtDiscriminant = math.sqrt(discriminant);
					float t1 = (-derivative.y + sqrtDiscriminant) / (2 * derivative.x);
					float t2 = (-derivative.y - sqrtDiscriminant) / (2 * derivative.x);

					if (t1 >= 0 && t1 <= 1)
					{
						SampleValueAtT(t1, polynomial, out float y);
						min = math.min(min, y);
						max = math.max(max, y);
					}
					if (t2 >= 0 && t2 <= 1)
					{
						SampleValueAtT(t2, polynomial, out float y);
						min = math.min(min, y);
						max = math.max(max, y);
					}
				}
			}
		}
	}
}
