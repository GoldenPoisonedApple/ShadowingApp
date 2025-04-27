using NAudio.Wave;
using System;
using System.IO;

namespace ShadowingApp.Services
{
	/// <summary>
	/// 音声録音
	/// </summary>
	public class AudioRecorderService : IDisposable
	{
		private WaveInEvent? _waveIn;
		private WaveFileWriter? _waveWriter;
		private string? _outputFilePath;

		public event EventHandler? RecordingStateChanged;

		public bool IsRecording { get; private set; }

		/// <summary>
		/// 録音開始
		/// </summary>
		/// <param name="outputFolder">出力フォルダ</param>
		/// <returns>録音ファイルパス</returns>
		public string StartRecording(string outputFolder)
		{
			// すでに録音中の場合は停止
			if (IsRecording)
			{
				StopRecording();
			}

			try
			{
				// フォルダが存在しない場合は作成
				if (!Directory.Exists(outputFolder))
				{
					Directory.CreateDirectory(outputFolder);
				}

				// 出力ファイルパスの生成（現在時刻をファイル名に）
				_outputFilePath = Path.Combine(outputFolder,
						$"Recording_{DateTime.Now:yyyyMMdd_HHmmss}.wav");

				// 録音デバイスの初期化
				_waveIn = new WaveInEvent
				{
					WaveFormat = new WaveFormat(44100, 1)  // 44.1kHz, モノラル
				};

				// 出力ファイルの初期化
				_waveWriter = new WaveFileWriter(_outputFilePath, _waveIn.WaveFormat);

				// データ受信時のイベントハンドラを設定
				_waveIn.DataAvailable += (s, e) =>
				{
					_waveWriter?.Write(e.Buffer, 0, e.BytesRecorded);
				};

				// 録音停止時のイベントハンドラを設定
				_waveIn.RecordingStopped += (s, e) =>
				{
					_waveWriter?.Dispose();
					_waveWriter = null;
					_waveIn?.Dispose();
					_waveIn = null;
					IsRecording = false;
					RecordingStateChanged?.Invoke(this, EventArgs.Empty);
				};

				// 録音開始
				_waveIn.StartRecording();
				IsRecording = true;
				RecordingStateChanged?.Invoke(this, EventArgs.Empty);

				return _outputFilePath;
			}
			catch (Exception)
			{
				StopRecording();
				throw;
			}
		}

		/// <summary>
		/// 録音停止
		/// </summary>
		/// <returns>録音したファイルパス（失敗時はnull）</returns>
		public string? StopRecording()
		{
			if (!IsRecording)
				return null;

			_waveIn?.StopRecording();

			// RecordingStoppedイベントハンドラで後処理が行われる
			return _outputFilePath;
		}

		public void Dispose()
		{
			StopRecording();
			_waveWriter?.Dispose();
			_waveIn?.Dispose();
			_waveWriter = null;
			_waveIn = null;
		}
	}
}