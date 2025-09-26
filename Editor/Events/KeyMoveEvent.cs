namespace Rails.Editor
{
	public struct KeyMoveEvent
	{
		public int DeltaFrames { get; }


		public KeyMoveEvent(int deltaFrames)
		{
			DeltaFrames = deltaFrames;
		}
	}
}