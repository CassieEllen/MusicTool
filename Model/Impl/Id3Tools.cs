using System;
using System.IO;
using System.Text.RegularExpressions;
using TagLib;


namespace MusicMerge.Utils
{
	public class Id3Tools
	{
		private static readonly log4net.ILog log =
			log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private System.IO.FileInfo fileInfo;
		private Tag tag;

		public static Regex positionPattern = new Regex(@"(\d{2}-\d{2}- )|(\d{2} )");
		public static Regex missingTrackPattern = new Regex(@"\d{2}-- ");
		public static Regex partialTrackPattern = new Regex (@"\d{2} ");

		public Id3Tools (System.IO.FileInfo info)
		{
			fileInfo = info;
			using (TagLib.File mp3 = TagLib.File.Create (fileInfo.FullName)) {
				tag = mp3.GetTag (TagTypes.Id3v2);
			}

		}

		public bool NameMatch {
			get {
				bool result = true == positionPattern.IsMatch (fileInfo.FullName);
				return result;
			}
		}

		public static bool IsNameMatch(string name)
		{
			return true == positionPattern.IsMatch (name);
		}

		public static bool IsMissingTrack(string name)
		{
			return true == missingTrackPattern.IsMatch (name);
		}

		public bool IsMissingTrack()
		{
			return true == missingTrackPattern.IsMatch (fileInfo.FullName);
		}

		public static bool IsPartialTrack(string name)
		{
			return true == partialTrackPattern.IsMatch (name);
		}

		public bool IsPartialTrack()
		{
			return true == partialTrackPattern.IsMatch (fileInfo.FullName);
		}

		public uint[] GetDiskTrack()
		{
			uint[] ary = new uint[2] { 0, 0 };
			try {
				ary[0] = tag.Disc;
				ary[1] = tag.Track;
			} catch(DirectoryNotFoundException e) {
				Console.Error.WriteLine (e.Message);
				Console.Error.WriteLine (e.StackTrace);
			}
			return ary;
		}

		public string GetPrefix()
		{
			return String.Format ("{0:D2}-{1:D2}- ", tag.Disc, tag.Track);
		}

		public bool ContansDiskAndTrack()
		{
			uint[] disktrack = GetDiskTrack ();
			if (disktrack [0] == 0 || disktrack [1] == 0) {
				log.Warn ("The tag does not contain required track and disk numbers.");
				return false;
			}
			return true;
		}

		public bool RenameTo(Regex pat) {
			if (!ContansDiskAndTrack ()) {
				return false;
			}

			string prefix = GetPrefix ();
			Console.WriteLine ("{0} {1}", prefix, fileInfo.Name);

			string newName = pat.Replace (fileInfo.Name, prefix);
			string newPath = string.Format ("{0}{1}{2}", fileInfo.DirectoryName, 
				System.IO.Path.DirectorySeparatorChar, newName);

			try {
				fileInfo.MoveTo (newPath);
			} catch (UnauthorizedAccessException e) {
				log.WarnFormat ("{0}: {1}", e.Message, newPath);
				return false;
			}
			return true;
		}

		public void FixDiskAndTrack()
		{
			uint disk = tag.Disc;
			uint diskCount = tag.DiscCount;
			uint track = tag.Track;
			uint trackCount = tag.TrackCount;

			//diskCount = 1;
			//trackCount = 11;

			Console.WriteLine ("{0}/{1} {2}/{3} {4}", disk, diskCount, track, trackCount, fileInfo.Name);

			using (TagLib.File mp3 = TagLib.File.Create (fileInfo.FullName)) {
				//tag = mp3.GetTag (TagTypes.Id3v2);
				tag.Disc = disk;
				tag.DiscCount = diskCount;
				tag.Track = track;
				tag.TrackCount = trackCount;
				//mp3.Save();
			}
		}

	}
}

