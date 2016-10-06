using System;
using System.Configuration;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using NHibernate;
using NHibernate.Linq;

using MusicMerge.Domain;
using TagLib;

namespace MusicMerge
{
	public class DbMusicCollection : IMusicCollection
	{
		private static readonly log4net.ILog log =
		log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		public bool limitSearch = false;
		public int searchCount = 0;
		public int stopCount = 3;
		public bool normalizeName;

		ISessionFactory factory;

		public DbMusicCollection (ISessionFactory factory)
		{
			this.factory = factory;
			string ls = ConfigurationManager.AppSettings ["limitsearch"];
			limitSearch = Convert.ToBoolean (ls);
			string lc = ConfigurationManager.AppSettings ["stopcount"];
			stopCount = Convert.ToInt32 (lc);
			string nn = ConfigurationManager.AppSettings ["limitsearch"];
			normalizeName = Convert.ToBoolean (nn);
		}

		private bool IgnoreFileExists (MusicInfo info)
		{			
			try {
				using (ISession session = factory.OpenSession ())
				using (ITransaction tx = session.BeginTransaction ()) {
					var list =
						from u in session.Query<IgnoreFile> ()
							where u.Path == info.Path
						select u;

					bool result = list.Count() > 0;
					return result;
				}
			} catch (ArgumentOutOfRangeException e) {
				Console.Error.WriteLine (e.StackTrace);
				log.ErrorFormat ("{0}", info.Path);
				throw new StopNowException (e.Message, e);
			}
		}

		private bool RejectFileExists (MusicInfo info)
		{			
			try {
				using (ISession session = factory.OpenSession ())
				using (ITransaction tx = session.BeginTransaction ()) {
					var list =
						from u in session.Query<RejectFile> ()
						where u.Path == info.Path
						select u;

					bool result = list.Count() > 0;
					return result;
				}
			} catch (ArgumentOutOfRangeException e) {
				Console.Error.WriteLine (e.StackTrace);
				log.ErrorFormat ("{0}", info.Path);
				throw new StopNowException (e.Message, e);
			}
		}

		private bool EntryExists (MusicInfo info)
		{	
			try {
				using (ISession session = factory.OpenSession ())
				using (ITransaction tx = session.BeginTransaction ()) {
					var list =
						from u in session.Query<MusicFile> ()
						where u.Path == info.Path
						select u;

					bool result = list.Count() > 0;
					return result;
				}
			} catch (ArgumentOutOfRangeException e) {
				Console.Error.WriteLine (e.StackTrace);
				log.ErrorFormat ("{0}", info.Path);
				throw new StopNowException (e.Message, e);
			}
		}

		private TagLib.Tag GetTag(MusicInfo info) {
			TagLib.Tag tag;
			using (TagLib.File mp3 = TagLib.File.Create (info.Info.FullName)) {
				tag = mp3.GetTag (TagTypes.Id3v2);
			}
			return tag;
		}

		/// <summary>
		/// Adds info to the database table files if it does not already exist.
		/// </summary>
		/// <param name="info">Info</param>
		public void Add(MusicInfo info) {

			Id3Tools id3 = new Id3Tools (info.Info);
			if (id3.NameMatch) {
				if (EntryExists (info)) {
					return;
				}

				if (limitSearch && ++searchCount > stopCount) {
					throw new StopNowException ("stop count reached");
				}

				// Only calculate the Md5Sum if the entry does not already exist. 
				info.Md5sum = MusicInfo.calcMd5sum (info.Info);

				// Add info to the database
				log.DebugFormat ("A   - {0}", info.Path);
				SaveMusicFile(info);
				return;
			}

			return;
		}

		public void Reject (MusicInfo info) {
			if (!RejectFileExists (info)) {
				SaveRejectFile (info);
			}
		}

		public void Ignore (MusicInfo info) {
			if (!IgnoreFileExists (info)) {
				SaveIgnoreFile (info);
			}
		}

		private void SaveMusicFile(MusicInfo info) {
			info.Reason = "A";
			log.InfoFormat ("{0} {1}", info.Reason, info.Path);
			try {
				using(MusicMerge.Domain.MusicFile file = new MusicFile (info))
				using (ISession session = factory.OpenSession ())
				using (ITransaction tx = session.BeginTransaction ()) 
				{
					session.Save (file);
					session.Flush ();
					tx.Commit ();
				}
			} catch(HibernateException e) {
				log.Error (e);
			}

		}

		private void SaveRejectFile(MusicInfo info) {
			log.InfoFormat ("{0} {1}", info.Reason, info.Path);
			try {
				using(MusicMerge.Domain.RejectFile file = new RejectFile (info))
				using (ISession session = factory.OpenSession ())
				using (ITransaction tx = session.BeginTransaction ()) 
				{
					session.Save (file);
					session.Flush ();
					tx.Commit ();
				}
			} catch(HibernateException e) {
				log.Error (e);
			}

		}

		private void SaveIgnoreFile(MusicInfo info) {
			log.InfoFormat ("{0} {1}", info.Reason, info.Path);
			try {
				using(MusicMerge.Domain.IgnoreFile file = new IgnoreFile (info))
				using (ISession session = factory.OpenSession ())
				using (ITransaction tx = session.BeginTransaction ()) 
				{
					session.Save (file);
					session.Flush ();
					tx.Commit ();
				}
			} catch(HibernateException e) {
				log.Error (e);
				throw;
			}

		}

	}
}

