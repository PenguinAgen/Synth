﻿using Stuff;
using SynthLib.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SynthLib.Board.Modules
{
    /// <summary>
    /// Returns a value between -1 and 0 as long as the given values are between 0 and 1.
    /// </summary>
    public class Envelope : Module
    {
        public int SampleRate { get; }

        public int Attack { get; }

        public int Decay { get; }

        private readonly int sustainStart;

        public float Sustain { get; }

        public int Release { get; }

        private readonly float releaseIncrement;

        private readonly float[] onValues;

        private readonly float[] offValues;

        private float curValue;

        public override Connections Inputs { get; }

        public override Connections Outputs { get; }

        public override BoardOutput OutputType => BoardOutput.None;

        private float[] output;

        public Envelope()
        {
            useable = false;
        }

        public Envelope(int attack, int decay, float sustain, int release, int outputs, int sampleRate = 44100)
        {
            SampleRate = sampleRate;
            Attack = attack;
            Decay = decay;
            sustainStart = Attack + Decay;
            Sustain = sustain;
            Release = release;
            releaseIncrement = (-Sustain / Release) / (SampleRate / 1000);

            curValue = -1;

            Inputs = new ConnectionsArray(0);
            Outputs = new ConnectionsArray(outputs);

            onValues = CalculateOnValues();
            offValues = CalculateOffValues();

            output = new float[Outputs.Count];
        }

        public Envelope(int outputs, int sampleRate = 44100) : this(0, 0, 1, 0, outputs, sampleRate)
        {

        }

        public Envelope(XElement element, SynthData data)
        {
            SampleRate = data.SampleRate;
            Attack = InvalidModuleSaveElementException.ParseInt(element.Element("attack"));
            Decay = InvalidModuleSaveElementException.ParseInt(element.Element("decay"));
            sustainStart = Attack + Decay;
            Sustain = InvalidModuleSaveElementException.ParseFloat(element.Element("sustain"));
            Release = InvalidModuleSaveElementException.ParseInt(element.Element("release"));
            releaseIncrement = (-Sustain / Release) / (SampleRate / 1000);

            curValue = -1;

            Inputs = new ConnectionsArray(element.Element("inputs"));
            Outputs = new ConnectionsArray(element.Element("outputs"));

            onValues = CalculateOnValues();
            offValues = CalculateOffValues();

            output = new float[Outputs.Count];
        }

        private float[] CalculateOnValues()
        {
            float[] result = new float[sustainStart];
            for (int t = 0; t < Attack; ++t)
                result[t] = ((float)t / Attack) - 1;
            for (int t = Attack; t < sustainStart; ++t)
                result[t] = 0 - ((float)(t - Attack) / Decay) * (1 - Sustain);
            return result;
        }

        private float[] CalculateOffValues()
        {
            float[] result = new float[Release];
            int t = -1;
            while (++t < Release)
                result[t] = Sustain - ((float)t / Release) * Sustain - 1;
            return result;
        }

        public override Module Clone(int sampleRate = 44100)
        {
            return new Envelope(Attack, Decay, Sustain, Release, Outputs.Count, SampleRate);
        }

        public override Module CreateInstance(XElement element, SynthData data)
        {
            return new Envelope(element, data);
        }

        protected override float[] IntProcess(float[] inputs, long time, bool noteOn, ModuleBoard moduleBoard)
        {
            if (noteOn)
            {
                if (time < sustainStart)
                    curValue = onValues[time];
                else
                    curValue = Sustain - 1;
            }
            else
            {
                curValue += releaseIncrement;
                if (curValue < -1)
                    curValue = -1;
            }
            for (int i = 0; i < output.Length; ++i)
                output[i] = curValue;
            return output;
        }

        public override XElement ToXElement(string name)
        {
            var element = base.ToXElement(name);
            element.AddValue("attack", Attack);
            element.AddValue("decay", Decay);
            element.AddValue("sustain", Sustain);
            element.AddValue("release", Release);
            return element;
        }
    }
}
