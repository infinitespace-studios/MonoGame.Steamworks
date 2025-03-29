#region License
/* MonoGame.Steamworks based on
 * FNA.Steamworks - XNA4 Xbox Live Reimplementation for Steamworks
 * Copyright 2016 Ethan "flibitijibibo" Lee
 * Copyright 2025 Dean "dellis1972" Ellis
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

#region Using Statements
using System;
#endregion

namespace Microsoft.Xna.Framework.Net
{
	public class GamerJoinedEventArgs : EventArgs
	{
		#region Public Properties

		public NetworkGamer Gamer
		{
			get;
			private set;
		}

		#endregion

		#region Public Constructor

		public GamerJoinedEventArgs(NetworkGamer gamer)
		{
			Gamer = gamer;
		}

		#endregion
	}
}
