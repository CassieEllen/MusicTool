using System;
using System.Collections.Generic;
using MusicTool.Model.Interfaces;

namespace MusicTool.Model.Impl
{
	public class ListMusicCollection : IMusicCollection
	{
		public SortedList<string, MusicInfo> entries;


		public ListMusicCollection ()
		{
			entries = new SortedList<string, MusicInfo>();
		}

		public void Add(MusicInfo info) {
			entries.Add (info.Md5sum, info);
		}

		public void Reject(MusicInfo info) {
			throw new NotImplementedException ();
			//System.Reflection.MethodBase.GetCurrentMethod ());
		}

		public void Ignore(MusicInfo info) {
			throw new NotImplementedException ();
		}

		public IList<MusicInfo> Entries {
			get { 
				return entries.Values; 
			}
		}

	}
}

