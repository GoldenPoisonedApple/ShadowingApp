using System.Threading.Tasks;
using ReactiveUI;
using System.Windows.Input;
using System.Reactive.Linq;

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
		SelectedVoice = ReactiveCommand.CreateFromTask(async () =>
		{
			await TestFunc(); // コマンド実行時の処理を呼び出す
			DebugText = "Selected Voice Executed!"; // セッターを通じて変更通知も発行
		});
	}

	// コマンドの実行内容を定義
	private async Task TestFunc()
	{
		// ここに非同期処理を記述
		await Task.Delay(1000); // 1秒待機
		DebugText = "TestFunc executed!"; // デバッグテキストを更新
	}
}