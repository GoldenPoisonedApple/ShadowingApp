using System.Threading.Tasks;
using ReactiveUI;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace ShadowingApp.ViewModels;


public partial class MainWindowViewModel : ViewModelBase
{
	// プライベートフィールドを追加
	private string _debugText = "Welcome to Avalonia!";

	// プロパティのゲッターとセッターを定義
	public string DebugText
	{
		get => _debugText;
		private set
		{
			_debugText = value;
			this.RaisePropertyChanged(nameof(DebugText));
		}
	}

	// コマンドを宣言
	public ICommand SelectedVoice { get; }

	// コンストラクタ
	public MainWindowViewModel()
	{
		// ReactiveCommandを使用してコマンドを作成
		SelectedVoice = ReactiveCommand.CreateFromTask(SelectVoiceFile);
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
					DebugText = $"選択されたファイル: {filePath}";
				}
				else
				{
					DebugText = "ファイルが選択されませんでした";
				}
			}
			else
			{
				DebugText = "ストレージプロバイダーが利用できません";
			}
		}
	}
}