/// <summary>
/// Program.cs
/// Contains MainClass
/// </summary>
/// Author:
///   Cassie E Nicol
///
/// Copyright (C) Cassie E Nicol 2016

using System;
using System.Collections.Generic;
using System.IO;
using log4net;
using log4net.Config;
using NHibernate;
using NHibernate.Cfg;
using MusicMerge.Domain;
using NHibernate.Tool.hbm2ddl;

using MusicTool.Options;


/* Usage: MusicMerge [options] inputdir...
 * 
 * What are duplicates?
 * Songs with the same content
 * 
 */

namespace MusicTool
{
	/// <summary>
	/// MainClass .
	/// </summary>
	class MusicTool : IDisposable
	{
		private static readonly log4net.ILog log =
			log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Define the possible exit codes. 
		/// As always, Success is 0 for all *nix apps.
		/// </summary>
		public enum ExitCode : int
		{
			Success = 0,
			GeneralFailure = 1,
			InvalidFilename = 2,
			Exception = 3
		}


		public static MusicTool Instance { get; private set; }

		// Non-Static data
		public ConfigOptions Options { get; protected set; }
		public ISessionFactory factory { get; set; }

		public List<DirectoryInfo> Directories => Options.Directories;


		//{ get { value = options.Fix.Length != 0; } }

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name="args">The command-line arguments.</param>
		public static int Main (string[] args)
		{
			ExitCode result = ExitCode.Success;

			string appConfigFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
			log.InfoFormat ("Config = {0}", appConfigFile);

			XmlConfigurator.Configure(new System.IO.FileInfo("log4net.cfg.xml"));

			// Set logging level to warn.
			//((log4net.Repository.Hierarchy.Logger)logger.Logger).Level = log4net.Core.Level.Warn;

			string exePath = System.Reflection.Assembly.GetEntryAssembly ().Location;
			string exeName = Path.GetFileName (System.Reflection.Assembly.GetEntryAssembly ().Location);
			string versionNumber = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

			// Yields "mono..."
			// System.Diagnostics.Process.GetCurrentProcess ().MainModule.FileName;
				
			log.Info (exeName);

			Console.WriteLine ("{0} {1}", exeName, versionNumber);
			try {
				Instance = new MusicTool ();
				Instance.Options = new ConfigOptions();
				Instance.ProcessArgs (args);
				Instance.Run ();
				return (int) result;
			} catch(FileNotFoundException e) {
				Console.Error.WriteLine ("FileNotFoundException: {0}: {1}", e.Message, e.FileName);
				result = ExitCode.Exception;
			} catch(TypeInitializationException e) {
				Console.Error.WriteLine ("Exception: {0}", e.Message);
				Console.Error.WriteLine (e.StackTrace);
				result = ExitCode.Exception;
			} catch(StopNowException e) {
				Console.Error.WriteLine ("Exception: {0}", e.Message);
				result = ExitCode.GeneralFailure;
			} catch (Exception e) {
				Console.Error.WriteLine (e.GetType().Name);
				Console.Error.WriteLine ("Exception: {0}", e.Message);
				Console.Error.WriteLine (e.StackTrace);
				result = ExitCode.Exception;
			}
			return (int)result;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MusicMerge.MainClass"/> class.
		/// </summary>
		MusicTool()
		{
			log.Info ("starting logging");
		}

		public void Dispose()
		{
			Options = null;

			Instance = null;
		}

		/// <summary>
		/// Processes the command line arguments.
		/// </summary>
		/// <param name="args">Arguments.</param>
		private void ProcessArgs (string[] args)
		{
			try {
				Options.Parse(args);
			} catch(FileNotFoundException e) {
				// TODO Fix the exception code.
				log.ErrorFormat ("FileNotFoundException: {0}: {1}", e.Message, e.FileName);
				string message = "File not found: " + e.FileName;
				throw new StopNowException (message, e);
			}

		}

		protected void Run () {

			if(Options.DoFix) {
				switch(Options.Fix) 
				{
				case "filename":
					FixFilename();
					break;
				default:
					Console.WriteLine("Invalid selection.");
					break;
				}
			}

			// Everything else needs nhibernate
			ConfigureHibernate();

			//return ExitCode.Success;
			Search ();

			if (Options.DoMerge) {
				
			}

			//return result;
		}

		/// <summary>
		/// Configures the hibernate library.
		/// </summary>
		private void ConfigureHibernate()
		{
			log.Info ("Configuring Hibernate");

			var cfg = new Configuration ();
			cfg.Configure ();
			cfg.AddAssembly (typeof(MusicFile).Assembly);

			factory = cfg.BuildSessionFactory ();

			if (factory == null) {
				throw new NullReferenceException ("Hibernate Factory is null");
			}

			// For the following section to work, property hbm2ddl.auto must be commented out.
			//   <!--
			//   <property name="hbm2ddl.auto">update</property>
			//	 -->
			// Uncomment the property hbm2ddl.auto to always update the table automitically.
			// and the following section will not matter.

			if(Options.Reset) {
				var schema = new SchemaExport(cfg);
				schema.Execute(false, true, false);
			} else {
				new SchemaUpdate (cfg).Execute (false, true);
			}

			#if SHOW_SCHEMA
			NHibernate.Dialect.Dialect d = new NHibernate.Dialect.MySQLDialect();
			string[] x = cfg.GenerateSchemaCreationScript (d);
			foreach (var s in x) {
			Console.WriteLine (s);
			}
			#endif
		
			#if CLEAR_ENTRIES
			if (Options.Reset) {
				using (ISession session = factory.OpenSession ())
				using (ITransaction tx = session.BeginTransaction ()) 
				{
					session.CreateQuery("delete MusicFile f").ExecuteUpdate();
					session.CreateQuery("delete RejectFile r").ExecuteUpdate();
					session.CreateQuery("delete IgnoreFile i").ExecuteUpdate();
					tx.Commit ();
				}
			}
			#endif
		}

		/// <summary>
		/// Run the specified args.
		/// </summary>
		/// <param name="args">Arguments.</param>
		private ExitCode Search ()
		{
//			if (fix.Equals ("disk")) {
//				using( var op = new FixDiskNumber ())
//				using(MusicSearch ms = new MusicSearch( op ))
//				mm.DrillDown (GetDirectoryInfo (merge));
//				var entries = mc.Entries;
//				foreach (var entry in entries ) {
//					Console.WriteLine (entry);
//				}
//				return (int)ExitCode.Success;
//			}

			if (Options.Verbose > 0) {
				Console.WriteLine ("\n{0} Directories to process", Directories.Count);
				foreach (var dir in Directories) {
					Console.WriteLine ("\t{0}", dir.FullName);
				}
			}

			// Search all included directories.
			IMusicCollection op;
			if (Options.Fix.Equals ("disk")) {
				op = new FixDiskNumber ();
			} else {
				op = new DbMusicCollection (factory);
			}
			MusicSearch ms = new MusicSearch ( op );
			foreach (var dir in Directories) {
				ms.DrillDown (dir);
			}

#if USE_LIST
			var mc = new ListMusicCollection ();
			MusicSearch mm = new MusicSearch( mc );
			mm.DrillDown (GetDirectoryInfo (merge));
			var entries = mc.Entries;
			foreach (var entry in entries ) {
				Console.WriteLine (entry);
			}
#endif

			return ExitCode.Success;
		}

		/// <summary>
		/// Changes the current filename to match the ID3 tag information.
		/// </summary>
		protected void FixFilename() {

			IMusicCollection op = new MusicMerge.FixFilename ();
			MusicSearch ms = new MusicSearch ( op );
			foreach (var dir in Directories) {
				ms.DrillDown (dir);
			}
		}


	} // class		
} // namespace
