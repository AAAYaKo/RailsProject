namespace Rails.Editor
{
	public class KeyMoveEvent
	{
		public int DeltaFrames { get; }


		public KeyMoveEvent(int deltaFrames)
		{
			DeltaFrames = deltaFrames;
		}
	}
}