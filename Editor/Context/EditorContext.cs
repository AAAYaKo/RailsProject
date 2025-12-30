using System;
using Rails.Editor.ViewModel;
using Rails.Runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace Rails.Editor.Context
{
	public class EditorContext
	{
		public RailsClipViewModel SelectedClip => ViewModel.SelectedClip;
		public static EditorContext Instance => _instance ??= new();
		public RailsAnimator CurrentTarget
		{
			get => currentTarget;
			internal set
			{
				if (currentTarget == value)
					return;
				currentTarget = value;
				CurrentTargetChanged?.Invoke(value);
			}
		}
		public RailsAnimatorViewModel ViewModel { get; internal set; }
		public float FramePixelSize { get; internal set; }
		public float TimePosition { get; internal set; }
		public DataStorage DataStorage { get; internal set; }
		public RailsEditor Editor { get; internal set; }
		public RailsInspector Inspector { get; internal set; }

		private static EditorContext _instance;
		private RailsAnimator currentTarget;

		public event Action<RailsAnimator> CurrentTargetChanged;


		private EditorContext()
		{
			RegisterConverters();
			Selection.selectionChanged += TargetChangedHandler;
			RailsAnimatorEditor.AnimatorDestroyed += AnimatorDestroyed;
			RailsAnimator.AnimatorReset += AnimatorReset;
			TargetChangedHandler();
			EventBus.Subscribe<FramePixelSizeChangedEvent>(OnFramePixelSizeChanged);
			EventBus.Subscribe<TimePositionChangedEvent>(OnTimePositionChanged);
		}

		public void Record(string undoRecordName)
		{
			if (CurrentTarget == null)
				return;
			Undo.RecordObject(CurrentTarget, $"Rails({CurrentTarget.name}) " + undoRecordName);
		}

		public void Record(UnityEngine.Object target, string undoRecordName)
		{
			if (CurrentTarget == null)
				return;
			Undo.RecordObjects(new UnityEngine.Object[] { CurrentTarget, target }, $"Rails({CurrentTarget.name}) " + undoRecordName);
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
			ConverterGroups.RegisterGlobalConverter((ref bool x) =>
			{
				return x ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None);
			});
			ConverterGroups.RegisterGlobalConverter((ref AnimationTime x) => x.Frames);
			ConverterGroups.RegisterGlobalConverter((ref int x) => new AnimationTime() { Frames = x });
		}

		private void TargetChangedHandler()
		{
			if (Selection.activeGameObject == null)
				return;
			if (!Selection.activeGameObject.TryGetComponent<RailsAnimator>(out var next))
				return;

			CurrentTarget = next;
		}

		private void OnFramePixelSizeChanged(FramePixelSizeChangedEvent evt)
		{
			FramePixelSize = evt.FramePixelSize;
		}

		private void OnTimePositionChanged(TimePositionChangedEvent evt)
		{
			TimePosition = evt.TimePosition;
		}
	}
}