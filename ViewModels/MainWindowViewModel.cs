using System;
using System.Threading.Tasks;
using ReactiveUI;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using System.IO;
using Avalonia.Threading;
using ShadowingApp.Services;

namespace ShadowingApp.ViewModels;


public class MainWindowViewModel : ViewModelBase, IDisposable
{
	private readonly AudioPlayerService _audioPlayer;
	private readonly System.Timers.Timer _updateTimer;
	private const int TIMER_INTERVAL = 100;

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
		set => this.RaiseAndSetIfChanged(ref _selectedVoiceFileLabel, value);
	}

	// シークバーラベル
	private string _elapsedLabel = "--:--";
	public string ElapsedLabel
	{
		get => _elapsedLabel;
		set => this.RaiseAndSetIfChanged(ref _elapsedLabel, value);
	}

	private string _remainingLabel = "--:--";
	public string RemainingLabel
	{
		get => _remainingLabel;
		set => this.RaiseAndSetIfChanged(ref _remainingLabel, value);
	}

	// シークバー値
	public double CurrentTime
	{
		get => _audioPlayer.CurrentTime.TotalMilliseconds;
		set
		{
			_audioPlayer.CurrentTime = TimeSpan.FromMilliseconds(value);
			UpdateSeekBar();
		}
	}

	private double _totalTime = 1;
	public double TotalTime
	{
		get => _totalTime;
		set => this.RaiseAndSetIfChanged(ref _totalTime, value);
	}

	// 再生中かどうか
	public bool IsPlaying => _audioPlayer.IsPlaying;

	// コマンド
	public ICommand SelectVoiceCommand { get; }
	public ICommand PlayControlCommand { get; }

	// 選択されたファイルパス
	private string? _selectedVoiceFile;

	// コンストラクタ
	public MainWindowViewModel()
	{
		// 再生サービス
		_audioPlayer = new AudioPlayerService();
		_audioPlayer.PlaybackStateChanged += (_, _) =>
		{
			this.RaisePropertyChanged(nameof(IsPlaying));
		};
		_audioPlayer.PositionChanged += (_, _) =>
		{
			UpdateSeekBar();
			this.RaisePropertyChanged(nameof(CurrentTime));
		};

		// コマンド初期化
		SelectVoiceCommand = ReactiveCommand.CreateFromTask(SelectVoiceFileAsync);
		PlayControlCommand = ReactiveCommand.Create(TogglePlayback);

		// タイマーの初期化
		_updateTimer = new System.Timers.Timer(TIMER_INTERVAL);
		_updateTimer.Elapsed += (_, _) =>
		{
			Dispatcher.UIThread.Post(() =>
							{
					if (_audioPlayer.IsPlaying)
					{
						this.RaisePropertyChanged(nameof(CurrentTime));
						UpdateSeekBar();
					}
				});
		};
	}

	private async Task SelectVoiceFileAsync()
	{
		await SelectVoiceFile();
		if (!string.IsNullOrEmpty(_selectedVoiceFile))
		{
			LoadAudioFile();
		}
		this.RaisePropertyChanged(nameof(IsPlaying));
	}

	private void TogglePlayback()
	{
		if (IsPlaying)
		{
			PauseAudioFile();
		}
		else
		{
			PlayAudioFile();
		}
		this.RaisePropertyChanged(nameof(IsPlaying));
	}

	// 音声ファイル選択
	private async Task SelectVoiceFile()
	{
		if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
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

			if (desktop.MainWindow?.StorageProvider != null)
			{
				var files = await desktop.MainWindow.StorageProvider.OpenFilePickerAsync(options);
				if (files != null && files.Count > 0)
				{
					_selectedVoiceFile = files[0].Path.LocalPath;
				}
			}
		}
	}

	// 音声読み込み
	private void LoadAudioFile()
	{
		if (string.IsNullOrEmpty(_selectedVoiceFile))
		{
			SelectedVoiceFileLabel = "音声ファイルが選択されていません";
			return;
		}

		if (_audioPlayer.LoadFile(_selectedVoiceFile))
		{
			// シークバー反映
			TotalTime = _audioPlayer.TotalTime.TotalMilliseconds;
			UpdateSeekBar();
			DebugText = _audioPlayer.TotalTime.ToString();

			// ラベル表示
			SelectedVoiceFileLabel = Path.GetFileName(_selectedVoiceFile);
		}
		else
		{
			// エラー時の表示
			ElapsedLabel = "--:--";
			RemainingLabel = "--:--";
			SelectedVoiceFileLabel = "音声ファイルを読み込めませんでした";
		}
	}

	// 音声再生
	private void PlayAudioFile()
	{
		_audioPlayer.Play();
		_updateTimer.Start();
		UpdateSeekBar();
	}

	// 音声一時停止
	private void PauseAudioFile()
	{
		_audioPlayer.Pause();
		_updateTimer.Stop();
		UpdateSeekBar();
	}

	// シークバー反映
	private void UpdateSeekBar()
	{
		TimeSpan currentTime = _audioPlayer.CurrentTime;
		TimeSpan remainTime = _audioPlayer.TotalTime - currentTime;

		ElapsedLabel = FormatTimespan(currentTime);
		RemainingLabel = "-" + FormatTimespan(remainTime);
	}

	private string FormatTimespan(TimeSpan time)
	{
		return (60 * time.Hours + time.Minutes).ToString("D2") + ":" + time.Seconds.ToString("D2");
	}

	// ウィンドウクローズ時の処理
	public void OnWindowClosing()
	{
		Dispose();
	}

	public void Dispose()
	{
		_updateTimer?.Stop();
		_updateTimer?.Dispose();
		_audioPlayer?.Dispose();
	}
}
