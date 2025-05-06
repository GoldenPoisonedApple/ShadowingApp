using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ShadowingApp.Controls;

public partial class AudioControls : UserControl
{
	public AudioControls()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}
