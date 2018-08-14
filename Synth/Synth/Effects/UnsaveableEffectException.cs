﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthLib.Effects
{
    public class UnsaveableEffectException : Exception
    {
        public UnsaveableEffectException() : base("Cannot save effects of this type.")
        {

        }
    }
}
