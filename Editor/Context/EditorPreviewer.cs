using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.EditorCoroutines.Editor;
using UnityEditor;

namespace Rails.Editor.Context
{
	public static class EditorPreviewer
	{
		public static bool IsPreviewing { get; private set; }

		private static Action<float> onPreviewUpdated;
		private static readonly List<UnityEngine.Object> uiGraphics = new();
		private static readonly List<Tween> tweens = new();
		private static EditorCoroutine updateRoutine;


		#region Public Methods

		/// <summary>
		/// Starts the update loop of tween in the editor. Has no effect during playMode.
		/// </summary>
		/// <param name="onPreviewUpdated">Eventual callback to call after every update</param>
		public static void Start(float targetFrameTime, Action<float> onPreviewUpdated = null)
		{
			if (IsPreviewing || EditorApplication.isPlayingOrWillChangePlaymode || updateRoutine != null)
				return;

			IsPreviewing = true;
			EditorPreviewer.onPreviewUpdated = onPreviewUpdated;
			float threshold = targetFrameTime * 0.15f;
			updateRoutine = EditorCoroutineUtility.StartCoroutineOwnerless(PreviewUpdateRoutine(targetFrameTime, threshold));
		}

		/// <summary>
		/// Stops the update loop and clears the onPreviewUpdated callback.
		/// </summary>
		/// <param name="resetTweenTargets">If TRUE also resets the tweened objects to their original state.
		/// Note that this works by calling Rewind on all tweens, so it will work correctly
		/// only if you have a single tween type per object and it wasn't killed</param>
		/// <param name="clearTweens">If TRUE also kills any cached tween</param>
		public static void Stop(bool clearTweens = true)
		{
			IsPreviewing = false;
			uiGraphics.Clear();
			EditorCoroutineUtility.StopCoroutine(updateRoutine);
			updateRoutine = null;
			onPreviewUpdated = null;

			foreach (Tween tween in tweens)
			{
				try
				{
					tween.Rewind();
				}
				catch
				{
					// Ignore
				}
			}
			if (clearTweens)
				tweens.Clear();
			else
				ValidateTweens();
		}

		/// <summary>
		/// Readies the tween for editor preview by setting its UpdateType to Manual plus eventual extra settings.
		/// </summary>
		/// <param name="tween">The tween to ready</param>
		/// <param name="clearCallbacks">If TRUE (recommended) removes all callbacks (OnComplete/Rewind/etc)</param>
		/// <param name="preventAutoKill">If TRUE prevents the tween from being auto-killed at completion</param>
		/// <param name="andPlay">If TRUE starts playing the tween immediately</param>
		public static void PrepareTweenForPreview(Tween tween, IEnumerable<UnityEngine.Object> animatedObjects, bool clearCallbacks = false, bool preventAutoKill = true, bool andPlay = true)
		{
			tweens.Add(tween);
			uiGraphics.AddRange(animatedObjects);
			tween.SetUpdate(UpdateType.Manual);
			if (preventAutoKill)
				tween.SetAutoKill(false);
			if (clearCallbacks)
			{
				tween.OnComplete(null)
					.OnStart(null).OnPlay(null).OnPause(null).OnUpdate(null).OnWaypointChange(null)
					.OnStepComplete(null).OnRewind(null).OnKill(null);
			}
			if (andPlay)
				tween.Play();
		}

		#endregion
		private static IEnumerator PreviewUpdateRoutine(float targetFrameTime, float threshold)
		{
			double currentTime = EditorApplication.timeSinceStartup;
			while (IsPreviewing)
			{
				yield return null;

				double previousTime = currentTime;
				float delta = (float)(EditorApplication.timeSinceStartup - previousTime);
				if (delta >= targetFrameTime - threshold)
				{
					currentTime = EditorApplication.timeSinceStartup;
					DOTween.ManualUpdate(1 / 60f, 1 / 60f);

					// Force visual refresh of UI objects
					// (a simple SceneView.RepaintAll won't work with UI elements)
					//foreach (UnityEngine.Object obj in uiGraphics)
					//	EditorUtility.SetDirty(obj);

					onPreviewUpdated?.Invoke(delta);
				}
			}
		}

		private static void ValidateTweens()
		{
			List<Tween> toRemove = new();
			foreach (var tween in tweens)
			{
				if (tween == null || !tween.active)
					toRemove.Add(tween);
			}
			toRemove.ForEach(x => tweens.Remove(x));
		}
	}
}