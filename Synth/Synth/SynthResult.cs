﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using Stuff.Music;
using SynthLib.Oscillators;
using SynthLib.Effects;
using Stuff;
using SynthLib.MidiSampleProviders;
using SynthLib.Data;

namespace SynthLib
{
    public class SynthResult : ISampleProvider
    {
        private IMidiSampleProvider synthProvider;

        private IMidiSampleProvider newSynthProvider;

        public WaveFormat WaveFormat { get; private set; }

        public SynthData Data { get; }

        public float Gain { get; set; }

        public (float left, float right) MaxValue => synthProvider.MaxValue;

        public SynthResult(SynthData data)
        {
            Data = data;
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(data.SampleRate, 2);
            synthProvider = null;
            newSynthProvider = null;
            Gain = 1;
        }

        public void ReplaceSynthProvider(IMidiSampleProvider synthProvider)
        {
            newSynthProvider = synthProvider;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            try
            {
                if (newSynthProvider != null)
                {
                    lock (newSynthProvider)
                    {
                        synthProvider = newSynthProvider;
                        newSynthProvider = null;
                    }
                }
                if (synthProvider != null)
                {
                    synthProvider.Next(buffer, offset, count, Gain);
                }
            }
            catch(Exception e)
            {
                Data.Log.Log("exceptions", e);
                throw e;
            }

            return count;
        }
    }
}
