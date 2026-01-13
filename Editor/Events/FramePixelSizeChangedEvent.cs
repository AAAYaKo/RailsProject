namespace Rails.Editor
{
	public readonly struct FramePixelSizeChangedEvent
	{
		public float FramePixelSize { get; }


		public FramePixelSizeChangedEvent(float framePixelSize)
		{
			FramePixelSize = framePixelSize;
		}
	}
}