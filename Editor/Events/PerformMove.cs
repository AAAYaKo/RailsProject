namespace Rails.Editor
{
	public readonly struct PerformMove
	{
		public bool NeedMoveKey { get; }
		public bool IsForward { get; }
		public MoveMode Mode { get; }


		public PerformMove(bool needMoveKey, bool isForward, MoveMode mode)
		{
			NeedMoveKey = needMoveKey;
			IsForward = isForward;
			Mode = mode;
		}

		public enum MoveMode { frame, frame10, key, startEnd }
	}
}