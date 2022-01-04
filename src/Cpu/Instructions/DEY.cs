using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class DEY_Instruction : Instruction
    {
        public DEY_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "Decrements the value in the Y register by one.";

        public override void Execute(Cpu cpu)
        {
            byte result = 0;

            unchecked
            {
                result = (byte)(cpu.Y - (byte)1);
            }

            cpu.Y = result;
            cpu.SetZeroFlagIfRequired(result);
            cpu.SetNegativeFlagIfRequired(result);
            cpu.SetTimingControl(machineCycles);
            cpu.IncrementPC();
        }
    }
}