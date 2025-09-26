namespace Rails.Editor
{
	public struct KeyDragEvent
	{
		public int DragFrames { get; }


		public KeyDragEvent(int dragFrames)
		{
			DragFrames = dragFrames;
		}
	}
}