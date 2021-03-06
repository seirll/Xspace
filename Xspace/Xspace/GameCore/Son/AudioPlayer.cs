﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FMOD;
using System.Runtime.InteropServices;


/* Interface entre le jeu et l'API Fmod */

namespace Xspace
{
    public delegate void EndMusicEventHandler(object sender, EventArgs e);

    public static class AudioPlayer
    {
        private static FMOD.System system = null;

        private static Sound music = null;
        private static string currentMusicPath;
        private static Channel musicChannel = null;
        private static uint length;
        private static int[] data_channel;
        private static float baseFrequency, current_speed;
        private static FMOD.CHANNEL_CALLBACK channelCallback;
        public static event EndMusicEventHandler EndMusic;

        private static List<float> history_frequencies = new List<float>();

        private static void ErrCheck(RESULT result)
        {
            if (result != RESULT.OK)
            {
                throw new ApplicationException("FMOD : " + result + " - " + Error.String(result));
            }
        }

        public static bool Initialize()
        {
            RESULT result;

            result = Factory.System_Create(ref system);
            ErrCheck(result);

            uint version = 0;
            length = 0;
            result = system.getVersion(ref version);
            ErrCheck(result);

            if (version < VERSION.number)
            {
                throw new ApplicationException("Error! You are using an old version of FMOD " + version.ToString("X") + ". This program requires " + VERSION.number.ToString("X") + ".");
            }

            result = system.init(32, INITFLAGS.NORMAL, (IntPtr)null);
            ErrCheck(result);

            channelCallback = new CHANNEL_CALLBACK(OnEndMusic);

            return true;
        }

        public static void Deinitialize()
        {
            if (music != null)
            {
                music.release();
            }

            system.release();
        }

        public static void PlayMusic()
        {
            PlayMusic(currentMusicPath);
        }

        public static void PlayMusic(string path, int loop = -1, bool paused = false)
        {
            bool isPlaying = false;
            RESULT result;

            if (musicChannel != null)
            {
                result = musicChannel.isPlaying(ref isPlaying);
            }
            else
            {
                isPlaying = false;
            }

            if (currentMusicPath == path && isPlaying)
            {
                return;
            }
            else if (currentMusicPath == path)
            {
                result = system.playSound(CHANNELINDEX.FREE, music, false, ref musicChannel);
                ErrCheck(result);
            }
            else
            {
                if (music != null)
                {
                    result = music.release();
                    ErrCheck(result);
                }

                result = system.createSound(path, MODE.SOFTWARE | MODE.LOOP_NORMAL | MODE._2D, ref music);
                ErrCheck(result);
                music.setLoopCount(loop);
                music.getLength(ref length, TIMEUNIT.PCM);
                
                /* Shitstorm incoming */
                IntPtr ptr1 = IntPtr.Zero;
                IntPtr ptr2 = IntPtr.Zero;
                uint len1 = 0;
                uint len2 = 0;
                data_channel = new int[length];
                result = music.@lock(0, length, ref ptr1, ref ptr2, ref len1, ref len2);
                ErrCheck(result);
                try
                {
                    Marshal.Copy(ptr1, data_channel, 0, (int)length);
                }
                catch (AccessViolationException)
                {
                    throw new ApplicationException("Cannot read the music file.");
                }

                for (int i = 0; i < length; i++)
                {
                    data_channel[i] = data_channel[i] >> 16;
                }
                music.@unlock(ptr1, ptr2, len1, len2);
                result = system.playSound(CHANNELINDEX.FREE, music, paused, ref musicChannel);
                ErrCheck(result);

                musicChannel.getFrequency(ref baseFrequency);
                musicChannel.setCallback(channelCallback);
                
                currentMusicPath = path;

            }
        }

        public static RESULT OnEndMusic(IntPtr channelraw, CHANNEL_CALLBACKTYPE type, IntPtr commanddata1, IntPtr commanddata2)
        {
            if (EndMusic != null)
                EndMusic(currentMusicPath, new EventArgs());
            musicChannel = null;
            return RESULT.OK;
        }

        public static void StopMusic()
        {
            if (musicChannel != null)
            {
                RESULT result = musicChannel.stop();
                musicChannel = null;
                ErrCheck(result);
            }
        }

        public static void PauseMusic()
        {
            bool paused = false;
            if (musicChannel != null)
            {
                RESULT result = musicChannel.getPaused(ref paused);
                ErrCheck(result);
                result = musicChannel.setPaused(!paused);
                ErrCheck(result);
            }
        }

        public static void SetVolume(float volume)
        {
            if (musicChannel != null)
            {
                RESULT result = musicChannel.setVolume(volume);
                ErrCheck(result);
            }
        }

        public static void Update()
        {
            system.update();
        }

        public static float[] GetSpectrum(int nb_values)
        {
            float[] spectre = new float[nb_values];
            if (musicChannel != null)
            {
                RESULT result = musicChannel.getSpectrum(spectre, nb_values, 0, DSP_FFT_WINDOW.RECT);
                ErrCheck(result);
            }
            return spectre;
        }

        public static float GetFreq()
        {
            float freq = 0;
            if (musicChannel != null)
            {
                RESULT result = musicChannel.getFrequency(ref freq);
                ErrCheck(result);
            }

            return freq;
        }

        public static float GetEnergy()
        {
            float[] now = GetSpectrum(64);
            return now.Sum();
        }

        public static uint GetLength()
        {
            return length;
        }

        public static int[] GetMusicData()
        {
            return data_channel;
        }

        public static uint GetCurrentTime()
        {
            uint pos = 0;
            musicChannel.getPosition(ref pos, TIMEUNIT.PCM);
            return pos;
        }

        public static void SetCurrentTime(uint pcm)
        {
            musicChannel.setPosition(pcm, TIMEUNIT.PCM);
        }

        public static bool IsPlaying()
        {
            bool isPlaying = false;
            if (musicChannel != null)
                musicChannel.isPlaying(ref isPlaying);
            return isPlaying;
        }

        public static void SetSpeed(float speed)
        {
            current_speed = speed;
            musicChannel.setFrequency(baseFrequency * speed);
        }

        public static void DownSpeed()
        {
            if (current_speed >= 0.3)
            {
                SetSpeed(current_speed - 0.1f);
            }
        }

        public static void UpSpeed()
        {
            if (current_speed <= 1.7)
            {
                SetSpeed(current_speed + 0.1f);
            }
        }
    }
}

