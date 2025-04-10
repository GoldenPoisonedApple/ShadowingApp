using NAudio.Wave;

namespace ShadowingApp;

public partial class Form1 : Form
{
	// ボタン 
	private Button selectAudioButton = null!;
	private Button playButton = null!;

	// ラベル
	private Label selectedAudioLabel = null!;

	// 音声ファイルパス
	private string audioFilePath = string.Empty;

	// NAudio
	private WaveOutEvent? outputDevice; // 再生デバイス
	private AudioFileReader? audioFile; // 音声ファイル


	public Form1()
	{
		Text = "音声アプリ";
		Width = 400;
		Height = 200;
		InitializeComponents();
	}

	private void InitializeComponents()
	{
		// 音声ファイル選択ボタン
		selectAudioButton = new Button
		{
			Text = "音声ファイル選択",
			Left = 30,
			Top = 10,
			Width = 120
		};
		// ハンドラーを追加
		selectAudioButton.Click += SelectButton_Click;

		// 選択音声表示ラベル
		selectedAudioLabel = new Label
		{
			Text = "音声ファイルを選択してください",
			Left = 160,
			Top = 10,
			Width = 200
		};

		// 再生ボタン
		playButton = new Button
		{
			Text = "再生",
			Left = 120,
			Top = 50,
			Width = 80
		};
		// ハンドラーを追加
		playButton.Click += PlayButton_Click;


		Controls.Add(selectAudioButton);
		Controls.Add(selectedAudioLabel);
		Controls.Add(playButton);
	}


	/// <summary>
	/// 音声ファイル選択ボタンのクリックイベント
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void SelectButton_Click(object? sender, EventArgs e)
	{
		using (var openFileDialog = new OpenFileDialog())
		{
			openFileDialog.Filter = "音声ファイル (*.wav; *.mp3)|*.wav; *.mp3|すべてのファイル (*.*)|*.*";
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				// 選択された音声ファイルのパスを取得
				audioFilePath = openFileDialog.FileName;
				// ラベルの幅を調整
				string fileName = Path.GetFileName(audioFilePath);
				UpdateLabelWidth(fileName);
				// ラベルに選択された音声ファイル名を表示
				selectedAudioLabel.Text = Path.GetFileName(audioFilePath);
			}
		}
	}

	/// <summary>
	/// 再生ボタンのクリックイベント
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void PlayButton_Click(object? sender, EventArgs e)
	{		
		// 音声ファイルが選択されていない場合はエラーメッセージを表示
		if (string.IsNullOrEmpty(audioFilePath))
		{
			MessageBox.Show("音声ファイルが選択されていません。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return;
		}

		try
		{
			// 既存の再生デバイスを停止して破棄
			outputDevice?.Stop();
			outputDevice?.Dispose();
			audioFile?.Dispose();

			// 新しい再生デバイスと音声ファイルを作成
			audioFile = new AudioFileReader(audioFilePath);
			outputDevice = new WaveOutEvent();
			outputDevice.Init(audioFile);
			outputDevice.Play();

			// 表示変更
			playButton.Text = "一時停止";
			// ハンドラ変更
			playButton.Click -= PlayButton_Click; // 既存のイベントハンドラーを削除
			playButton.Click += PauseButton_Click; // 停止ボタンのイベントハンドラーを追加
		}
		catch (Exception ex)
		{
			MessageBox.Show($"音声の再生に失敗しました: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	/// <summary>
	/// 一時停止ボタンのクリックイベント
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void PauseButton_Click(object? sender, EventArgs e)
	{
		try {
		outputDevice.Pause();
		playButton.Text = "再生";
		playButton.Click -= PauseButton_Click; // 既存のイベントハンドラーを削除
		playButton.Click += PlayButton_Click; // 再生ボタンのイベントハンドラーを追加
		}
		catch (Exception ex)
		{
			MessageBox.Show($"音声の一時停止に失敗しました: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

	}

	/// <summary>
	/// ラベルの幅を文字列に合わせて調整するメソッド
	/// </summary>
	/// <param name="text"></param>
	private void UpdateLabelWidth(string text)
	{
		selectedAudioLabel.Text = text;

		// ラベルの幅を文字列に合わせて調整
		using (Graphics g = selectedAudioLabel.CreateGraphics())
		{
			SizeF textSize = g.MeasureString(text, selectedAudioLabel.Font);
			selectedAudioLabel.Width = (int)textSize.Width + 10; // 余白を少し追加
		}
	}
}
