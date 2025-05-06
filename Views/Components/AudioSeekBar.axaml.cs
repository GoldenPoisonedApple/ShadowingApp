using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ShadowingApp.Views.Components;

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
