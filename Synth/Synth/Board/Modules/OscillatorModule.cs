﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SynthLib.Oscillators;
using SynthLib.Music;
using SynthLib.ValueProviders;

namespace SynthLib.Board.Modules
{
    public class OscillatorModule : Module
    {
        private readonly IOscillator oscillator;

        private readonly float gain;

        private readonly float frequencyMultiplier;

        private readonly Midi midi;

        public override Connections Inputs { get; }

        public override Connections Outputs { get; }

        public override string Type { get; } = "Oscillator";

        public OscillatorModule(IOscillator oscillator, Midi midi, int outputs, float halfToneOffset = 0, float gain = 1f)
        {
            this.oscillator = oscillator.Clone();
            frequencyMultiplier = (float) Math.Pow(2, (1 / 12d) * halfToneOffset);
            this.gain = gain;
            Inputs = new ConnectionsArray(0);
            Outputs = new ConnectionsArray(outputs);
            this.midi = midi;
        }

        private OscillatorModule(OscillatorModule oscMod)
        {
            oscillator = oscMod.oscillator.Clone();
            gain = oscMod.gain;
            frequencyMultiplier = oscMod.frequencyMultiplier;
            midi = oscMod.midi;
            Inputs = new ConnectionsArray(0);
            Outputs = new ConnectionsArray(oscMod.Outputs.Count);
            Type = oscMod.Type;
        }

        public override void Reset()
        {
            base.Reset();
            oscillator.Reset();
        }

        public override Module Clone()
        {
            return new OscillatorModule(this);
        }

        public override float[] Process(float[] inputs, float frequency)
        {
            double tempFrequency;
            if (midi.CurrentNoteNumbers.Count() > 0)
                tempFrequency = Tone.FrequencyFromNote(midi.CurrentNoteNumbers.Last());
            else
                tempFrequency = 0;
            oscillator.Next(tempFrequency * frequencyMultiplier);

            var output = new float[Outputs.Count];
            if (midi.CurrentNoteNumbers.Count() == 0)
                return output;
            var next = oscillator.CurrentValue();
            for (int i = 0; i < output.Length; ++i)
                output[i] = next * gain;
            return output;
        }
    }
}
