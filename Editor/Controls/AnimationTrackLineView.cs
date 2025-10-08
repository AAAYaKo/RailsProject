using System.Collections.Generic;
using System.Linq;
using Rails.Editor.ViewModel;
using Rails.Runtime.Tracks;
using UnityEngine.UIElements;

namespace Rails.Editor.Controls
{
	[UxmlElement]
	public partial class AnimationTrackLineView : BaseTrackLineView<AnimationKeyViewModel, AnimationKey>
	{
		private List<TrackTweenLineView> tweenLines = new();
		private Dictionary<int, TrackTweenLineView> keyToTweenLines = new();


		public AnimationTrackLineView() : base()
		{
			AddToClassList("animation-track-line");
		}

		protected override void SelectVisually(TrackKeyView key, int keyIndex)
		{
			base.SelectVisually(key, keyIndex);
			if (keyToTweenLines.ContainsKey(keyIndex))
				keyToTweenLines[keyIndex].AddToClassList(SelectedClass);
		}

		protected override void DeselectVisually(TrackKeyView key, int keyIndex)
		{
			base.DeselectVisually(key, keyIndex);
			if (keyToTweenLines.ContainsKey(keyIndex))
				keyToTweenLines[keyIndex].RemoveFromClassList(SelectedClass);
		}

		private void UpdateTweenLines()
		{
			if (Values.IsNullOrEmpty())
			{
				tweenLines.ForEach(x =>
				{
					x.Unbind();
					container.Remove(x);
				});
				tweenLines.Clear();
				return;
			}
			int count = Values.Take(Values.Count - 1).Count(x => x.Ease.EaseType is not Runtime.RailsEase.EaseType.NoAnimation);

			while (count > tweenLines.Count)
			{
				var line = CreateTweenLine();
				container.Add(line);
				tweenLines.Add(line);
			}
			while (count < tweenLines.Count)
			{
				var line = tweenLines[^1];
				line.Unbind();
				container.Remove(line);
				tweenLines.Remove(line);
			}

			int lineI = 0;
			keyToTweenLines.Clear();
			for (int i = 0; i < Values.Count - 1; i++)
			{
				var previous = Values[i];
				if (previous.Ease.EaseType is not Runtime.RailsEase.EaseType.NoAnimation)
				{
					TrackTweenLineView line = tweenLines[lineI];
					keyToTweenLines.Add(i, line);
					lineI++;
					line.Unbind();
					line.Bind(views[i], views[i + 1]);
					if (SelectedIndexes?.Contains(i) == true)
						line.AddToClassList(SelectedClass);
				}
			}
		}

		private TrackTweenLineView CreateTweenLine()
		{
			TrackTweenLineView line = new();
			return line;
		}

		protected override void UpdateList()
		{
			base.UpdateList();
			UpdateTweenLines();
		}

		protected override TrackKeyView CreateElement()
		{
			TrackKeyView key = new();
			return key;
		}
	}
}