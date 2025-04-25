using System;
using System.ComponentModel;
using Rails.Runtime.Tracks;
using Unity.Properties;

namespace Rails.Editor.ViewModel
{
	public class AnimationTrackViewModel : BaseNotifyPropertyViewModel<AnimationTrack>
	{
		[CreateProperty]
		public UnityEngine.Object Reference
		{
			get => reference;
			set
			{
				if (reference == value)
					return;
				reference = value;
				NotifyPropertyChanged();
				model.SceneReference = reference;
			}
		}
		[CreateProperty]
		public Type Type => type;
		[CreateProperty]
		public AnimationTrack.ValueType ValueType => valueType;

		private UnityEngine.Object reference;
		private Type type;
		private AnimationTrack.ValueType valueType;


		protected override void OnModelChanged()
		{
			if (model == null)
				return;

			Reference = model.SceneReference;
			type = model.AnimationComponentType;
			NotifyPropertyChanged(nameof(Type));
			valueType = model.Type;
			NotifyPropertyChanged(nameof(ValueType));
		}

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(AnimationTrack.SceneReference))
				Reference = model.SceneReference;
		}
	}
}