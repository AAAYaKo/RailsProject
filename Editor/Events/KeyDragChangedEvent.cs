namespace Rails.Editor
{
	public readonly struct KeyDragChangedEvent
	{
		public int DragFrames { get; }


		public KeyDragChangedEvent(int dragFrames)
		{
			DragFrames = dragFrames;
		}
	}
}