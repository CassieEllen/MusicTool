﻿using System;

namespace MusicTool.Model.Domain
{
	public class MusicFile : IDisposable
	{
		public virtual int Id { get; set; }
		public virtual string Path { get; set; }
		public virtual string Base { get; set; }
		public virtual string Artist { get; set; }
		public virtual string Album { get; set; }
		public virtual string Title { get; set; }
		public virtual long Size { get; set; }
		public virtual string Md5sum { get; set; }

		public MusicFile()
		{
		}

		public MusicFile(MusicFile info)
		{
			this.Id = info.Id;
			this.Path = info.Path;
			this.Base = info.Base;
			this.Artist = info.Artist;
			this.Album = info.Album;
			this.Title = info.Title;
			this.Size = info.Size;
			this.Md5sum = info.Md5sum;
		}

		public MusicFile(MusicInfo info)
		{
			Path = info.Path;
			Base = info.Base;
			Artist = info.Artist;
			Album = info.Album;
			Title = info.Title;
			Size = info.Size;
			Md5sum = info.Md5sum;
		}

		public override string ToString ()
		{
			//return string.Format ("[MusicInfo: Base={0}, Artist={1}, Album={2}, Song={3}, Size={4}]", 
			//	Base, Artist, Album, Song, Size);
			return string.Format ("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\"",  Path, Artist, Album, Title, Size);
		}

		public void Dispose() {
			//Id = null;
			Path = null;
			Base = null;
			Artist = null;
			Title = null;
			//Size = null;
			Md5sum = null;
		}

	}
}

