namespace Rails.Editor
{
	public readonly struct KeyMoveEvent
	{
		public int DeltaFrames { get; }


		public KeyMoveEvent(int deltaFrames)
		{
			DeltaFrames = deltaFrames;
		}
	}
}