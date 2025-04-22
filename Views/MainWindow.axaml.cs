using Avalonia.Controls;
using ShadowingApp.ViewModels;


namespace ShadowingApp.Views;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();

		// ウィンドウクローズイベントを購読
		this.Closing += MainWindow_Closing;
	}

	private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
	{
		// ViewModelを取得して、OnWindowClosingメソッドを呼び出す
		if (this.DataContext is MainWindowViewModel viewModel)
		{
			viewModel.OnWindowClosing();
		}
	}

}