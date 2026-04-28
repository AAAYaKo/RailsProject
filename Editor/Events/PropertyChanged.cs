namespace Rails.Editor
{
	public readonly struct PropertyChanged
	{
 		public object Sender { get; }
		public string PropertyName { get; }

		
		public PropertyChanged(object model, string name)
		{
			Sender = model;
			PropertyName = name;
		}
	}
}