//Based on
//https://github.com/qwe321qwe321qwe321/Unity-EasingAnimationCurve
// https://github.com/thednp/bezier-easing/blob/master/src/index.ts

using System.Diagnostics;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

namespace Rails.Runtime
{
	//[Serializable]
	[CreateAssetMenu]
	public class EaseTest : ScriptableObject
	{
		[SerializeField] private RailsEase _ease;

		public RailsEase Ease => _ease;
		//[SerializeField] private float4 controls;
		//[SerializeField] private DG.Tweening.Ease ease;
		//[SerializeField] private AnimationCurve curve = null;
		//[SerializeField] private RectTransform test1;
		//[SerializeField] private RectTransform test2;


		//private void Start()
		//{
		//	test1.DOAnchorPosX(300, 3)
		//		.SetEase(DG.Tweening.Ease.OutQuad);

		//	Stopwatch stopwatch = new();
		//	stopwatch.Start();
		//	BezierUtils.CalculatePolynomial(new float4(0.333333f, 0.666667f, 0.666667f, 1.0f), out float3x2 polynomial);
		//	stopwatch.Stop();
		//	UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds);
		//	test2.DOAnchorPosX(300, 3)
		//		.SetEase((float time, float duration, float overshootOrAmplitude, float period) =>
		//		{
		//			return BezierUtils.GetBezierYbyX(time / duration, polynomial);
		//		});
		//}
	}
}
