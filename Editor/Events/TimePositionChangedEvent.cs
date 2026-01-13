namespace Rails.Editor
{
	public readonly struct TimePositionChangedEvent
	{
		public float TimePosition { get; }


		public TimePositionChangedEvent(float timePosition)
		{
			TimePosition = timePosition;
		}
	}
}