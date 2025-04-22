using System.Threading.Tasks;
using ReactiveUI;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using NAudio.Wave;
using System;
using System.IO;
using Avalonia.Threading;

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
	// 再生時間
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
	// 残り時間
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


	// シークバー
	// 現在値 シークバーからsetすると勝手にgetされるっぽい
	public double CurrentTime
	{
		get
		{
			if (_audioFileReader == null) return 0;
			// 再生時間取得
			return _audioFileReader.CurrentTime.TotalMilliseconds;
		}
		set
		{
			if (_audioFileReader == null) return;
			// 再生時間反映
			_audioFileReader.CurrentTime = TimeSpan.FromMilliseconds(value);
			// シークバーラベル反映
			UpdateSeekBar();
			// 反映
			this.RaisePropertyChanged(nameof(CurrentTime));
		}
	}
	// 最大値
	private double _totalTime = 1;
	public double TotalTime
	{
		get => _totalTime;
		set
		{
			_totalTime = value;
			this.RaisePropertyChanged(nameof(TotalTime));
		}
	}


	// コマンド
	// 音声ファイル選択コマンド
	public ICommand SelectVoice { get; }
	// 音声再生コマンド
	public ICommand PlayControlCommand { get; }


	// 音声再生用のフィールド
	private IWavePlayer? _waveOutDevice;
	private AudioFileReader? _audioFileReader;

	// 音声ファイルパス
	private string? _selectedVoiceFile;

	// タイマー関連
	private System.Timers.Timer? _updateTimer;
	private const int TIMER_INTERVAL = 100; // 100ミリ秒ごとに更新（滑らかさと性能のバランス）

	// コンストラクタ
	public MainWindowViewModel()
	{
		// ReactiveCommandを使用してコマンドを作成
		SelectVoice = ReactiveCommand.CreateFromTask(async () =>
		{
			await SelectVoiceFile();
			LoadAudioFile();
		});

		// 音声再生管理コマンド
		PlayControlCommand = ReactiveCommand.Create(() =>
		{
			if (_waveOutDevice?.PlaybackState == PlaybackState.Playing)
			{
				PauseAudioFile();
			}
			else if (_waveOutDevice != null)
			{
				PlayAudioFile();
			}
		});

		// タイマーの初期化
		_updateTimer = new System.Timers.Timer(TIMER_INTERVAL);
		_updateTimer.Elapsed += (sender, e) =>
		{
			// UIスレッドで実行する必要がある
			if (Application.Current != null)
			{
				// Dispatcher.UIThread を使用
				Dispatcher.UIThread.Post(() =>
					{
						if (_audioFileReader != null && _waveOutDevice?.PlaybackState == PlaybackState.Playing)
						{
							// CurrentTimeプロパティのgetが呼ばれるが、setは呼ばれない
							this.RaisePropertyChanged(nameof(CurrentTime));
							// シークバーラベルも更新
							UpdateSeekBar();
						}
					});
			}
		};
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
			// 音声ファイル読み込み
			// 再生デバイスとファイルリーダーを初期化
			_waveOutDevice?.Stop();
			_waveOutDevice?.Dispose();
			_audioFileReader?.Dispose();
			_waveOutDevice = new WaveOutEvent();
			_audioFileReader = new AudioFileReader(_selectedVoiceFile);
			// 再生デバイスにファイルを設定して再生
			_waveOutDevice.Init(_audioFileReader);

			// シークバー反映
			CurrentTime = 0;
			TotalTime = _audioFileReader.TotalTime.TotalMilliseconds;
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

	// 音声再生
	private void PlayAudioFile()
	{
		// 再生開始
		_waveOutDevice?.Play();
    // タイマー開始
    _updateTimer?.Start();
		// シークバー更新
		UpdateSeekBar();
	}

	// 音声一時停止
	private void PauseAudioFile()
	{
		// 再生停止
		_waveOutDevice?.Pause();
    // タイマー停止
    _updateTimer?.Stop();
		// シークバー更新
		UpdateSeekBar();
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

	// ウィンドウクローズ時の処理
	public void OnWindowClosing()
	{
		// タイマー
		_updateTimer?.Stop();
		_updateTimer?.Dispose();
		// 再生デバイスとファイルリーダー
		_waveOutDevice?.Stop();
		_waveOutDevice?.Dispose();
		_audioFileReader?.Dispose();
	}

}