using System;
using Rails.Runtime;
using UnityEditor;

namespace Rails.Editor
{
	[CustomEditor(typeof(RailsAnimator))]
	public class RailsAnimatorEditor : UnityEditor.Editor
	{
		public static event Action AnimatorDestroyed;


		private void OnDisable()
		{
			if (target == null)
				AnimatorDestroyed?.Invoke();
		}
	}
}