using System;
using System.Collections.Generic;
using TagLib;
using System.Text.RegularExpressions;

namespace MusicMerge
{
	public class FixDiskNumber : IMusicCollection
	{
		private static readonly log4net.ILog log =
			log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		public FixDiskNumber ()
		{
		}

		public void Add(MusicInfo info)
		{
		}

		public void Reject(MusicInfo info)
		{
			string filename = System.IO.Path.GetFileName (info.Info.FullName);
			Regex pat = new Regex (@"^\d{1,2}-\d{2}- ");
			if (!pat.IsMatch (filename)) {
				log.WarnFormat ("Invalid filename: {0}", filename);
				return;
			}
				
			using( TagLib.File mp3 = TagLib.File.Create (info.Info.FullName) )
			{
				
				Tag tag = mp3.GetTag (TagTypes.Id3v2);
				Console.WriteLine (tag.ToString ());
			}

			#if false
				string prefix = String.Format ("{0:D2}-{1:D2}- ", tag.Disc, tag.Track);
				if (!filename.StartsWith (prefix)) {
					Console.WriteLine ("\t{0}, {1}", prefix, filename.Substring (0, 6));
					Console.WriteLine ("\t{0}", info.Info.FullName);
					uint diskNo = Convert.ToUInt32 (filename.Substring (0, 2));
					Console.WriteLine ("Testing from {0} to {1}", diskNo, tag.Disc);
					if (tag.Disc == 0 && diskNo != 0) {
						if (info.Info.IsReadOnly) {
							Console.WriteLine ("Readonly file: {0}", info.Info.FullName);
						} else {
							Console.WriteLine ("Changing from {0} to {1}", tag.Disc, diskNo);
							mp3.Tag.Disc = diskNo;
							mp3.Save ();
						}
					}
				}
			#endif
		}

		public void Ignore ( MusicInfo info )
		{
			throw new NotImplementedException ();
		}


	}
}

