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

namespace Microsoft.Xna.Framework.GamerServices
{
	public struct LeaderboardIdentity
	{
		#region Public Properties

		public string Key
		{
			get;
			set;
		}

		public int GameMode
		{
			get;
			set;
		}

		#endregion

		#region Public Static Methods

		public static LeaderboardIdentity Create(LeaderboardKey key)
		{
			return new LeaderboardIdentity()
			{
				Key = key.ToString(),
				GameMode = 0
			};
		}

		public static LeaderboardIdentity Create(LeaderboardKey key, int gameMode)
		{
			return new LeaderboardIdentity()
			{
				Key = key.ToString(),
				GameMode = gameMode
			};
		}

		#endregion
	}
}
