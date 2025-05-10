using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ShadowingApp.Services
{
	/// <summary>
	/// フォルダ内の音声ファイルシーケンスをナビゲートするためのサービス
	/// </summary>
	public class FileSequenceNavigator
	{
		// サポートする音声ファイル拡張子
		private static readonly string[] SupportedExtensions = { ".mp3", ".wav" };

		/// <summary>
		/// 現在のファイルから同じフォルダ内の次の音声ファイル（名前順）を取得します
		/// </summary>
		/// <param name="currentPath">現在の音声ファイルのパス</param>
		/// <returns>次の音声ファイルのパス、存在しない場合は null</returns>
		public static string NextAudioPath(string currentPath)
		{
			if (string.IsNullOrEmpty(currentPath) || !File.Exists(currentPath))
				return string.Empty;

			try
			{
				string? directory = Path.GetDirectoryName(currentPath);
				if (string.IsNullOrEmpty(directory))
					return string.Empty;

				// フォルダ内の音声ファイルをすべて取得して名前順にソート
				List<string> audioFiles = GetSortedAudioFiles(directory);
				if (audioFiles.Count == 0)
					return string.Empty;

				// 現在のファイルのインデックスを取得
				int currentIndex = audioFiles.IndexOf(currentPath);

				// 現在のファイルが見つからない場合
				if (currentIndex == -1)
					return audioFiles.FirstOrDefault() ?? string.Empty;

				// 次のファイルを返す（最後のファイルの場合は最初に戻る）
				int nextIndex = (currentIndex + 1) % audioFiles.Count;
				return audioFiles[nextIndex];
			}
			catch (Exception ex)
			{
				// 例外処理（必要に応じてログ出力など）
				System.Diagnostics.Debug.WriteLine($"次のファイル取得エラー: {ex.Message}");
				return string.Empty;
			}
		}

		/// <summary>
		/// 現在のファイルから同じフォルダ内の前の音声ファイル（名前順）を取得します
		/// </summary>
		/// <param name="currentPath">現在の音声ファイルのパス</param>
		/// <returns>前の音声ファイルのパス、存在しない場合は null</returns>
		public static string PreviousAudioPath(string currentPath)
		{
			if (string.IsNullOrEmpty(currentPath) || !File.Exists(currentPath))
				return string.Empty;

			try
			{
				string? directory = Path.GetDirectoryName(currentPath);
				if (string.IsNullOrEmpty(directory))
					return string.Empty;

				// フォルダ内の音声ファイルをすべて取得して名前順にソート
				List<string> audioFiles = GetSortedAudioFiles(directory);
				if (audioFiles.Count == 0)
					return string.Empty;

				// 現在のファイルのインデックスを取得
				int currentIndex = audioFiles.IndexOf(currentPath);

				// 現在のファイルが見つからない場合
				if (currentIndex == -1)
					return audioFiles.LastOrDefault() ?? string.Empty;

				// 前のファイルを返す（最初のファイルの場合は最後に戻る）
				int prevIndex = (currentIndex - 1 + audioFiles.Count) % audioFiles.Count;
				return audioFiles[prevIndex];
			}
			catch (Exception ex)
			{
				// 例外処理（必要に応じてログ出力など）
				System.Diagnostics.Debug.WriteLine($"前のファイル取得エラー: {ex.Message}");
				return string.Empty;
			}
		}

		/// <summary>
		/// フォルダ内の音声ファイルを名前順でソートして取得します
		/// </summary>
		/// <param name="directory">検索対象フォルダ</param>
		/// <returns>ソートされた音声ファイルパスのリスト</returns>
		private static List<string> GetSortedAudioFiles(string directory)
		{
			List<string> audioFiles = new List<string>();

			foreach (string extension in SupportedExtensions)
			{
				audioFiles.AddRange(Directory.GetFiles(directory, $"*{extension}", SearchOption.TopDirectoryOnly));
			}

			// ファイル名でソート（自然順ソートの実装も検討可）
			return audioFiles.OrderBy(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase).ToList();
		}
	}
}