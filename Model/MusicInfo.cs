using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace MusicTool.Model
{
	public class MusicInfo
	{
		public string Base { get; set; }

		public string Artist { get; set; } = "";

		public string Album { get; set; } = "";

		public string Title { get; set; } = "";

		public long Size { get; set; }

		public string Md5sum { get; set; }

		public string Reason { get; set; }

		public FileInfo Info { get; set; }

		public string Key {
			get {
				char ps = System.IO.Path.PathSeparator;
				return Artist + ps +  Album + ps + Title;
			}
		}

		public string Path {
			get {
				string[] parts = new string[] { Base, Artist, Album, Title };
				string p = System.IO.Path.Combine (parts);
				return System.IO.Path.GetFullPath (p);
			}
		}

		public string FixedTitle {
			get {
				string s = System.IO.Path.GetFileNameWithoutExtension (Title);
				Console.WriteLine ("s: {0}", s);
				var regex = new Regex (@"[0-9]+-[0-9]+- ");
				Match match = regex.Match (s);
				if (!match.Success) {
					return s;
				}
				return s.Substring (match.Length);
			}
		}

		public MusicInfo()
		{
		}

		public MusicInfo(MusicInfo info, FileInfo fileInfo = null)
		{
			Base = info.Base;
			Artist = info.Artist;
			Album = info.Album;
			Title = info.Title;
			Size = info.Size;
			Md5sum = info.Md5sum;
			Reason = info.Reason;
			Info = fileInfo;
			if(Info != null) {
				Title = Info.Name;
				Size = Info.Length;
			}
		}
			
		public static string ByteArrayToString(byte[] ba)
		{
			StringBuilder hex = new StringBuilder(ba.Length * 2);
			foreach (byte b in ba)
				hex.AppendFormat("{0:x2}", b);
			return hex.ToString();
		}

		public static string calcMd5sum(FileInfo fi)
		{
			using (var md5 = MD5.Create())
			{
				using (var stream = fi.OpenRead() ) // File.OpenRead(filename))
				{
					return ByteArrayToString (md5.ComputeHash (stream));
				}
			}
		}

		public string toJSON()
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject (this);
		}

		public override string ToString ()
		{
			//return string.Format ("[MusicInfo: Base={0}, Artist={1}, Album={2}, Song={3}, Size={4}]", 
			//	Base, Artist, Album, Song, Size);
			return string.Format ("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",{5}", Md5sum, Base, Artist, Album, Title, Size);
		}

		public override bool Equals (object obj)
		{
			return base.Equals (obj);
		}

		public override int GetHashCode ()
		{
			string s = Artist + Album + Title;
			return s.GetHashCode();
		}
	}
}

