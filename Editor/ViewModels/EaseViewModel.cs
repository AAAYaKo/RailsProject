using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DG.Tweening;
using Rails.Editor.Context;
using Rails.Runtime;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;

namespace Rails.Editor.ViewModel
{
	public class EaseViewModel : BaseNotifyPropertyViewModel<RailsEase>
	{
		private static readonly Ease[] easeVariants;
		private static readonly HashSet<Ease> notSupportedList = new()
		{
			Ease.Unset,
			Ease.INTERNAL_Custom,
			Ease.INTERNAL_Zero,
			Ease.Flash,
			Ease.InFlash,
			Ease.OutFlash,
			Ease.InOutFlash,
		};

		[CreateProperty]
		public Vector2 FirstPoint
		{
			get => firstPoint ?? Vector2.zero;
			set
			{
				if (Utils.Approximately(value, FirstPoint))
					return;
				firstPoint = value;
				NotifyPropertyChanged();
				NotifyPropertyChanged(nameof(Spline));
			}
		}
		[CreateProperty]
		public Vector2 SecondPoint
		{
			get => secondPoint ?? Vector2.one;
			set
			{
				if (Utils.Approximately(value, SecondPoint))
					return;
				secondPoint = value;
				NotifyPropertyChanged();
				NotifyPropertyChanged(nameof(Spline));
			}
		}
		[CreateProperty]
		public RailsEase.EaseType EaseType
		{
			get => easeType ?? RailsEase.EaseType.NoAnimation;
			set
			{
				if (easeType == value)
					return;
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
			set => SetProperty(ref hasHandles, value);
		}
		[CreateProperty]
		public bool HasFunction
		{
			get => hasFunction ?? false;
			set => SetProperty(ref hasFunction, value);
		}
		[CreateProperty]
		public List<Ease> EaseVariants
		{
			get => easeVariants.ToList();
		}
		[CreateProperty]
		public Ease SelectedVariant
		{
			get => selectedVariant ?? Ease.Linear;
			set
			{
				if (SetProperty(ref selectedVariant, value))
					NotifyPropertyChanged(nameof(Spline));
			}
		}
		[CreateProperty]
		public bool ShowEaseFoldout
		{
			get => showEaseFoldout;
			set => SetProperty(ref showEaseFoldout, value);
		}

		[CreateProperty]
		public ICommand<Ease> EaseFunctionChangeCommand { get; set; }
		[CreateProperty]
		public ICommand<RailsEase.EaseType> EaseTypeChangeCommand { get; set; }
		[CreateProperty]
		public ICommand<Vector2> FirstPointChangeCommand { get; set; }
		[CreateProperty]
		public ICommand<Vector2> SecondPointChangeCommand { get; set; }

		private bool? hasHandles;
		private bool? hasFunction;
		private Vector2? firstPoint;
		private Vector2? secondPoint;
		private RailsEase.EaseType? easeType;
		private Ease? selectedVariant;
		private bool showEaseFoldout = true;


		static EaseViewModel()
		{
			easeVariants = Enum
				.GetValues(typeof(Ease))
				.Cast<Ease>()
				.Where(x => !notSupportedList.Contains(x))
				.ToArray();
		}

		public EaseViewModel()
		{
			EaseFunctionChangeCommand = new RelayCommand<Ease>(x =>
			{
				EditorContext.Instance.Record("Key Ease Function Changed");
				model.EaseFunc = x;
			});

			EaseTypeChangeCommand = new RelayCommand<RailsEase.EaseType>(x =>
			{
				EditorContext.Instance.Record("Key Ease Type Changed");
				model.Type = x;
			});

			FirstPointChangeCommand = new RelayCommand<Vector2>(x =>
			{
				EditorContext.Instance.Record("Key Ease First Control Changed");
				float4 controls = model.Controls;
				controls.x = Mathf.Clamp(x.x, 0, 1);
				controls.z = x.y;
				model.Controls = controls;
			});

			SecondPointChangeCommand = new RelayCommand<Vector2>(x =>
			{
				EditorContext.Instance.Record("Key Second Control Changed");
				float4 controls = model.Controls;
				controls.y = Mathf.Clamp(x.x, 0, 1); ;
				controls.w = x.y;
				model.Controls = controls;
			});
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
			else if (e.PropertyName == nameof(RailsEase.EaseFunc))
			{
				SelectedVariant = model.EaseFunc;
			}
		}

		protected override void OnModelChanged()
		{
			FirstPoint = model.Controls.xz;
			SecondPoint = model.Controls.yw;
			EaseType = model.Type;
			SelectedVariant = model.EaseFunc;
		}
	}
}