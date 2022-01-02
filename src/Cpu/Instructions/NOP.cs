using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cpu.Instructions
{
    public class NOP_Instruction : Instruction
    {
        public NOP_Instruction(Cpu cpu, string mnemonic, byte hexCode, AddressingMode addressingMode, byte instructionBytes, byte machineCycles) : base(cpu, mnemonic, hexCode, addressingMode, instructionBytes, machineCycles)
        {
        }

        public override string Description => "No operation";

        public override void Execute(Cpu cpu)
        {
            cpu.IncrementPC(); //don't fetch as this is always implied mode
            cpu.SetTimingControl(machineCycles);
            cpu.IncrementPC();
        }
    }
}
