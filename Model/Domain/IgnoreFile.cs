using System;

namespace MusicTool.Model.Domain
{
	public class IgnoreFile : IDisposable
	{
		public virtual int Id { get; set; }
		public virtual string Path { get; set; }
		public virtual string Reason { get; set; }

		public IgnoreFile ()
		{
		}

		public IgnoreFile(MusicInfo info)
		{
			Path = info.Path;
			Reason = info.Reason;
		}

		public void Dispose() 
		{
			Path = null;
			Reason = null;
		}
	}
}

