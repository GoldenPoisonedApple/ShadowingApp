using NAudio.Wave;
using System;
using System.Diagnostics;
using System.IO;

namespace ShadowingApp.Services;

/// <summary>
/// 音声再生
/// </summary>
public class AudioPlayerService : IDisposable
{
	private IWavePlayer? _waveOutDevice;
	private AudioFileReader? _audioFileReader;

	public event EventHandler? PlaybackStateChanged;

	public PlaybackState PlaybackState => _waveOutDevice?.PlaybackState ?? PlaybackState.Stopped;
	public bool IsPlaying => PlaybackState == PlaybackState.Playing;

	public TimeSpan CurrentTime
	{
		get => _audioFileReader?.CurrentTime ?? TimeSpan.Zero;
		set
		{
			if (_audioFileReader != null)
			{
				_audioFileReader.CurrentTime = value;
			}
		}
	}

	public TimeSpan TotalTime => _audioFileReader?.TotalTime ?? TimeSpan.Zero;

	public bool LoadFile(string filePath)
	{
		if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
			return false;

		try
		{
			Stop();
			_waveOutDevice?.Dispose();
			_audioFileReader?.Dispose();

			_waveOutDevice = new WaveOutEvent();
			_audioFileReader = new AudioFileReader(filePath);
			_waveOutDevice.Init(_audioFileReader);

			_waveOutDevice.PlaybackStopped += (s, e) =>
			{
				PlaybackStateChanged?.Invoke(this, EventArgs.Empty);
			};

			PlaybackStateChanged?.Invoke(this, EventArgs.Empty);
			return true;
		}
		catch
		{
			return false;
		}
	}

	public void Play()
	{
		if (_waveOutDevice != null && _audioFileReader != null)
		{
			_waveOutDevice.Play();
			PlaybackStateChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	public void Pause()
	{
		_waveOutDevice?.Pause();
		PlaybackStateChanged?.Invoke(this, EventArgs.Empty);
	}

	public void Stop()
	{
		_waveOutDevice?.Stop();
		if (_audioFileReader != null)
		{
			_audioFileReader.Position = 0;
		}
		PlaybackStateChanged?.Invoke(this, EventArgs.Empty);
	}

	public void Dispose()
	{
		_waveOutDevice?.Stop();
		_waveOutDevice?.Dispose();
		_audioFileReader?.Dispose();
		_waveOutDevice = null;
		_audioFileReader = null;
	}
}
