using System.Threading.Tasks;
using ReactiveUI;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using NAudio.Wave;
using System;
using System.Reactive.Linq;
using System.IO;

namespace ShadowingApp.ViewModels;


public partial class MainWindowViewModel : ViewModelBase
{
	// デバッグ用テキスト
	private string _debugText = "Welcome to Avalonia!";
	// デバッグ用テキスト パブリック
	public string DebugText
	{
		get => _debugText;
		private set
		{
			_debugText += "\n" + value;
			this.RaisePropertyChanged(nameof(DebugText));
		}
	}

	// 音声ファイルパス
	private string _selectedVoiceFile = "音声ファイルを選択してください";
	// 音声ファイルパス パブリック
	public string SelectedVoiceFileLabel
	{
		get => Path.GetFileName(_selectedVoiceFile);
		set
		{
			_selectedVoiceFile = value;
			this.RaisePropertyChanged(nameof(SelectedVoiceFileLabel));
		}
	}

	// コマンドを宣言
	public ICommand SelectVoice { get; }

	// 音声再生用のフィールド
	private IWavePlayer? _waveOutDevice;
	private AudioFileReader? _audioFileReader;
	private bool _isPlaying = false;

	// 再生コマンド
	public ICommand PlayCommand { get; }



	// コンストラクタ
	public MainWindowViewModel()
	{
		// ReactiveCommandを使用してコマンドを作成
		SelectVoice = ReactiveCommand.CreateFromTask(SelectVoiceFile);

		// 再生コマンドの作成 - ファイル選択されている場合のみ実行可能
		// var canPlay = this.WhenAnyValue(
		// 		x => x._selectedVoiceFile,
		// 		file => !string.IsNullOrEmpty(file) && file != "音声ファイルを選択してください" && File.Exists(file)
		// );

		PlayCommand = ReactiveCommand.Create(PlayAudio);
	}



	// 音声ファイル選択
	private async Task SelectVoiceFile()
	{
		// ApplicationLifetime を取得
		if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			// ファイルピッカーのオプションを設定
			var options = new FilePickerOpenOptions
			{
				Title = "音声ファイルを選択",
				AllowMultiple = false,
				FileTypeFilter = new List<FilePickerFileType>
						{
								new FilePickerFileType("音声ファイル")
								{
										Patterns = new[] { "*.wav", "*.mp3" },
										MimeTypes = new[] { "audio/wav", "audio/mpeg" }
								}
						}
			};

			// ファイルピッカーを表示
			if (desktop.MainWindow?.StorageProvider != null)
			{
				var files = await desktop.MainWindow.StorageProvider.OpenFilePickerAsync(options);

				if (files != null && files.Count > 0)
				{
					// 選択されたファイルのパスを取得
					var filePath = files[0].Path.LocalPath;
					SelectedVoiceFileLabel = filePath;
					DebugText = $"選択されたファイル: {filePath}";
				}
				else
				{
					DebugText = "ファイルが選択されませんでした";
				}
			}

		}

	}


	// 音声再生メソッド
	private void PlayAudio()
	{
			try
			{
					// 既に再生中なら停止
					if (_isPlaying)
					{
							StopAudio();
							return;
					}

					// 再生デバイスとファイルリーダーを初期化
					_waveOutDevice = new WaveOutEvent();
					_audioFileReader = new AudioFileReader(_selectedVoiceFile);
					
					// イベントハンドラを設定
					_waveOutDevice.PlaybackStopped += OnPlaybackStopped;
					
					// 再生デバイスにファイルを設定して再生
					_waveOutDevice.Init(_audioFileReader);
					_waveOutDevice.Play();
					
					_isPlaying = true;
					DebugText = "再生開始しました";
			}
			catch (Exception ex)
			{
					DebugText = $"再生エラー: {ex.Message}";
					StopAudio();
			}
	}

	// 再生停止メソッド
	private void StopAudio()
	{
			if (_waveOutDevice != null)
			{
					_waveOutDevice.PlaybackStopped -= OnPlaybackStopped;
					_waveOutDevice.Stop();
					_waveOutDevice.Dispose();
					_waveOutDevice = null;
			}

			if (_audioFileReader != null)
			{
					_audioFileReader.Dispose();
					_audioFileReader = null;
			}

			_isPlaying = false;
			DebugText = "再生停止しました";
	}

	// 再生終了時のイベントハンドラ
	private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
	{
			StopAudio();
	}

	// リソース解放
	public void Dispose()
	{
			StopAudio();
	}
}