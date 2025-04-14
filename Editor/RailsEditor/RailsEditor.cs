using System;
using System.Runtime.CompilerServices;
using Rails.Editor.ViewModel;
using Rails.Runtime;
using Unity.Properties;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rails.Editor
{
	public class RailsEditor : EditorWindow
	{
		[SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

		[MenuItem("Window/UI Toolkit/RailsEditor")]
		public static void ShowExample()
		{
			RailsEditor wnd = GetWindow<RailsEditor>();
			wnd.titleContent = new GUIContent("RailsEditor");
		}

		public void CreateGUI()
		{
			// Each editor window contains a root VisualElement object
			VisualElement root = rootVisualElement;

			// Instantiate UXML
			VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
			root.Add(labelFromUXML);

			EaseTest model = Resources.Load<EaseTest>("New Ease Test");
			EditorContext.Instance.CurrentTarget = model;

			TestViewModel m = new();
			m.Ease = new EaseViewModel(model.Ease);

			root.dataSource = m;
		}
	}

	public class TestViewModel : INotifyBindablePropertyChanged
	{
		[CreateProperty]
		public EaseViewModel Ease
		{
			get => ease;
			set
			{
				if (ease != value)
				{
					if (ease != null)
						ease.propertyChanged -= OnEasePropertyChanged;
					ease = value;
					ease.propertyChanged += OnEasePropertyChanged;
					NotifyPropertyChanged();
				}
			}
		}

		public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

		private EaseViewModel ease;


		private void NotifyPropertyChanged([CallerMemberName] string property = "")
		{
			propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
		}

		private void OnEasePropertyChanged(object sender, BindablePropertyChangedEventArgs e)
		{
			NotifyPropertyChanged(nameof(Ease));
		}
	}

	public class EditorContext
	{
		public static EditorContext Instance => _instance ??= new();
		public UnityEngine.Object CurrentTarget { get; set; }
		private static EditorContext _instance;


		private EditorContext()
		{
			RegisterConverters();
		}

		public void Record(string undoRecordName)
		{
			Undo.RecordObject(CurrentTarget, undoRecordName);
		}

		private void RegisterConverters()
		{
			ConverterGroups.RegisterGlobalConverter((ref ToggleButtonGroupState x) =>
			{
				for (int i = 0; i < x.length; i++)
				{
					if (x[i])
						return (Ease.EaseType)i;
				}
				return Ease.EaseType.NoAnimation;
			});
			ConverterGroups.RegisterGlobalConverter((ref Ease.EaseType x) =>
			{
				int length = Enum.GetNames(typeof(Ease.EaseType)).Length;
				ToggleButtonGroupState result = new(0, length);
				result.ResetAllOptions();
				result[(int)x] = true;
				return result;
			});
		}
	}
}