namespace Rails.Editor
{
	public struct TimePositionChangedEvent
	{
		public float TimePosition { get; }


		public TimePositionChangedEvent(float timePosition)
		{
			TimePosition = timePosition;
		}
	}

	public struct RecordIntChangedEvent
	{
		public int PreviousValue { get; }
		public int NextValue { get; }
		public string Key { get; }


		public RecordIntChangedEvent(string key, int previousValue, int nextValue)
		{
			PreviousValue = previousValue;
			NextValue = nextValue;
			Key = key;
		}
	}
}