using System;
using System.IO;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Linq;

using MusicMerge.Model.Domain;

namespace MusicTool.Utils
{
	/// <summary>
	/// Searches down from a base music directory, adding, rejecting, or ignoring files.
	/// </summary>
	public class MusicSearch
	{
		private static readonly log4net.ILog log =
			log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		// Define some assumptions on file extensions. 
		private string[] exts = new string[] {".mp3"};
		private string[] skip = new string[] {".db", ".pdf", ".jpg", ".ini"};
		private string[] secure = new string[] { ".m4p" };

		/// <summary>
		/// The music collection.
		/// </summary>
		private IMusicCollection musicCollection;
		public IMusicCollection MusicCollection {
			get {
				return musicCollection;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MusicMerge.MusicSearch"/> class.
		/// </summary>
		/// <param name="musicCollection">Music collection.</param>
		public MusicSearch (IMusicCollection musicCollection)
		{
			this.musicCollection = musicCollection;
		}

		/// <summary>
		/// Drills down into the Band directories from the base directory. 
		/// </summary>
		/// <param name="baseDirectory">The base directory to start from.</param>
		public void DrillDown(DirectoryInfo baseDirectory) 
		{
			MusicInfo info = new MusicInfo ();
			info.Base = baseDirectory.FullName;
			log.Debug (info.Path);
			DoBands (info, baseDirectory);
		}

		/// <summary>
		/// Drills down into the Band directories from a base directory.
		/// </summary>
		/// <param name="mi">MusicInfo containing upper level information.</param>
		/// <param name="di">BaseDirectoryInfo.</param>
		private void DoBands(MusicInfo mi, DirectoryInfo di)
		{
			IEnumerable<DirectoryInfo> dirs = di.EnumerateDirectories();
			foreach (var dir in dirs) {
				if(dir.Name.StartsWith(".") ) {
					throw new ArgumentException ("dot Author");
				}
				MusicInfo info = new MusicInfo (mi);
				info.Artist = dir.Name;
				log.Debug (info.Path);
				DoAlbums (info, dir);
			}

		}

		/// <summary>
		/// Drills down into the Album directories from a Band directory.
		/// </summary>
		/// <param name="mi">MusicInfo containing upper level information.</param>
		/// <param name="di">Band DirectoryInfo.</param>
		public void DoAlbums (MusicInfo mi, DirectoryInfo di)
		{
			IEnumerable<DirectoryInfo> dirs = di.EnumerateDirectories ();
			foreach (var dir in dirs) {
				if(dir.Name.StartsWith(".") ) {
					throw new ArgumentException ("dot Album");
				}
				MusicInfo info = new MusicInfo (mi);
				info.Album = dir.Name;
				log.Debug (info.Path);
				DoSongs (info, dir);
			}
		}

		/// <summary>
		/// Drills down into the song files from the Album directory.
		/// </summary>
		/// <param name="mi">MusicInfo containing upper level information.</param>
		/// <param name="di">Album DirectoryInfo.</param>
		public void DoSongs(MusicInfo mi, DirectoryInfo di)
		{
			IEnumerable<FileInfo> files = di.EnumerateFiles ();
			foreach (var file in files) {

				MusicInfo info = new MusicInfo (mi, file);
				log.Debug (info.Path);

				if (file.Name.StartsWith(".")) { // Hidden
					RejectEntry ("H", info);
					continue;
				}
				if (secure.Contains(file.Extension)) { // Secure - so ignore
					IgnoreEntry("S", info);
					continue;
				}
				if (skip.Contains (file.Extension)) { // Ignore
					IgnoreEntry ("I", info);
					continue;
				}
				if (!exts.Contains (file.Extension)) { // Extension Unknown
					RejectEntry("E", info);
					continue;
				}
					
				AddEntry (info);
			}
		}
			
		/// <summary>
		/// Adds the entry into the collection if it matches the required format.
		/// Othewise, adds the entry into the reject collection.
		/// </summary>
		/// <param name="info">Info.</param>
		private void AddEntry(MusicInfo info)
		{	
			if (Id3Tools.IsNameMatch(info.Title)) {
				musicCollection.Add (info);
			} else {
				RejectEntry ("P", info);
			}
		}

		/// <summary>
		/// Adds the entry into the reject collection.
		/// </summary>
		/// <param name="reason">Reason.</param>
		/// <param name="info">Info.</param>
		private void RejectEntry (string reason, MusicInfo info)
		{
			info.Reason = reason;
			log.DebugFormat (" {0}  - {1}", info.Reason, info.Path);
			musicCollection.Reject (info);
		}

		/// <summary>
		/// Adds the entry into the ignore collection.
		/// </summary>
		/// <param name="reason">Reason.</param>
		/// <param name="info">Info.</param>
		private void IgnoreEntry (string reason, MusicInfo info)
		{
			info.Reason = reason;
			log.DebugFormat (" {0}  - {1}", info.Reason, info.Path);
			musicCollection.Ignore (info);
		}

	}
}

