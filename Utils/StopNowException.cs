using System;
using System.Runtime.Serialization;

// https://msdn.microsoft.com/en-us/library/system.exception(v=vs.110).aspx

namespace MusicTool.Utils
{
	/// <summary>
	/// Stop now exception.
	/// Used to stop processing, and is not a real error. 
	/// </summary>
	[Serializable()]
	public class StopNowException : SystemException
	{
		protected StopNowException()
			: base()
		{
		}

		public StopNowException (string message)
			: base(message)
		{
		}

		public StopNowException(string message, Exception e)
			: base(message, e)
		{
		}
	}
}

