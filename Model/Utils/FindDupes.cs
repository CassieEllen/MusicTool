using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NHibernate;
using NHibernate.Linq;
using MusicTool.Model.Domain;
using MusicTool.Options;

namespace MusicTool.Model
{


	public class FindDupes
	{
		private static readonly log4net.ILog log =
			log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		ISessionFactory factory;
		Options.ConfigOptions options;
		string merge = String.Empty;
		bool verbose = false;

		Dictionary<string, MusicFile> allEntries = new Dictionary<string, MusicFile>();

		public FindDupes (Options.ConfigOptions options, ISessionFactory factory)
		{
			this.factory = factory;
			merge = options.Merge;
			verbose = options.Verbose > 1;
		}

		private List<MusicFile> GetAllEntries()
		{
			List<MusicFile> entries = new List<MusicFile>();

			try {
				using (ISession session = factory.OpenSession ())
				using (ITransaction tx = session.BeginTransaction ()) 
				{
					#if false
					var allClassMetadata = session.SessionFactory.GetAllClassMetadata();
					foreach (var entry in allClassMetadata)
					{
						Console.WriteLine (entry.ToString() );
						session.CreateCriteria((Type) entry.Key.GetType())
							.SetMaxResults(0).List();
					}
					#endif
					entries = session.Query<MusicFile>()
						.OrderBy (x => x.Album)
						.ThenBy(y => y.Title)
						.ToList();
					tx.Commit ();
				}
			} catch(HibernateException e) {
				log.Error (e);
			}
			return entries;
		}

		private void display(IList<MusicFile> list)
		{
			if(null == list) {
				Console.WriteLine("empty");
			} else {
				foreach (var f in list) {
					Console.WriteLine ("{0} {1} {2}", f.Id, f.Album, f.Title);
				}
			}
			Console.WriteLine ();
		}

		public void CopySong(string dir, MusicFile musicFile) {
			string source = musicFile.Path;
			string[] parts = { dir, musicFile.Artist, musicFile.Album, musicFile.Title };
			string separator = Path.DirectorySeparatorChar.ToString();
			string target = string.Join (separator, parts);
			//Console.WriteLine ("Copy {0} to {1}", source, target);
			FileInfo targetFile = new FileInfo (target);
			if ( ! targetFile.Directory.Exists ) {
				System.IO.Directory.CreateDirectory (targetFile.DirectoryName);
			}
			try {
			System.IO.File.Copy (source, target, false);
			} catch(Exception e) {
				log.Error(e.StackTrace);
			}
		}

		public string GetName(MusicFile f) 
		{
			string[] parts = new string[] { f.Artist, f.Album, f.Size.ToString() };
			return string.Concat (parts);
		}


		public void Run()
		{		
			List<MusicFile> entries = GetAllEntries ();
			if (verbose) {
				display (entries);
			}
			List<MusicFile> mergeList = new List<MusicFile> ();
				
			int rowCount = 0;
			using (ISession session = factory.OpenSession ())
			using (ITransaction tx = session.BeginTransaction ()) 
			{
				bool hasEntries = entries.Exists(x => { return x.Base == merge; } );
				if (true == hasEntries) {
					mergeList = entries.FindAll( x => { return x.Base == merge; } );
					rowCount = mergeList.Count;
				}

				tx.Commit ();
			}

			log.InfoFormat ("{0} rows in {1}", rowCount, "oops");

			int copyCount = 0;
			int skipCount = 0;
			foreach (var entry in entries) {
				MusicFile targetEntry = new MusicFile (entry);
				targetEntry.Base = merge;

				bool found = mergeList.Exists (e => {return GetName(e) == GetName(targetEntry);});
				if ( found ) {
					if (entry.Size != targetEntry.Size) {
						log.ErrorFormat ("SizeMismatch {0} - (1)", entry.Size, targetEntry.Size);
					}
					MusicFile mergeEntry = mergeList.Find (e => {return GetName(e) == GetName(targetEntry); });
					if (verbose) {
						Console.WriteLine ("m {0}", mergeEntry.Path);
						Console.WriteLine ("e {0}", entry.Path);
					}
					++skipCount;
				} else {
					if (verbose) {
						Console.WriteLine ("c {0}", entry.Path);
					}
					mergeList.Add (targetEntry);
					CopySong (merge, entry);
					++copyCount;
				}
				//System.Threading.Thread.Sleep(1000);
			}
		}
	}
}

