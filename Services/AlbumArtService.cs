using System;
using System.IO;
using TagLib;
using Avalonia.Media.Imaging;

namespace ShadowingApp.Services;

public class AlbumArtService
{
	/// <summary>
	/// オーディオファイルからアルバムアートを取得します
	/// </summary>
	/// <param name="audioFilePath">オーディオファイルのパス</param>
	/// <returns>アルバムアートのBitmap、取得できない場合はnull</returns>
	public static Bitmap? GetAlbumArt(string audioFilePath)
	{
		try
		{
			if (!System.IO.File.Exists(audioFilePath))
				return null;

			using (var file = TagLib.File.Create(audioFilePath))
			{
				if (file.Tag.Pictures != null && file.Tag.Pictures.Length > 0)
				{
					var picture = file.Tag.Pictures[0];
					if (picture.Data != null && picture.Data.Count > 0)
					{
						using (var stream = new MemoryStream(picture.Data.Data))
						{
							return new Bitmap(stream);
						}
					}
				}
			}

			return null;
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine($"アルバムアート取得エラー: {ex.Message}");
			return null;
		}
	}
}
