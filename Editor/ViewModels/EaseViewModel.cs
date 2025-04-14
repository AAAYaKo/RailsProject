using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Rails.Runtime;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor.ViewModel
{
	public class EaseViewModel : INotifyBindablePropertyChanged
	{
		[CreateProperty]
		public Vector2 FirstPoint
		{
			get => _firstPoint;
			set
			{
				if (!Utils.Approximately(value, _firstPoint))
				{
					EditorContext.Instance.Record("");
					Vector4 controls = _model.Controls;
					controls.x = Mathf.Clamp(value.x, 0, 1);
					controls.z = value.y;
					_firstPoint = value;
					_model.Controls = controls;
					NotifyPropertyChanged();
					NotifyPropertyChanged(nameof(Spline));
				}
			}
		}

		[CreateProperty]
		public Vector2 SecondPoint
		{
			get => _secondPoint;
			set
			{
				if (!Utils.Approximately(value, _secondPoint))
				{
					EditorContext.Instance.Record("");
					Vector4 controls = _model.Controls;
					controls.y = Mathf.Clamp(value.x, 0, 1);
					controls.w = value.y;
					_secondPoint = value;
					_model.Controls = controls;
					NotifyPropertyChanged();
					NotifyPropertyChanged(nameof(Spline));
				}
			}
		}

		[CreateProperty]
		public Ease.EaseType EaseType
		{
			get => _easeType;
			set
			{
				if (_easeType != value)
				{
					EditorContext.Instance.Record("");
					_model.Type = value;
					_easeType = value;
					NotifyPropertyChanged();
					NotifyPropertyChanged(nameof(Spline));
					HasHandles = _model.Type is Ease.EaseType.EaseCurve;
					HasFunction = _model.Type is Ease.EaseType.EaseFunction;
				}
			}
		}

		[CreateProperty]
		public Vector2[] Spline
		{
			get => _model.GetEaseSpline();
		}

		[CreateProperty]
		public bool HasHandles
		{
			get => _hasHandles ?? false;
			set
			{
				if (_hasHandles != value)
				{
					_hasHandles = value;
					NotifyPropertyChanged();
				}
			}
		}

		[CreateProperty]
		public bool HasFunction
		{
			get => _hasFunction ?? false;
			set
			{
				if (_hasFunction != value)
				{
					_hasFunction = value;
					NotifyPropertyChanged();
				}
			}
		}

		[CreateProperty]
		public List<string> EaseVariants
		{
			get => _easeVariants.Select(x => x.ToString()).ToList();
		}

		[CreateProperty]
		public int SelectedVariant
		{
			get => _selectedVariant;
			set
			{
				if (_selectedVariant != value)
				{
					_selectedVariant = value;
					_model.EaseFunc = _easeVariants[_selectedVariant];
					NotifyPropertyChanged();
					NotifyPropertyChanged(nameof(Spline));
				}
			}
		}

		private bool? _hasHandles;
		private bool? _hasFunction;
		private Ease _model;
		private Vector2 _firstPoint;
		private Vector2 _secondPoint;
		private Ease.EaseType _easeType;
		private DG.Tweening.Ease[] _easeVariants;
		private int _selectedVariant;

		public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;


		public EaseViewModel(Ease model)
		{
			_model = model;
			model.PropertyChanged += OnPropertyChanged;

			_easeVariants = Enum
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

			FirstPoint = _model.Controls.xz;
			SecondPoint = _model.Controls.yw;
			EaseType = _model.Type;
			SelectedVariant = Array.IndexOf(_easeVariants, _model.EaseFunc);
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Ease.Controls))
			{
				FirstPoint = _model.Controls.xz;
				SecondPoint = _model.Controls.yw;
			}
			if (e.PropertyName == nameof(Ease.Type))
			{
				EaseType = _model.Type;
			}
			if (e.PropertyName != nameof(Ease.EaseFunc))
			{
				SelectedVariant = Array.IndexOf(_easeVariants, _model.EaseFunc);
			}
		}

		private void NotifyPropertyChanged([CallerMemberName] string property = "")
		{
			propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
		}
	}
}