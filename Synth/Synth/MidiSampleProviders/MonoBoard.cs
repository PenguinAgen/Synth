﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using SynthLib.Board;
using Stuff.Music;
using System.Diagnostics;
using NAudio.Midi;
using SynthLib.Data;

namespace SynthLib.MidiSampleProviders
{
    public class MonoBoard : IMidiSampleProvider
    {
        private readonly ModuleBoard board;

        private readonly BoardTemplate boardTemplate;

        public int SampleRate { get; }

        public (float left, float right) MaxValue { get; private set; }

        private readonly float baseGlideSamples;

        private readonly float baseGlideTime;
        private readonly float glideSamples;

        private float destFreq;

        private float baseFreqPerSample;

        private List<int> currentTones;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="boardTemplate"></param>
        /// <param name="glideTime">Glide time in milliseconds</param>
        /// <param name="sampleRate"></param>
        public MonoBoard(BoardTemplate boardTemplate, float glideTime, SynthData data)
        {
            SampleRate = data.SampleRate;
            MaxValue = (0, 0);
            this.boardTemplate = boardTemplate;
            board = boardTemplate.CreateInstance(data);
            baseGlideTime = glideTime;
            glideSamples = baseGlideSamples = (glideTime * SampleRate / 1000);
            baseFreqPerSample = 10e8f;
            currentTones = new List<int>();
        }

        public IMidiSampleProvider Clone(SynthData data)
        {
            return new MonoBoard(boardTemplate, baseGlideTime, data);
        }

        private bool On => currentTones.Count != 0;

        public void HandleNoteOn(int noteNumber)
        {
            if (On)
            {
                if (currentTones.Contains(noteNumber))
                    currentTones.Remove(noteNumber);
                currentTones.Add(noteNumber);
                ChangeNote(noteNumber);
            }
            else
            {
                board.NoteOn(noteNumber);
                currentTones.Add(noteNumber);
                baseFreqPerSample = 0;
            }
        }

        public void HandleNoteOff(int noteNumber)
        {
            currentTones.Remove(noteNumber);
            if (On)
                ChangeNote(currentTones.Last());
            else
                board.NoteOff();
        }

        public void HandlePitchWheelChange(int pitch)
        {
            board.PitchWheelChange(pitch);
        }

        public void HandleControlChange(MidiController controller, int controllerValue)
        {
            board.ControllerChange(controller, controllerValue);
        }

        private void ChangeNote(int noteNumber)
        {
            Debug.Assert(On);
            destFreq = (float)Tone.FrequencyFromNote(noteNumber);
            baseFreqPerSample = (destFreq - board.BaseFrequency) / glideSamples;

        }

        public void Next(float[] buffer, int offset, int count, float gain)
        {
            for (int i = offset; i < count + offset; i += 2)
            {
                board.BaseFrequency += baseFreqPerSample / board.GlideModifier;
                if (baseFreqPerSample > 0 && board.BaseFrequency > destFreq || baseFreqPerSample < 0 && board.BaseFrequency < destFreq)
                {
                    baseFreqPerSample = 0;
                    board.BaseFrequency = destFreq;
                }
                (buffer[i], buffer[i + 1]) = board.Next();
                MaxValue = (Math.Max(MaxValue.left, Math.Abs(buffer[i])), Math.Max(MaxValue.right, Math.Abs(buffer[i + 1])));
            }
        }
    }
}
