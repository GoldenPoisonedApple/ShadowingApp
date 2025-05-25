using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ShadowingApp.Views.Components;

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
