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
using System.IO;
using System.Globalization;
using Steamworks;
#endregion

namespace Microsoft.Xna.Framework.GamerServices
{
	public sealed class GamerProfile : IDisposable
	{
		#region Public Properties

		public int GamerScore
		{
			get;
			private set;
		}

		public GamerZone GamerZone
		{
			get;
			private set;
		}

		public string Motto
		{
			get;
			private set;
		}

		public RegionInfo Region
		{
			get;
			private set;
		}

		public float Reputation
		{
			get;
			private set;
		}

		public int TitlesPlayed
		{
			get;
			private set;
		}

		public int TotalAchievements
		{
			get;
			private set;
		}

		public bool IsDisposed
		{
			get;
			private set;
		}

		#endregion

		#region Internal Constructor

		internal GamerProfile()
		{
			IsDisposed = false;

			// TODO: Everything below
			GamerScore = 0;
			GamerZone = GamerZone.Pro; // WUBWUBWUBWUBWUB
			Motto = string.Empty;
			Region = RegionInfo.CurrentRegion;
			Reputation = 5.0f;
			TitlesPlayed = 1;
			TotalAchievements = 0;
		}

		#endregion

		#region Public Methods

		public void Dispose()
		{
			IsDisposed = true;
		}

		public Stream GetGamerPicture()
		{
			var id = SteamFriends.GetSmallFriendAvatar(SteamUser.GetSteamID());
			uint ImageWidth;
			uint ImageHeight;
			bool bIsValid = SteamUtils.GetImageSize(id, out ImageWidth, out ImageHeight);

			if (bIsValid)
			{
				byte[] image = new byte[ImageWidth * ImageHeight * 4];
				byte[] flipped = new byte[ImageWidth * ImageHeight * 4];

				bIsValid = SteamUtils.GetImageRGBA(id, image, (int)(ImageWidth * ImageHeight * 4));
				if (bIsValid)
				{
					// convert RGBA -> BGRA
					for (var i = 0; i < image.Length; i += 4)
					{
						var r = image[i];
						var g = image[i + 1];
						var b = image[i + 2];
						var a = image[i + 3];
						image[i] = b;
						image[i + 1] = g;
						image[i + 2] = r;
						image[i + 3] = a;
					}
					// Flip Vertically
					uint len = (uint)image.Length;
					for (uint row = 0; row < ImageHeight; row++)
					{
						for (uint x = 0; x < ImageWidth; x++)
						{
							uint pixel = row * ImageWidth * 4 + x * 4;
							uint destPixel = (ImageHeight - row - 1) * ImageWidth * 4 + x * 4;
							flipped[destPixel] = image[pixel];
							flipped[destPixel + 1] = image[pixel + 1];
							flipped[destPixel + 2] = image[pixel + 2];
							flipped[destPixel + 3] = image[pixel + 3];
						}
					}

					var ms = new MemoryStream();
					using var sw = new BinaryWriter(ms, encoding: System.Text.Encoding.UTF8, leaveOpen: true);
					sw.Write('B');
					sw.Write('M');
					sw.Write((uint)flipped.Length + 14 + 40);
					sw.Write((ushort)0);
					sw.Write((ushort)0);
					sw.Write((uint)54); // offset 32 bit
					sw.Write((uint)40); // header size 
					sw.Write(ImageWidth);
					sw.Write(ImageHeight);
					sw.Write((ushort)1);
					sw.Write((ushort)32); // bpp
					sw.Write((uint)0);  // compress
					sw.Write((uint)0);// img sz
					sw.Write((uint)0); //hr
					sw.Write((uint)0); //vr
					sw.Write((uint)0);//
					sw.Write((uint)0);//
					sw.Flush();
					ms.Write(flipped, 0, flipped.Length);
					ms.Position = 0;
					return ms;
				}
			}
			return null;
		}

		#endregion
	}
}
