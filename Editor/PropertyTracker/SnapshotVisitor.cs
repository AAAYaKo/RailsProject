using System.Collections;
using System.Text;
using Rails.Editor.Context;
using Unity.Properties;
using UnityEngine;

namespace Rails.Editor.Property
{
	internal class SnapshotVisitor : PropertyVisitor
	{
		private static readonly StringBuilder builder = new(32);

		protected override void VisitProperty<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
		{
			base.VisitProperty(property, ref container, ref value);
			if (value is not ICollection)
			{
				if (PropertyStorage<TContainer, TValue>.CheckValueChanged(new SnapshotPropertyKey<TContainer>(container, property.Name), value))
				{
					builder.Clear();
					builder
						.Append('[')
						.Append(container)
						.Append("] ")
						.Append(property.Name)
						.Append(": ")
						.Append(value);
					Debug.Log(builder.ToString());
					EventBus.Publish(new PropertyChanged(container, property.Name));
				}
			}
		}

		protected override void VisitCollection<TContainer, TCollection, TElement>(Property<TContainer, TCollection> property, ref TContainer container, ref TCollection value)
		{
			base.VisitCollection<TContainer, TCollection, TElement>(property, ref container, ref value);
			if (PropertyStorage<TContainer, TCollection, TElement>.CheckValueChanged(new SnapshotPropertyKey<TContainer>(container, property.Name), value))
			{
				builder.Clear();
				builder
					.Append('[')
					.Append(container)
					.Append("] ")
					.Append(property.Name)
					.Append(": ");

				int i = 0;
				int count = value.Count;
				foreach (var element in value)
				{
					i++;
					builder.Append(element);
					if (i < count)
						builder.Append(", ");
				}

				Debug.Log(builder.ToString());
				EventBus.Publish(new PropertyChanged(container, property.Name));
			}
		}
	}
}