namespace Rails.Editor
{
	public readonly struct KeyDragEvent
	{
		public int DragFrames { get; }


		public KeyDragEvent(int dragFrames)
		{
			DragFrames = dragFrames;
		}
	}
}