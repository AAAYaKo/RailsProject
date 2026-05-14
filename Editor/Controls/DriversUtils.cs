using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Rails.Editor.ViewModel;
using UnityEditor;
using UnityEngine;

namespace Rails.Editor.Drivers
{
	public static class DriversUtils
	{
		private static Dictionary<AppDomain, List<Type>> cachedDomainTypes = new();


		public static Type ExtractTypeFromString(string typeName)
		{
			if (typeName.IsNullOrEmpty())
				return null;

			var splitFieldTypeName = typeName.Split(' ');
			var assemblyName = splitFieldTypeName[0];
			assemblyName = assemblyName == "Assembly" ? "Assembly-CSharp" : assemblyName;
			var subStringTypeName = splitFieldTypeName[1];
			if (splitFieldTypeName.Length > 2)
				subStringTypeName = typeName.Substring(assemblyName.Length + 1);

			var assembly = Assembly.Load(assemblyName);
			var targetType = assembly.GetType(subStringTypeName);
			return targetType;
		}

		public static List<Type> GetAssignableTypes(Type propertyType)
		{
			var derivedTypes = TypeCache.GetTypesDerivedFrom(propertyType);
			var nonUnityTypes = derivedTypes.Where(IsAssignableNonUnityType).OrderBy(x => x.Name).ToList();

			return nonUnityTypes;

			static bool IsAssignableNonUnityType(Type type)
			{
				return IsFinalAssignableType(type) && !type.IsSubclassOf(typeof(UnityEngine.Object));
			}
		}

		public static object CreateObjectFromType(Type type)
		{
			object newObject;
			if (type?.GetConstructor(Type.EmptyTypes) != null)
			{
				newObject = Activator.CreateInstance(type);
			}
			else
			{
				newObject = type != null ? FormatterServices.GetUninitializedObject(type) : null;
			}

			return newObject;
		}

		private static bool IsFinalAssignableType(Type type)
		{
			return type.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface;
		}

		private static IEnumerable<Type> GetAllTypesInCurrentDomain()
		{
			var currentDomain = AppDomain.CurrentDomain;
			if (cachedDomainTypes.TryGetValue(currentDomain, out var cachedTypes))
				return cachedTypes;

			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			var types = new List<Type>();
			foreach (var assembly in assemblies)
			{
				try
				{
					types.AddRange(assembly.GetTypes());
				}
				catch (Exception e)
				{
					Debug.LogError(e);
				}
			}

			cachedDomainTypes.Add(currentDomain, types);

			return types;
		}
	}
}