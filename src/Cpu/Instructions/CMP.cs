﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class CMP_Instruction : Instruction
    {
        public CMP_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "Subtracts the contents of memory from the contents of the accumulator.";

        public override void Execute(Cpu cpu)
        {
            FetchResult fr = cpu.Fetch(this.addressingMode);
            byte result = (byte)(cpu.A - fr.operand);

            //Set carry flag
            if (fr.operand <= cpu.A)
            {
                cpu.SetProcessorStatusFlag(true, StatusFlagName.Carry);
            }
            else
            {
                cpu.SetProcessorStatusFlag(false, StatusFlagName.Carry);
            }

            cpu.SetZeroFlagIfRequired(result);
            cpu.SetNegativeFlagIfRequired(result);
            cpu.SetTimingControl(machineCycles + fr.pageCross);
            cpu.IncrementPC();
        }
    }
}
