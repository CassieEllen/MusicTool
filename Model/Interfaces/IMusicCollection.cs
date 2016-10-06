using System;
using System.Collections.Generic;

namespace MusicMerge
{
	public interface IMusicCollection
	{
		/// <summary>
		/// Add the specified info.
		/// </summary>
		/// <param name="info">Info.</param>
		void Add(MusicInfo info);

		/// <summary>
		/// Reject the specified info.
		/// </summary>
		/// <param name="info">Info.</param>
		void Reject(MusicInfo info);

		/// <summary>
		/// Ignore the specified info.
		/// </summary>
		/// <param name="info">Info.</param>
		void Ignore(MusicInfo info);
	}
}

