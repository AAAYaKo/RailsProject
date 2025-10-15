namespace Rails.Editor.ViewModel
{
	public readonly struct ReorderArgs
	{
		public int OldIndex { get; }
		public int NewIndex { get; }


		public ReorderArgs(int oldIndex, int newIndex)
		{
			OldIndex = oldIndex;
			NewIndex = newIndex;
		}
	}
}