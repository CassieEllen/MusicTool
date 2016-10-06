using System;
using System.Collections.Generic;
using System.IO;

// Documentation on Mono.Options can be found here:
// http://docs.go-mono.com/?link=T%3aMono.Options.ArgumentSource%2fP
// https://components.xamarin.com/gettingstarted/mono.options?version=4.2.2.0
//
// OptionSet
// http://www.ndesk.org/doc/ndesk-options/NDesk.Options/OptionSet.html

/// <summary>
/// Config options.
/// </summary>
using Mono.Options;

// FileNotFoundException is thrown when a directory to be processed cannot be found. The
// directory may be from the list file, on the command line, or the merge directory.
//
// When this error is thrown, the system is in an unusable state, so the program needs to
// terminate.

namespace MusicTool.Options
{
	public class ConfigOptions : IDisposable
	{
		private static readonly log4net.ILog log =
			log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Defines the command line options
		/// </summary>
		private OptionSet options;

		// these variables will be set when the command line is parsed
		public int Verbose { get; set; } = 0;
		public Boolean Reset { get; set; } = false;
		Boolean Help { get; set; } = false;
		public string Merge { get; set; } = string.Empty;
		public string ListFile { get; set; } = string.Empty;
		public string Fix { get; set; } = string.Empty;
		List<string> extras = null;

		// Test Properties - Just for simplication of the code.
		public bool DoMerge => ! string.IsNullOrWhiteSpace (Merge);
		public bool DoFix   => ! string.IsNullOrWhiteSpace (Fix);
		public bool HasDirectories => 0 != Directories.Count;

		/// <summary>
		/// The directories to be processed. Includes the merge directory if specified.
		/// </summary>
		public List<DirectoryInfo> Directories { get; protected set; } 

		public ConfigOptions ()
		{
			// thses are the available options, not that they set the variables
			options = new OptionSet { 
				{ "l|list=", "directory list", l => ListFile = l },
				{ "h|help", "show this message and exit", h => { Help = true; } },
				{ "m|merge=", "the name of the merge directory", m => Merge = m },
				{ "v|verbose", "increase message verbosity.", v => { ++Verbose; } },
				{ "f|fix=", "fix", f => Fix = f },
				{ "r|reset", "reset tables", r => { Reset=true; } },
				};
			Directories = new List<DirectoryInfo> ();
		}

		public void Dispose ()
		{
			options.Clear ();
			options = null;

			extras.Clear ();
			extras = null;

			Directories.Clear ();
			Directories = null;
		}

		/// <summary>
		/// Shows help for the application
		/// </summary>
		public void Usage ()
		{
			Console.WriteLine ("usage: MusicMerge.exe [options] [basedir]...");
			options.WriteOptionDescriptions (Console.Out);
		}
			
		private void Show(string format, object arg)
		{				
			if (Verbose > 0) {
				Console.WriteLine (format, arg);
			}
		}

		private void Show(string format, params object[] args)
		{				
			if (Verbose > 0) {
				Console.WriteLine (format, args);
			}
		}
			
		public void Parse(string[] args)
		{
			// Parse the options from the command line arguments.
			try {
				extras = options.Parse (args);
			} catch (OptionException e) {
				log.Error (e.Message);
				throw;
			}

			log.InfoFormat ("verbose: {0}", Verbose);

			if (Help) {
				Usage ();
				return;
			}

			// If specified, add the merge file to the directories list. 
			if ( ! string.IsNullOrWhiteSpace (Merge) ) {
				Show ("merge={0}", Merge);
				AddToDirectories (Merge);
			}

			// If specified, add the file listed in the list file 
			// to the directories list.
			if ( ! string.IsNullOrWhiteSpace (ListFile)) {
				Show ("listFile={0}", ListFile);
				ProcessFileList (ListFile);
			}

			if (extras.Count > 0) {
				Console.WriteLine ("\nextras");
				foreach (var entry in extras) {
					Console.WriteLine ("\t{0}", entry);
				}
				Console.WriteLine ();
				ProcessExtras ();
			}

		}

		private DirectoryInfo GetDirectoryInfo (string dir)
		{
			string line = dir.Trim();
			if (line.EndsWith ("/")) {
				line = line.Remove (dir.Length - 1);
			}

			DirectoryInfo info = new DirectoryInfo (line);
			if (!info.Exists) {
				throw new FileNotFoundException("File does not exist", info.FullName);
			}
			return info;
		}

		/// <summary>
		/// Adds info to <see cref="MusicMerge.MainClass.directories"/> list.
		/// </summary>
		/// <param name="info">Info.</param>
		private void AddToDirectories(string dir) 
		{
			DirectoryInfo info = GetDirectoryInfo (dir);
			if (! info.Exists) {
				throw new FileNotFoundException("File does not exist", info.FullName);
			}
			bool exists = Directories.Exists (x => x.FullName.Equals (info.FullName));
			if (!exists) {
				Directories.Add (info);
			}
		}

		private void ProcessFileList (string filename)
		{
			if (string.IsNullOrWhiteSpace (filename)) {
				throw new ArgumentException ("Invalid list filename: \"{0}\"", filename);
			}

			FileInfo fi = new FileInfo (filename);
			if (!fi.Exists) {
				throw new FileNotFoundException ("Could not read list file.", fi.FullName);
			}

			Show ("Processing file: {0}", fi.FullName);

			try {
				using (FileStream fs = fi.OpenRead ())
				using (System.IO.StreamReader file = new StreamReader (fs)) {
					string line;
					while ((line = file.ReadLine ()) != null) {
						Show ("\t{0}", line);
						if (line.StartsWith ("#") || string.IsNullOrWhiteSpace (line)) {
							continue;
						}
						AddToDirectories(line);
					}
				}
			} catch (FileNotFoundException e) {
				//log.ErrorFormat ("FileNotFoundException: {0}: {1}", e.Message, e.FileName);
				log.DebugFormat(e.StackTrace);
				throw;
			} catch (SystemException e) {
				log.ErrorFormat ("SystemException: {0}", e.Message);
				log.Error (e.StackTrace);
				throw;
			}
		}

		private void ProcessExtras()
		{
			foreach (var name in extras) {
				AddToDirectories(name);
			}
		}




	}
}

