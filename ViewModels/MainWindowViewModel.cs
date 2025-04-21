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
	public string DebugText
	{
		get => _debugText;
		private set
		{
			_debugText += "\n" + value;
			this.RaisePropertyChanged(nameof(DebugText));
		}
	}


	// 音声ファイルラベル
	private string _selectedVoiceFileLabel = "音声ファイルを選択してください";
	public string SelectedVoiceFileLabel
	{
		get => _selectedVoiceFileLabel;
		set
		{
			_selectedVoiceFileLabel = value;
			this.RaisePropertyChanged(nameof(SelectedVoiceFileLabel));
		}
	}

	// シークバーラベル
	private string _elapsedLabel = "--:--";
	public string ElapsedLabel
	{
		get => _elapsedLabel;
		set
		{
			_elapsedLabel = value;
			this.RaisePropertyChanged(nameof(ElapsedLabel));
		}
	}

	private string _remainingLabel = "--:--";
	public string RemainingLabel
	{
		get => _remainingLabel;
		set
		{
			_remainingLabel = value;
			this.RaisePropertyChanged(nameof(RemainingLabel));
		}
	}

	// コマンド
	// 音声ファイル選択コマンド
	public ICommand SelectVoice { get; }
	// 再生コマンド
	public ICommand PlayCommand { get; }

	// 音声再生用のフィールド
	private IWavePlayer? _waveOutDevice;
	private AudioFileReader? _audioFileReader;
	private bool _isPlaying = false;
	// 音声ファイルパス
	private string? _selectedVoiceFile;



	// コンストラクタ
	public MainWindowViewModel()
	{
		// ReactiveCommandを使用してコマンドを作成
		SelectVoice = ReactiveCommand.CreateFromTask(async () =>
		{
			await SelectVoiceFile();
			LoadAudioFile();
		});

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
					// 選択されたファイルのパスを取得、保存
					_selectedVoiceFile = files[0].Path.LocalPath;
				}
			}

		}

	}

	// 音声読み込み
	private void LoadAudioFile()
	{
		if (string.IsNullOrEmpty(_selectedVoiceFile) || !File.Exists(_selectedVoiceFile))
		{
			SelectedVoiceFileLabel = "音声ファイルが選択されていません";
			return;
		}
		
		try
		{
			// 音声ファイル読み込み7
			// 再生デバイスとファイルリーダーを初期化
			_waveOutDevice?.Stop();
			_waveOutDevice?.Dispose();
			_audioFileReader?.Dispose();
			_waveOutDevice = new WaveOutEvent();
			_audioFileReader = new AudioFileReader(_selectedVoiceFile);
			// イベントハンドラを設定
			_waveOutDevice.PlaybackStopped += OnPlaybackStopped;
			// 再生デバイスにファイルを設定して再生
			_waveOutDevice.Init(_audioFileReader);

			// シークバー反映
			UpdateSeekBar();
			DebugText = _audioFileReader.TotalTime.ToString();

			// ラベル表示
			SelectedVoiceFileLabel = Path.GetFileName(_selectedVoiceFile);
		}
		catch (Exception)
		{
			// シークバー反映
			ElapsedLabel = "--:--";
			RemainingLabel = "--:--";
			// ラベル表示
			SelectedVoiceFileLabel = "音声ファイルを読み込めませんでした";
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

	// シークバー反映
	private void UpdateSeekBar()
	{
		if (_audioFileReader != null)
		{
			TimeSpan currentTime = _audioFileReader.CurrentTime;
			TimeSpan remainTime = _audioFileReader.TotalTime - currentTime;
			ElapsedLabel = (60 * currentTime.Hours + currentTime.Minutes).ToString("D2") + ":" + currentTime.Seconds.ToString("D2");
			RemainingLabel = "-" + (60 * remainTime.Hours + remainTime.Minutes).ToString("D2") + ":" + remainTime.Seconds.ToString("D2");
		}
	}
}