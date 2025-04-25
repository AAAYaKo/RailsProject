using System;
using Rails.Editor.ViewModel;
using Rails.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace Rails.Editor
{
	public class EditorContext
	{
		public RailsClipViewModel SelectedClip { get; set; }
		public static EditorContext Instance => _instance ??= new();
		public RailsAnimator CurrentTarget { get; private set; }
		public RailsAnimatorViewModel ViewModel { get; private set; } = new();
		private static EditorContext _instance;
		public event Action<RailsAnimator> CurrentTargetChanged;


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
			Undo.RecordObject(CurrentTarget, undoRecordName);
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