//Based on
//https://github.com/qwe321qwe321qwe321/Unity-EasingAnimationCurve
// https://github.com/thednp/bezier-easing/blob/master/src/index.ts

using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

namespace Rails.Runtime
{
	[Serializable]
	public class RailsEase : BaseSerializableNotifier
	{
		#region Ease Preview Spline Curves
		private static readonly Dictionary<Ease, Vector2[]> _easeSplines = new()
		{
			{
				Ease.Linear, new Vector2[]
				{
					new (0,0),
					new (0,0),
					new (1,1),
					new (1,1),
				}
			},
			{
				Ease.InSine, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.360780f, -0.000436f),
					new (0.673486f, 0.486554f),
					new (1.0f, 1.0f)
				}
			},
			{
				Ease.OutSine, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.330931f, 0.520737f),
					new (0.641311f, 1.000333f),
					new (1.0f, 1.0f)
				}
			},
			{
				Ease.InOutSine, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.180390f, -0.000217f),
					new (0.336743f, 0.243277f),
					new (0.5f, 0.5f),
					new (0.665465f, 0.760338f),
					new (0.820656f, 1.000167f),
					new (1.0f, 1.0f)
				}
			},
			{
				Ease.InQuad, new Vector2[]
				{
					new(0.0f, 0.0f),
					new(0.333333f, 0.0f),
					new(0.666667f, 0.333333f),
					new(1.0f, 1.0f)
				}
			},
			{
				Ease.OutQuad, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.333333f, 0.666667f),
					new (0.666667f, 1.0f),
					new (1.0f, 1.0f)
				}
			},
			{
				Ease.InOutQuad, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.166667f, 0.0f),
					new (0.333333f, 0.166667f),
					new (0.5f, 0.5f),
					new (0.666667f, 0.833333f),
					new (0.833333f, 1.0f),
					new (1.0f, 1.0f)
				}
			},
			{
				Ease.InCubic, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.333333f, 0.0f),
					new (0.666667f, 0.0f),
					new (1.0f, 1.0f)
				}
			},
			{
				Ease.OutCubic, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.333333f, 1.0f),
					new (0.666667f, 1.0f),
					new (1.0f, 1.0f)
				}
			},
			{
				Ease.InOutCubic, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.166667f, 0.0f),
					new (0.333333f, 0.0f),
					new (0.5f, 0.5f),
					new (0.666667f, 1.0f),
					new (0.833333f, 1.0f),
					new (1.0f, 1.0f)
				}
			},
			{
				Ease.InQuart, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.434789f, 0.006062f),
					new (0.730901f, -0.07258f),
					new (1.0f, 1.0f)
				}
			},
			{
				Ease.OutQuart, new Vector2[]
				{
					new(0.0f, 0.0f),
					new (0.269099f, 1.072581f),
					new (0.565211f, 0.993938f),
					new (1.0f, 1.0f)
				}
			},
			{
				Ease.InOutQuart, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.217394f, 0.003031f),
					new (0.365451f, -0.036291f),
					new (0.5f, 0.5f),
					new (0.634549f, 1.036290f),
					new (0.782606f, 0.996969f),
					new (1.0f, 1.0f)
				}
			},
			{
				Ease.InQuint, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.519568f, 0.012531f),
					new (0.774037f, -0.118927f),
					new (1.0f, 1.0f)
				}
			},
			{
				Ease.OutQuint, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.225963f, 1.11926f),
					new (0.481099f, 0.987469f),
					new (1.0f, 1.0f)
				}
			},
			{
				Ease.InOutQuint, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.259784f, 0.006266f),
					new (0.387018f, -0.059463f),
					new (0.5f, 0.5f),
					new (0.612982f, 1.059630f),
					new (0.740549f, 0.993734f),
					new (1.0f, 1.0f)
				}
			},
			{
				Ease.InExpo, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.636963f, 0.0199012f),
					new (0.844333f, -0.0609379f),
					new (1.0f, 1.0f),
				}
			},
			{
				Ease.OutExpo, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.155667f, 1.060938f),
					new (0.363037f, 0.980099f),
					new (1.0f, 1.0f)
				}
			},
			{
				Ease.InOutExpo, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.318482f, 0.009951f),
					new (0.422167f, -0.030469f),
					new (0.5f, 0.5f),
					new (0.577833f, 1.0304689f),
					new (0.681518f, 0.9900494f),
					new (1.0f, 1.0f)
				}
			},
			{
				Ease.InCirc, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.55403f, 0.001198f),
					new (0.998802f, 0.449801f),
					new (1.0f, 1.0f)
				}
			},
			{
				Ease.OutCirc, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.001198f, 0.553198f),
					new (0.445976f, 0.998802f),
					new (1.0f, 1.0f)
				}
			},
			{
				Ease.InOutCirc, new Vector2[]
				{
				new (0.0f, 0.0f),
				new (0.277013f, 0.000599f),
				new (0.499401f, 0.223401f),
				new (0.5f, 0.5f),
				new (0.500599f, 0.776599f),
				new (0.722987f, 0.999401f),
				new (1.0f, 1.0f)
				}
			},
			{
				Ease.InElastic, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.175f, 0.00250747f),
					new (0.173542f, 0.0f),
					new (0.175f, 0.0f),

					new (0.4425f, -0.0184028f),
					new (0.3525f, 0.05f),
					new (0.475f, 0.0f),

					new (0.735f, -0.143095f),
					new (0.6575f, 0.383333f),
					new (0.775f, 0.0f),

					new (0.908125f, -0.586139f),
					new (0.866875f, -0.666667f),
					new (1.0f, 1.0f),
				}
			},
			{
				Ease.OutElastic, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.133125f, 1.666667f),
					new (0.091875f, 1.586139f),
					new (0.225f, 1.0f),

					new (0.3425f, 0.616667f),
					new (0.265f, 1.143095f),
					new (0.525f, 1.0f),

					new (0.6475f, 0.95f),
					new (0.5575f, 1.0184028f),
					new (0.8250f, 1.0f),

					new (0.826458f, 1.0f),
					new (0.825f, 0.9974925f),
					new (1.0f, 1.0f),
				}
			},
			{
				Ease.InOutElastic, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.0875f, 0.001254f),
					new (0.086771f, 0.0f),
					new (0.0875f, 0.0f),

					new (0.22125f, -0.009201f),
					new (0.17625f, 0.025f),
					new (0.2375f, 0.0f),

					new (0.3675f, -0.071548f),
					new (0.32875f, 0.191667f),
					new (0.3875f, 0.0f),

					new (0.454063f, -0.293070f),
					new (0.433438f, -0.333334f),
					new (0.5f, 0.5f),

					new (0.5665625f, 1.333334f),
					new (0.5459375f, 1.293070f),
					new (0.6125f, 1.0f),

					new (0.67125f, 0.808334f),
					new (0.6325f, 1.071548f),
					new (0.7625f, 1.0f),

					new (0.82375f, 0.975f),
					new (0.77875f, 1.009201f),
					new (0.9125f, 1.0f),

					new (0.913229f, 1.0f),
					new (0.9125f, 0.9987463f),
					new (1.0f, 1.0f),
				}
			},
			{
				Ease.InBack, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.333333f, 0.0f),
					new (0.666667f, -0.567193f),
					new (1.0f, 1.0f),
				}
			},
			{
				Ease.OutBack, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.333333f, 1.567193f),
					new (0.666667f, 1.0f),
					new (1.0f, 1.0f),
				}
			},
			{
				Ease.InOutBack, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.166667f, 0.0f),
					new (0.333333f, -0.432485f),
					new (0.5f, 0.5f),

					new (0.666667f, 1.432485f),
					new (0.833333f, 1.0f),
					new (1.0f, 1.0f)
				}
			},
			{
				Ease.InBounce, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.030303f, 0.020833f),
					new (0.060606f, 0.020833f),
					new (0.0909f, 0.0f),

					new (0.151515f, 0.083333f),
					new (0.212121f, 0.083333f),
					new (0.2727f, 0.0f),

					new (0.393939f, 0.333333f),
					new (0.515152f, 0.333333f),
					new (0.6364f, 0.0f),

					new (0.757576f, 0.666667f),
					new (0.878788f, 1.0f),
					new (1.0f, 1.0f),
				}
			},
			{
				Ease.OutBounce, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.121212f, 0.0f),
					new (0.242424f, 0.333333f),
					new (0.3636f, 1.0f),

					new (0.484848f, 0.666667f),
					new (0.606060f, 0.666667f),
					new (0.7273f, 1.0f),

					new (0.787879f, 0.916667f),
					new (0.848485f, 0.916667f),
					new (0.9091f, 1.0f),

					new (0.939394f, 0.9791667f),
					new (0.969697f, 0.9791667f),
					new (1.0f, 1.0f),
				}
			},
			{
				Ease.InOutBounce, new Vector2[]
				{
					new (0.0f, 0.0f),
					new (0.015152f, 0.010417f),
					new (0.030303f, 0.010417f),
					new (0.0455f, 0.0f),

					new (0.075758f, 0.041667f),
					new (0.106061f, 0.041667f),
					new (0.1364f, 0.0f),

					new (0.196970f, 0.166667f),
					new (0.257576f, 0.166667f),
					new (0.3182f, 0.0f),

					new (0.378788f, 0.333333f),
					new (0.439394f, 0.5f),
					new (0.5f, 0.5f),

					new (0.560606f, 0.5f),
					new (0.621212f, 0.666667f),
					new (0.6818f, 1.0f),

					new (0.742424f, 0.833333f),
					new (0.803030f, 0.833333f),
					new (0.8636f, 1.0f),

					new (0.893939f, 0.958333f),
					new (0.924242f, 0.958333f),
					new (0.9550f, 1.0f),

					new (0.969697f, 0.989583f),
					new (0.984848f, 0.989583f),
					new (1.0f, 1.0f),
}
			},
		};
		private static readonly Vector2[] _splineNoAnimation = new Vector2[]
		{
			new(0,0),
			new(0,0),
			new(1,0),
			new(1,0),
			new(1,0),
			new(1,1),
			new(1,1),
		};
		#endregion

		[SerializeField] private EaseType easeType;
		[SerializeField] private float4 controls = new(1 / 3f, 1 / 6f, 0, 1);
		[SerializeField] private Ease ease = Ease.Linear;

		public EaseType Type
		{
			get => easeType;
			set
			{
				if (SetProperty(ref easeType, value))
					CalculatePolynomial();
			}
		}

		/// <summary>
		/// format (x1, x2, y1, y2)
		/// </summary>
		public float4 Controls
		{
			get => controls;
			set
			{
				if (SetProperty(ref controls, value, VectorComparer.Instance))
					CalculatePolynomial();
			}
		}

		public Ease EaseFunc
		{
			get => ease;
			set => SetProperty(ref ease, value);
		}

		private float3x2? polynomial;
#if UNITY_EDITOR
		[NonSerialized] private EaseType easeTypeCopy;
		[NonSerialized] private float4 controlsCopy;
		[NonSerialized] private Ease easeFuncCopy;
#endif


		public Vector2[] GetEaseSpline() => easeType switch
		{
			EaseType.NoAnimation => _splineNoAnimation,
			EaseType.EaseFunction => _easeSplines[ease],
			EaseType.EaseCurve => new Vector2[]
			{
				new(0,0),
				new(controls.x, controls.z),
				new(controls.y, controls.w),
				new(1,1),
			},
			_ => _splineNoAnimation,
		};

		public float CurveFunction(float time, float duration, float overshootOrAmplitude, float period)
		{
			if (polynomial == null)
				CalculatePolynomial();
			return BezierUtils.GetBezierYbyX(time / duration, polynomial ?? float3x2.zero);
		}

		public float Eased(float t)
		{
			return EasedValue(0, 1, t);
		}

		public float EasedValue(float from, float to, float t)
		{
			if (Type is EaseType.NoAnimation)
				return from;
			if (Type is EaseType.EaseCurve)
			{
				if (polynomial == null)
					CalculatePolynomial();
				float y = BezierUtils.GetBezierYbyX(t, polynomial ?? float3x2.zero);
				return math.lerp(from, to, y);
			}
			return DOVirtual.EasedValue(from, to, t, EaseFunc);
		}

		public Vector2 EasedValue(Vector2 from, Vector2 to, float t)
		{
			if (Type is EaseType.NoAnimation)
				return from;
			if (Type is EaseType.EaseCurve)
			{
				if (polynomial == null)
					CalculatePolynomial();
				float y = BezierUtils.GetBezierYbyX(t, polynomial ?? float3x2.zero);
				return math.lerp(from, to, y);
			}
			return DOVirtual.EasedValue(from, to, t, EaseFunc);
		}

		public Vector3 EasedValue(Vector3 from, Vector3 to, float t)
		{
			if (Type is EaseType.NoAnimation)
				return from;
			if (Type is EaseType.EaseCurve)
			{
				if (polynomial == null)
					CalculatePolynomial();
				float y = BezierUtils.GetBezierYbyX(t, polynomial ?? float3x2.zero);
				return math.lerp(from, to, y);
			}
			return DOVirtual.EasedValue(from, to, t, EaseFunc);
		}

		public override void OnBeforeSerialize()
		{
#if UNITY_EDITOR
			easeTypeCopy = Type;
			controlsCopy = Controls;
			easeFuncCopy = EaseFunc;
#endif
		}

		public override void OnAfterDeserialize()
		{
#if UNITY_EDITOR
			if (NotifyIfChanged(Type, easeTypeCopy, nameof(Type)))
				easeTypeCopy = Type;
			if (NotifyIfChanged(Controls, controlsCopy, nameof(Controls), VectorComparer.Instance))
				controlsCopy = Controls;
			if (NotifyIfChanged(EaseFunc, easeFuncCopy, nameof(EaseFunc)))
				easeFuncCopy = EaseFunc;
#endif
		}

		private void CalculatePolynomial()
		{
			if (Type is EaseType.NoAnimation or EaseType.EaseFunction)
				this.polynomial = null;
			BezierUtils.CalculatePolynomial(controls, out var polynomial);
			this.polynomial = polynomial;
		}

		public enum EaseType
		{
			NoAnimation,
			EaseCurve,
			EaseFunction,
		}
	}
}
