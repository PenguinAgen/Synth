﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthLib.Effects
{
    public interface IEffect
    {
        float Next(float input);
    }
}
