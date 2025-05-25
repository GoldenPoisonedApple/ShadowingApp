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
using System.Diagnostics;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Rendering;
using System.Runtime.CompilerServices;

namespace ShadowingApp.ViewModels;


public class MainWindowViewModel : ViewModelBase, IDisposable
{
	private readonly AudioPlayerService _audioPlayer;
	private readonly System.Timers.Timer _updateTimer;
	private const int TIMER_INTERVAL = 100;


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

	// アルバムアート用のプロパティ
	private Bitmap? _albumArt;
	public Bitmap? AlbumArt
	{
		get => _albumArt;
		set => this.RaiseAndSetIfChanged(ref _albumArt, value);
	}

	// デフォルトの画像
	private readonly Bitmap? _defaultAlbumArt = new Bitmap(AssetLoader.Open(new Uri("avares://ShadowingApp/Assets/音楽ファイルアイコン 1.png")));


	// シークバー値
	public double CurrentTime
	{
		get
		{
			UpdateSeekBar();
			return _audioPlayer.CurrentTime.TotalMilliseconds;
		}
		set => _audioPlayer.CurrentTime = TimeSpan.FromMilliseconds(value);
	}

	private double _totalTime = 1;
	public double TotalTime
	{
		get => _totalTime;
		set => this.RaiseAndSetIfChanged(ref _totalTime, value);
	}

	// 再生中かどうか
	public bool IsPlaying => _audioPlayer.IsPlaying;

	private readonly AudioRecorderService _audioRecorder;
	private readonly AudioPlayerService _recordAudioPlayer;

	// 録音中かどうか
	public bool IsRecording => _audioRecorder.IsRecording;

	// 選択されたファイルパス
	private string? _selectedVoiceFile;


	// コマンド
	public ICommand SelectVoiceCommand { get; }
	public ICommand PlayControlCommand { get; }
	public ICommand RecordControlCommand { get; }
	public ICommand SkipBackwardCommand { get; }
	public ICommand PreviousAudioCommand { get; }
	public ICommand NextAudioCommand { get; }



	// コンストラクタ
	public MainWindowViewModel()
	{
		// 再生サービス
		_audioPlayer = new AudioPlayerService();
		_audioPlayer.PlaybackStateChanged += (_, _) =>
		{
			this.RaisePropertyChanged(nameof(IsPlaying));

			// 再生状態に応じてタイマーを制御
			if (_audioPlayer.IsPlaying)
				_updateTimer?.Start();
			else
				_updateTimer?.Stop();
		};

		// 録音サービス
		_audioRecorder = new AudioRecorderService();
		_audioRecorder.RecordingStateChanged += (_, _) =>
		{
			this.RaisePropertyChanged(nameof(IsRecording));
		};
		// 録音音声再生サービス
		_recordAudioPlayer = new AudioPlayerService();

		// デフォルトのアルバムアート画像の読み込み
		AlbumArt = _defaultAlbumArt;

		// コマンド初期化
		SelectVoiceCommand = ReactiveCommand.CreateFromTask(SelectVoiceFileAsync);
		PlayControlCommand = ReactiveCommand.Create(TogglePlayback);
		RecordControlCommand = ReactiveCommand.CreateFromTask(ToggleRecording);
		SkipBackwardCommand = ReactiveCommand.Create<object>(param => SkipBackward(int.Parse((string)param)));
		PreviousAudioCommand = ReactiveCommand.Create(PriviousAudioFile);
		NextAudioCommand = ReactiveCommand.Create(SkipAudioFile);

		// タイマーはUIの更新だけを担当
		_updateTimer = new System.Timers.Timer(TIMER_INTERVAL);
		_updateTimer.Elapsed += (_, _) =>
		{
			Dispatcher.UIThread.Post(() =>
			{
				if (_audioPlayer.IsPlaying)
				{
					this.RaisePropertyChanged(nameof(CurrentTime));
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


	// バック
	private void SkipBackward(int seconds)
	{
		double time = _audioPlayer.CurrentTime.TotalMilliseconds + (seconds * 1000);
		if (time < 0) {
			time = 0;
		} else if (time > _audioPlayer.TotalTime.TotalMilliseconds) {
			time = _audioPlayer.TotalTime.TotalMilliseconds;
		}
		CurrentTime = time;
		this.RaisePropertyChanged(nameof(CurrentTime));
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
			CurrentTime = 0;
			this.RaisePropertyChanged(nameof(CurrentTime));

			// ラベル表示
			SelectedVoiceFileLabel = Path.GetFileName(_selectedVoiceFile);

			// アルバムアート取得
			LoadAlbumArt(_selectedVoiceFile);
		}
		else
		{
			// エラー時の表示
			ElapsedLabel = "--:--";
			RemainingLabel = "--:--";
			SelectedVoiceFileLabel = "音声ファイルを読み込めませんでした";
			AlbumArt = _defaultAlbumArt;
		}
	}

	// アルバムアート取得処理
	private void LoadAlbumArt(string audioFilePath)
	{
		var albumArt = AlbumArtService.GetAlbumArt(audioFilePath);
		if (albumArt != null)
		{
			AlbumArt = albumArt;
		}
		else
		{
			AlbumArt = _defaultAlbumArt;
		}
	}

	// 音声次へ
	private void SkipAudioFile () {
		if (string.IsNullOrEmpty(_selectedVoiceFile))
			return;
			
		string nextFile = FileSequenceNavigator.NextAudioPath(_selectedVoiceFile);
		if (!string.IsNullOrEmpty(nextFile))
		{
			_selectedVoiceFile = nextFile;
			LoadAudioFile();
			if (!IsPlaying)
			{
				PlayAudioFile();
			}
		}
	}

	// 音声前へ
	private void PriviousAudioFile () {
		if (string.IsNullOrEmpty(_selectedVoiceFile))
			return;
			
		string prevFile = FileSequenceNavigator.PreviousAudioPath(_selectedVoiceFile);
		if (!string.IsNullOrEmpty(prevFile))
		{
			_selectedVoiceFile = prevFile;
			LoadAudioFile();
			if (!IsPlaying)
			{
				PlayAudioFile();
			}
		}
	}


	// 音声再生
	private void PlayAudioFile()
	{
		_audioPlayer.Play();
	}

	// 音声一時停止
	private void PauseAudioFile()
	{
		_audioPlayer.Pause();
	}

	// 録音トグル処理
	private async Task ToggleRecording()
	{
		if (IsRecording)
		{
			await PlayRecording();
		}
		else
		{
			StartRecording();
		}
	}

	// 録音再生
	private async Task PlayRecording()
	{
		string? recordedFile = _audioRecorder.StopRecording();
		_audioRecorder.Dispose();

		await Task.Delay(400);

		if (!string.IsNullOrEmpty(recordedFile))
		{
			// 録音が成功した場合、録音ファイルを読み込む
			_recordAudioPlayer.LoadFile(recordedFile);
			_recordAudioPlayer.Play();
		}
	}

	// 録音開始
	private void StartRecording()
	{
		// 再生中の場合停止
		_recordAudioPlayer.Stop();
		_recordAudioPlayer.Dispose();
		// 録音開始
		_audioRecorder.StartRecording();
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
    _audioRecorder?.Dispose();
		_recordAudioPlayer?.Dispose();
		_albumArt?.Dispose();
		_defaultAlbumArt?.Dispose();
	}
}
