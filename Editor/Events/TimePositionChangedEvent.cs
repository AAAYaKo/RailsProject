namespace Rails.Editor
{
	public class TimePositionChangedEvent
	{
		public float TimePosition { get; }


		public TimePositionChangedEvent(float timePosition)
		{
			TimePosition = timePosition;
		}
	}
}