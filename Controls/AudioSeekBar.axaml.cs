using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ShadowingApp.Controls;

public partial class AudioSeekBar : UserControl
{
	public AudioSeekBar()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
