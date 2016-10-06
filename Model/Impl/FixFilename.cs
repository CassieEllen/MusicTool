using System;
using System.IO;
using System.Text.RegularExpressions;

using MusicTool.Model.Domain;
using MusicTool.Model.Interfaces;
using MusicTool.Model.Impl;

namespace MusicTool.Model.Impl
{
	public class FixFilename : IMusicCollection
	{
		private static readonly log4net.ILog log =
		log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public FixFilename ()
		{
		}
	
		public void Add(MusicInfo info) 
		{

			Id3Tools id3 = new Id3Tools (info.Info);
			id3.FixDiskAndTrack ();

			if (id3.NameMatch) {
				return;
			}

			// If name == "dd-- ", then fix it with "dd-tt- "
			if ( ! info.Path.Equals (info.Info.FullName)) {
				throw new ArgumentException ("info.Path not equal to info.Info.FullName");
			}
			if (Id3Tools.IsMissingTrack (info.Path)) {
				//Id3Tools id3 = new Id3Tools (info.Info);
				try {
					id3.RenameTo (Id3Tools.missingTrackPattern);
				} catch (UnauthorizedAccessException e) {
					log.WarnFormat ("{0}", e.Message);
				}

			} else if (id3.IsPartialTrack()) {
				try {
					id3.RenameTo (Id3Tools.partialTrackPattern);
				} catch (UnauthorizedAccessException e) {
					log.WarnFormat ("{0}", e.Message);
				}

			}
		}

		public void Reject(MusicInfo info) {
			throw new NotImplementedException ();
		}

		public void Ignore(MusicInfo info) {
			throw new NotImplementedException ();
		}

	}
}

