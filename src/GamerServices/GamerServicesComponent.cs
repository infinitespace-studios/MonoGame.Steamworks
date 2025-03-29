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
	public class GamerServicesComponent : GameComponent
	{
		#region Public Constructor

		public GamerServicesComponent(Game game) : base(game)
		{
		}

		#endregion

		#region Public Methods

		public override void Initialize()
		{
			GamerServicesDispatcher.WindowHandle = Game.Window.Handle;
			GamerServicesDispatcher.Initialize(Game.Services);
		}

		public override void Update(GameTime gameTime)
		{
			GamerServicesDispatcher.Update();
		}

		#endregion
	}
}
