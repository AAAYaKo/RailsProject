namespace Rails.Editor
{
	public struct FramePixelSizeChangedEvent
	{
		public float FramePixelSize { get; }


		public FramePixelSizeChangedEvent(float framePixelSize)
		{
			FramePixelSize = framePixelSize;
		}
	}
}