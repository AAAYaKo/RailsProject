using System;
using System.Collections.Generic;
using System.Linq;

namespace Rails.Editor
{
	public static class EventBus
	{
		private static readonly Dictionary<Type, List<object>> bus = new();


		public static void Subscribe<T>(Action<T> callback)
		{
			Type type = typeof(T);
			if (bus.ContainsKey(type))
				bus[type].Add(callback);
			else
				bus.Add(type, new List<object> { callback });
		}

		public static void Unsubscribe<T>(Action<T> callback)
		{
			if (bus.ContainsKey(typeof(T)))
				bus[typeof(T)].Remove(callback);
		}

		public static void Publish<T>(in T eventT)
		{
			if (!bus.ContainsKey(typeof(T)))
				return;
			foreach (var call in bus[typeof(T)].Cast<Action<T>>())
				call?.Invoke(eventT);
		}
	}
}