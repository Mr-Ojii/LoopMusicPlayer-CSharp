﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NVorbis;
using ManagedBass;

namespace LoopMusicPlayer
{
	internal class Player : IDisposable
	{
		private VorbisReader reader = null;
		private int StreamHandle = -1;
		private StreamProcedure tSTREAMPROC = null;
		public readonly bool IsLoop = false;
		public readonly long LoopStart = 0;
		public readonly long LoopEnd = 0;
		public bool NextIsLoop = true;
		public int LoopCount
		{
			get;
			private set;
		} = 0;

		public TimeSpan LoopStartTime 
		{
			get 
			{
				double time = (LoopStart / (double)reader.SampleRate);
				int millisecond = (int)((time % 1) * 1000);
				int second = (int)(time % 60);
				int minute = (int)(time / 60) % 60;
				int hour = (int)(time / 3600) % 24;
				int day = (int)(time / 86400);
				return new TimeSpan(day, hour, minute, second, millisecond);
			}
		}

		public TimeSpan LoopEndTime
		{
			get
			{
				double time = (LoopEnd / (double)reader.SampleRate);
				int millisecond = (int)((time % 1) * 1000);
				int second = (int)(time % 60);
				int minute = (int)(time / 60) % 60;
				int hour = (int)(time / 3600) % 24;
				int day = (int)(time / 86400);
				return new TimeSpan(day, hour, minute, second, millisecond);
			}
		}


		public long TotalSamples
		{
			get 
			{
				return reader.TotalSamples;
			}
		}

		public long SamplePosition
		{
			get 
			{
				return reader.SamplePosition;
			}
		}

		public TimeSpan TotalTime
		{
			get 
			{
				return reader.TotalTime;
			}
		}

		public TimeSpan TimePosition
		{
			get 
			{
				return reader.TimePosition;
			}
		}

		public string Title 
		{
			get 
			{
				return !string.IsNullOrEmpty(reader.Tags.Title) ?reader.Tags.Title : System.IO.Path.GetFileName(FilePath);
			}
		}

		public string Artist
		{
			get
			{
				return !string.IsNullOrEmpty(reader.Tags.Artist) ? reader.Tags.Artist : "";
			}
		}

		public event EventHandler LoopAction;

		public readonly string FilePath;

		public Player(string filepath, double volume)
		{
			this.FilePath = filepath;
			this.reader = new VorbisReader(filepath);

			this.IsLoop = !string.IsNullOrEmpty(reader.Tags.GetTagSingle("LOOPSTART")) && (!string.IsNullOrEmpty(reader.Tags.GetTagSingle("LOOPLENGTH")) || !string.IsNullOrEmpty(reader.Tags.GetTagSingle("LOOPEND")));
			if (this.IsLoop)
			{
				LoopStart = long.Parse(reader.Tags.GetTagSingle("LOOPSTART"));
				if (!string.IsNullOrEmpty(reader.Tags.GetTagSingle("LOOPLENGTH")))
				{
					LoopEnd = LoopStart + long.Parse(reader.Tags.GetTagSingle("LOOPLENGTH"));
				}
				else
				{
					LoopEnd = long.Parse(reader.Tags.GetTagSingle("LOOPEND"));
				}
			}
			else 
			{
				LoopStart = 0;
				LoopEnd = reader.TotalSamples;
			}

			this.tSTREAMPROC = new StreamProcedure(this.StreamProc);

			this.StreamHandle = Bass.CreateStream(reader.SampleRate, reader.Channels, BassFlags.Float, this.tSTREAMPROC);

			ChangeVolume(volume);

			Bass.ChannelPlay(this.StreamHandle);
		}

		public void Seek(long sample) 
		{
			PlaybackState state = Bass.ChannelIsActive(this.StreamHandle);
			Bass.ChannelPause(this.StreamHandle);
			this.reader.SamplePosition = sample;

			this.NextIsLoop = sample < this.LoopEnd;

			if (state == PlaybackState.Playing) 
				Bass.ChannelPlay(this.StreamHandle);
		}

		public void ChangeVolume(double volume)//0~1
		{
			Bass.ChannelSetAttribute(this.StreamHandle, ChannelAttribute.Volume, volume);
		}

		public void Play()
		{
			this.reader.SamplePosition = 0;
			Bass.ChannelPlay(this.StreamHandle);
		}

		public void Pause() 
		{
			switch (Bass.ChannelIsActive(this.StreamHandle)) 
			{
				case PlaybackState.Paused:
					Bass.ChannelPlay(this.StreamHandle);
					break;
				case PlaybackState.Playing:
					Bass.ChannelPause(this.StreamHandle);
					break;
				default:
					break;
			}
		}

		public void Stop() 
		{
			Bass.ChannelStop(this.StreamHandle);
			this.reader.SamplePosition = 0;
		}

		public bool CheckDeviceEnable() 
		{
			if (Bass.GetDeviceInfo(Bass.CurrentDevice, out DeviceInfo info))
			{
				return info.IsEnabled;
			}
			return false;
		}

		public int StreamProc(int handle, IntPtr buffer, int length, IntPtr user)
		{
			int num;
			int floatlength = (int)((double)sizeof(byte) / sizeof(float) * length);
			float[] tmp = new float[floatlength];

			if (NextIsLoop && reader.SamplePosition + floatlength > LoopEnd)
			{
				int tmplength = (int)(LoopEnd - reader.SamplePosition);
				num = reader.ReadSamples(tmp, 0, tmplength);
				reader.SamplePosition = LoopStart;
				num = reader.ReadSamples(tmp, tmplength, floatlength - tmplength);
				num = length;
				this.LoopCount++;
			}
			else
			{
				num = reader.ReadSamples(tmp, 0, tmp.Length) * (int)((double)sizeof(float) / sizeof(byte));
			}

			if (num < 0) num = 0;

			unsafe
			{
				fixed (float* point = tmp)
					Buffer.MemoryCopy(point, (float*)buffer, length, num);
			}

			return num;
		}

		public void Dispose() 
		{
			if(this.StreamHandle != -1){
				Bass.ChannelStop(this.StreamHandle);
				Bass.StreamFree(this.StreamHandle);
			}
			reader?.Dispose();
		}
	}
}
