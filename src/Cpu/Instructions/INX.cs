using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class INX_Instruction : Instruction
    {
        public INX_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "Increments the value in the X register by one.";

        public override void Execute(Cpu cpu)
        {
            byte result = 0;

            unchecked
            {
                result = (byte)(cpu.X + (byte)1);
            }

            cpu.X = result;
            cpu.SetZeroFlagIfRequired(result);
            cpu.SetNegativeFlagIfRequired(result);
            cpu.SetTimingControl(machineCycles);
            cpu.IncrementPC();
        }
    }
}