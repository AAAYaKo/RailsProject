using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Rails.Runtime;
using Unity.Properties;
using UnityEngine;

namespace Rails.Editor.ViewModel
{
	public class EaseViewModel : BaseNotifyPropertyViewModel<RailsEase>
	{
		private static readonly DG.Tweening.Ease[] easeVariants;

		[CreateProperty]
		public Vector2 FirstPoint
		{
			get => firstPoint;
			set
			{
				if (Utils.Approximately(value, firstPoint))
					return;
				//EditorContext.Instance.Record("");
				Vector4 controls = model.Controls;
				controls.x = Mathf.Clamp(value.x, 0, 1);
				controls.z = value.y;
				firstPoint = value;
				model.Controls = controls;
				NotifyPropertyChanged();
				NotifyPropertyChanged(nameof(Spline));
			}
		}

		[CreateProperty]
		public Vector2 SecondPoint
		{
			get => secondPoint;
			set
			{
				if (Utils.Approximately(value, secondPoint))
					return;
				//EditorContext.Instance.Record("");
				Vector4 controls = model.Controls;
				controls.y = Mathf.Clamp(value.x, 0, 1);
				controls.w = value.y;
				secondPoint = value;
				model.Controls = controls;
				NotifyPropertyChanged();
				NotifyPropertyChanged(nameof(Spline));
			}
		}

		[CreateProperty]
		public RailsEase.EaseType EaseType
		{
			get => easeType;
			set
			{
				if (easeType == value)
					return;
				//EditorContext.Instance.Record("");
				model.Type = value;
				easeType = value;
				NotifyPropertyChanged();
				NotifyPropertyChanged(nameof(Spline));
				HasHandles = model.Type is RailsEase.EaseType.EaseCurve;
				HasFunction = model.Type is RailsEase.EaseType.EaseFunction;
			}
		}

		[CreateProperty]
		public Vector2[] Spline
		{
			get => model.GetEaseSpline();
		}

		[CreateProperty]
		public bool HasHandles
		{
			get => hasHandles ?? false;
			set
			{
				if (hasHandles != value)
				{
					hasHandles = value;
					NotifyPropertyChanged();
				}
			}
		}

		[CreateProperty]
		public bool HasFunction
		{
			get => hasFunction ?? false;
			set
			{
				if (hasFunction != value)
				{
					hasFunction = value;
					NotifyPropertyChanged();
				}
			}
		}

		[CreateProperty]
		public List<string> EaseVariants
		{
			get => easeVariants.Select(x => x.ToString()).ToList();
		}

		[CreateProperty]
		public int SelectedVariant
		{
			get => selectedVariant;
			set
			{
				if (selectedVariant != value)
				{
					selectedVariant = value;
					model.EaseFunc = easeVariants[selectedVariant];
					NotifyPropertyChanged();
					NotifyPropertyChanged(nameof(Spline));
				}
			}
		}

		private bool? hasHandles;
		private bool? hasFunction;
		private Vector2 firstPoint;
		private Vector2 secondPoint;
		private RailsEase.EaseType easeType;
		private int selectedVariant;


		static EaseViewModel()
		{
			easeVariants = Enum
				.GetValues(typeof(DG.Tweening.Ease))
				.Cast<DG.Tweening.Ease>()
				.Where(x => x is not DG.Tweening.Ease.Unset
				and not DG.Tweening.Ease.INTERNAL_Custom
				and not DG.Tweening.Ease.INTERNAL_Zero
				and not DG.Tweening.Ease.Flash
				and not DG.Tweening.Ease.InFlash
				and not DG.Tweening.Ease.OutFlash
				and not DG.Tweening.Ease.InOutFlash)
				.ToArray();
		}

		public float EasedValue(float from, float to, float t) => model.EasedValue(from, to, t);

		public Vector2 EasedValue(Vector2 from, Vector2 to, float t) => model.EasedValue(from, to, t);

		public Vector3 EasedValue(Vector3 from, Vector3 to, float t) => model.EasedValue(from, to, t);

		protected override void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(RailsEase.Controls))
			{
				FirstPoint = model.Controls.xz;
				SecondPoint = model.Controls.yw;
			}
			else if (e.PropertyName == nameof(RailsEase.Type))
			{
				EaseType = model.Type;
			}
			else if (e.PropertyName != nameof(RailsEase.EaseFunc))
			{
				SelectedVariant = Array.IndexOf(easeVariants, model.EaseFunc);
			}
		}

		protected override void OnModelChanged()
		{
			FirstPoint = model.Controls.xz;
			SecondPoint = model.Controls.yw;
			EaseType = model.Type;
			SelectedVariant = Array.IndexOf(easeVariants, model.EaseFunc);
		}
	}
}