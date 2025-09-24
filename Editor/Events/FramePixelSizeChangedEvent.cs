namespace Rails.Editor
{
	public class FramePixelSizeChangedEvent
	{
		public float FramePixelSize { get; }


		public FramePixelSizeChangedEvent(float framePixelSize)
		{
			FramePixelSize = framePixelSize;
		}
	}
}