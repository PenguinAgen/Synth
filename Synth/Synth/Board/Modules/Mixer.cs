﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthLib.Board.Modules
{
    /// <summary>
    /// Applies the specified gain to each input and outputs the total to every output adjusted for output gain.
    /// </summary>
    public class Mixer : Module
    {
        public override Connections Inputs { get; }

        public override Connections Outputs { get; }

        public float[] InputGains { get; }

        public float[] OutputGains { get; }

        public override string Type { get; } = "Mixer";

        public Mixer(int inputs, int outputs)
        {
            Inputs = new ConnectionsArray(inputs);
            Outputs = new ConnectionsArray(outputs);
            InputGains = new float[inputs];
            for (int i = 0; i < inputs; ++i)
            {
                InputGains[i] = 1;
            }
            OutputGains = new float[outputs];
            for (int i = 0; i < outputs; ++i)
            {
                OutputGains[i] = 1;
            }
        }

        public Mixer(float[] inputGains, float[] outputGains)
        {
            Inputs = new ConnectionsArray(inputGains.Length);
            Outputs = new ConnectionsArray(outputGains.Length);
            InputGains = new float[inputGains.Length];
            inputGains.CopyTo(InputGains, 0);
            OutputGains = new float[outputGains.Length];
            outputGains.CopyTo(OutputGains, 0);
        }

        public override float[] Process(float[] inputs)
        {
            var totalInput = 0f;
            for (int i = 0; i < inputs.Length; ++i)
                totalInput += inputs[i] * InputGains[i];

            var result = new float[Outputs.Count];
            for (int i = 0; i < result.Length; ++i)
                result[i] = totalInput * OutputGains[i];

            return result;
        }
    }
}