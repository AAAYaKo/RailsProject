using System;
using Rails.Editor.ViewModel;
using Rails.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor
{
	public class EditorContext
	{
		public RailsClipViewModel SelectedClip => ViewModel.SelectedClip;
		public static EditorContext Instance => _instance ??= new();
		public RailsAnimator CurrentTarget { get; private set; }
		public RailsAnimatorViewModel ViewModel { get; private set; } = new();
		public float FramePixelSize
		{
			get => framePixelSize;
			set
			{
				if (framePixelSize == value)
					return;
				framePixelSize = value;
				FramePixelSizeChanged?.Invoke(framePixelSize);
			}
		}
		private static EditorContext _instance;
		public event Action<Vector2> TrackScrollPerformed;
		public event Action<RailsAnimator> CurrentTargetChanged;
		public event Action<float> FramePixelSizeChanged;
		private float framePixelSize = 30;


		private EditorContext()
		{
			RegisterConverters();
			Selection.selectionChanged += TargetChangedHandler;
			RailsAnimatorEditor.AnimatorDestroyed += AnimatorDestroyed;
			RailsAnimator.AnimatorReset += AnimatorReset;
			TargetChangedHandler();
		}

		public void Record(string undoRecordName)
		{
			Undo.RecordObject(CurrentTarget, $"Rails({CurrentTarget.name}) " + undoRecordName);
		}

		public void AnimatorDestroyed()
		{
			if (CurrentTarget != null)
				return;

			CurrentTarget = null;
			CurrentTargetChanged?.Invoke(CurrentTarget);
		}

		public void AnimatorReset(RailsAnimator animator)
		{
			if (CurrentTarget == animator)
				return;
			TargetChangedHandler();
		}

		public void PerformTrackScroll(Vector2 delta)
		{
			TrackScrollPerformed?.Invoke(delta);
		}

		private void RegisterConverters()
		{
			ConverterGroups.RegisterGlobalConverter((ref ToggleButtonGroupState x) =>
			{
				for (int i = 0; i < x.length; i++)
				{
					if (x[i])
						return (RailsEase.EaseType)i;
				}
				return RailsEase.EaseType.NoAnimation;
			});
			ConverterGroups.RegisterGlobalConverter((ref RailsEase.EaseType x) =>
			{
				int length = Enum.GetNames(typeof(RailsEase.EaseType)).Length;
				ToggleButtonGroupState result = new(0, length);
				result.ResetAllOptions();
				result[(int)x] = true;
				return result;
			});
		}

		private void TargetChangedHandler()
		{
			if (Selection.activeGameObject == null)
				return;
			if (!Selection.activeGameObject.TryGetComponent<RailsAnimator>(out var next))
				return;

			if (CurrentTarget == next)
				return;

			CurrentTarget = next;
			ViewModel.UnbindModel();
			ViewModel.BindModel(CurrentTarget);
			CurrentTargetChanged?.Invoke(CurrentTarget);
		}
	}
}