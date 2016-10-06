
using System;
using System.IO;
using System.Collections.Generic;
using TagLib;
using System.Text.RegularExpressions;

// Sets the Id3 tag Disk Number to the disk number as indicated in the filename.
// Filename Format: {DiskNumber}-{TrackNumber}- {Title}
//
// Do not use this app if your files are not in this format. 
//
// Usage: FixDiskNumber.exe [file]...

namespace FixDiskNumber
{
	class MainClass
	{
		enum ExitCode : int
		{
			Success = 0,
			InvalidLogin = 1,
			InvalidFilename = 2,
			UnknownError = 10
		}

		private static ExitCode ChangeDiskNumber(string filename)
		{
			string name = new FileInfo (filename).Name;

			TagLib.File mp3 = TagLib.File.Create (filename);
			Tag tag = mp3.GetTag (TagTypes.Id3v2);

			string prefix = String.Format ("{0:D2}-{1:D2}- ", tag.Disc, tag.Track);
			if (!name.StartsWith (prefix)) {
				uint diskNo = uint.Parse(name.Substring (0, 2));

				Console.WriteLine ("\t{0}, {1}", prefix, name.Substring (0, 6));
				Console.WriteLine ("\t{0}", filename);
				Console.WriteLine ("Testing from {0} to {1}", diskNo, tag.Disc);

				if (tag.Disc == 0 && diskNo != 0) 
				{
					FileAttributes attr = System.IO.File.GetAttributes (filename);
					if ((FileAttributes.ReadOnly & attr) != 0) {
						Console.WriteLine ("Readonly file: {0}", filename);
					} else {
						Console.WriteLine ("Changing from {0} to {1}", tag.Disc, diskNo);
						mp3.Tag.Disc = diskNo;
						mp3.Save ();
					}
				}
			}
			return ExitCode.Success;
		}

		private static void pause(bool skip=true)
		{
			if (skip) {
				return;
			}
			Console.Write ("> ");
			Console.ReadKey ();
		}

		public static int Main (string[] args)
		{
			ExitCode status = ExitCode.Success;

			Console.WriteLine ("FixDiskNumber");

			if (args.Length == 0) {
				Console.WriteLine ("Usage: FixDiskNumber file");
				return (int)status;
			}

			foreach (string filename in args) {
				if (!System.IO.File.Exists (filename)) {
					Console.Error.WriteLine ("File does not exist: {0}", filename);
					continue;
				}
				try {
					status = ChangeDiskNumber(filename);
				} catch (Exception e) {
					Console.WriteLine (e.Message);
					status = ExitCode.UnknownError;
				}
			}

			return (int)status;
		}
	}
}
