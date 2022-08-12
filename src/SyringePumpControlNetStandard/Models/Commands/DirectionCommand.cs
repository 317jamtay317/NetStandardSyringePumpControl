﻿using SyringePumpControlNetStandard.Annotations.Extensions;

namespace SyringePumpControlNetStandard.Models.Commands
{
    public class DirectionCommand:ValueCommand<string>
    {
        public DirectionCommand(int address, PumpDirection direction) : base(address)
        {
            Message = "DIR";

            Value = direction.ToCommandString();
        }

        public override string Message { get; }
    }
}