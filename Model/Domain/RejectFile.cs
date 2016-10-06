using System;
using MusicTool.Model;

namespace MusicTool.Model.Domain
{
	public class RejectFile : IDisposable
	{
		public virtual int Id { get; set; }
		//public virtual Guid Id { get; set; }
		public virtual string Path { get; set; }
		public virtual string Artist { get; set; }
		public virtual string Album { get; set; }
		public virtual string Title { get; set; }
		public virtual long Size { get; set; }
		public virtual string Reason { get; set; }

		public RejectFile()
		{
		}

		public RejectFile(MusicInfo info)
		{
			Path = info.Path;
			Artist = info.Artist;
			Album = info.Album;
			Title = info.Title;
			Size = info.Size;
			Reason = info.Reason;
		}

		public override string ToString ()
		{
			//return string.Format ("[MusicInfo: Base={0}, Artist={1}, Album={2}, Song={3}, Size={4}]", 
			//	Base, Artist, Album, Song, Size);
			return string.Format ("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",{5}", Reason, Path, Artist, Album, Title, Size);
		}

		public void Dispose() {
			//Id = null;
			Path = null;
			Artist = null;
			Title = null;
			//Size = null;
			Reason = null;
		}

	}
}

