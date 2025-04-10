namespace ShadowingApp;

public partial class Form1 : Form
{
	private Button recordButton = null!;
	private Button playButton = null!;
	private Button skipButton = null!;
	private Button backButton = null!;

	public Form1()
	{
		Text = "音声アプリ";
		Width = 400;
		Height = 200;
		InitializeComponents();
	}

	private void InitializeComponents()
	{
		// 録音ボタン
		recordButton = new Button
		{
			Text = "録音",
			Left = 30,
			Top = 50,
			Width = 80
		};
		recordButton.Click += (sender, e) => MessageBox.Show("録音開始（仮）");

		// 再生ボタン
		playButton = new Button
		{
			Text = "再生",
			Left = 120,
			Top = 50,
			Width = 80
		};
		playButton.Click += (sender, e) => MessageBox.Show("再生（仮）");

		// 10秒バック
		backButton = new Button
		{
			Text = "←10秒",
			Left = 210,
			Top = 50,
			Width = 70
		};
		backButton.Click += (sender, e) => MessageBox.Show("10秒戻る（仮）");

		// 10秒スキップ
		skipButton = new Button
		{
			Text = "10秒→",
			Left = 290,
			Top = 50,
			Width = 70
		};
		skipButton.Click += (sender, e) => MessageBox.Show("10秒進む（仮）");

		Controls.Add(recordButton);
		Controls.Add(playButton);
		Controls.Add(backButton);
		Controls.Add(skipButton);
	}
}
