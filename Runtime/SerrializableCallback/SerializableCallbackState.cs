using System;

namespace Rails.Runtime.Callback
{
	/// <summary>
	/// Controls the scope of UnityEvent callbacks.
	/// </summary>
	[Serializable]
	public enum SerializableCallbackState
	{
		/// <summary>
		/// Callback is not issued.
		/// </summary>
		Off,
		/// <summary>
		/// Callback is always issued.
		/// </summary>
		EditorAndRuntime,
		/// <summary>
		/// Callback is only issued in the Runtime and Editor playmode.
		/// </summary>
		RuntimeOnly,
	}
}