using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Diagnostics;
using System.Globalization;

namespace ShadowingApp.Converters
{
	public class ToggleImageSourceConverter : IValueConverter
	{
		private const string DefaultImagePath = "avares://ShadowingApp/Assets/Images/test.png";

		public string FalseValue { get; set; } = string.Empty;
		public string TrueValue { get; set; } = string.Empty;

		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			if (value is bool boolValue)
			{
				Debug.WriteLine($"ToggleImageSourceConverter: {boolValue}");

				var path = boolValue ? TrueValue : FalseValue;

				if (!string.IsNullOrEmpty(path))
				{
					try
					{
						// 新しい方法でAssetLoaderを取得
						var uri = new Uri(path);
						var assets = AssetLoader.Open(uri);
						return new Bitmap(assets);
					}
					catch (Exception ex)
					{
						Debug.WriteLine($"Error loading image: {ex.Message}");
					}
				}
			}

			return null;
		}

		public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}