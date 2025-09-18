namespace Rails.Editor
{
	public struct Offsets
	{
		public float Top { get; set; }
		public float Bottom { get; set; }
		public float Left { get; set; }
		public float Right { get; set; }


		public Offsets(float top, float bottom, float left, float right)
		{
			Top = top;
			Bottom = bottom;
			Left = left;
			Right = right;
		}
	}
}